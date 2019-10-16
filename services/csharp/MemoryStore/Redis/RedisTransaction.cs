using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Improbable.OnlineServices.DataModel;
using StackExchange.Redis;

namespace MemoryStore.Redis
{
    public class RedisTransaction : ITransaction
    {
        private readonly StackExchange.Redis.ITransaction _transaction;
        private readonly LoadedLuaScript _zpopMinScript;
        private readonly Dictionary<string, ConditionResult> _notExistsChecks;
        private readonly Dictionary<string, ConditionResult> _existsChecks;
        private readonly Dictionary<string, ConditionResult> _preconditionChecks;
        private ConditionResult _lengthCondition;

        public RedisTransaction(StackExchange.Redis.ITransaction transaction, LoadedLuaScript zpopMinScript)
        {
            _transaction = transaction;
            _zpopMinScript = zpopMinScript;
            _notExistsChecks = new Dictionary<string, ConditionResult>();
            _existsChecks = new Dictionary<string, ConditionResult>();
            _preconditionChecks = new Dictionary<string, ConditionResult>();
        }

        public void CreateAll(IEnumerable<Entry> entries)
        {
            foreach (var entry in entries)
            {
                var key = Key.ForEntry(entry);
                _notExistsChecks.Add(key, _transaction.AddCondition(Condition.KeyNotExists(key)));
                _transaction.StringSetAsync(key, entry.SerializeToJson());
            }
        }

        public void EnqueueAll(IEnumerable<QueuedEntry> entries)
        {
            foreach (var entry in entries)
            {
                _transaction.SortedSetAddAsync(Key.ForQueue(entry.QueueName), entry.Id, entry.Score);
            }
        }

        public async Task<IEnumerable<string>> DequeueAsync(string queue, uint number)
        {
            var queueKey = Key.ForQueue(queue);
            _lengthCondition = _transaction.AddCondition(Condition.SortedSetLengthGreaterThan(queueKey, number - 1));
            var results = await _transaction.ScriptEvaluateAsync(_zpopMinScript,
                new { key = (RedisKey) queueKey, count = $"{number}" });
            var returned = new List<string>();
            foreach (var r in (RedisResult[]) results)
            {
                var arr = (RedisResult[]) r;
                if (arr.Length != 2) continue;
                returned.Add((string) arr[0]);
            }

            return returned;
        }

        public void RemoveAllFromQueue(IEnumerable<QueuedEntry> entries)
        {
            foreach (var entry in entries)
            {
                _transaction.SortedSetRemoveAsync(Key.ForQueue(entry.QueueName), entry.Id);
            }
        }

        public void DeleteAll(IEnumerable<Entry> entries)
        {
            foreach (var entry in entries)
            {
                var key = Key.ForEntry(entry);
                _existsChecks.Add(key, _transaction.AddCondition(Condition.KeyExists(key)));
                _transaction.AddCondition(Condition.StringEqual(key, entry.PreviousState));
                _transaction.KeyDeleteAsync(key);
            }
        }

        public void UpdateAll(IEnumerable<Entry> entries)
        {
            foreach (var entry in entries)
            {
                var key = Key.ForEntry(entry);
                _existsChecks.Add(key, _transaction.AddCondition(Condition.KeyExists(key)));
                _transaction.AddCondition(Condition.StringEqual(key, entry.PreviousState));
                _transaction.StringSetAsync(key, entry.SerializeToJson());
            }
        }

        public void CreateHashWithEntries(string hash, IEnumerable<KeyValuePair<string, string>> hashEntries)
        {
            // Ensure the hash doesn't exist.
            _notExistsChecks.Add(hash, _transaction.AddCondition(Condition.HashLengthEqual(hash, 0)));
            _transaction.HashSetAsync(hash,
                hashEntries.Select(entry => new HashEntry(entry.Key, entry.Value)).ToArray());
        }

        public void UpdateHashWithEntries(string hash, IEnumerable<KeyValuePair<string, string>> hashEntries)
        {
            foreach (var (key, value) in hashEntries)
            {
                if (string.IsNullOrEmpty(value))
                {
                    _transaction.HashDeleteAsync(hash, key);
                }
                else
                {
                    _transaction.HashSetAsync(hash, key, value);
                }
            }
        }

        public void DeleteKey(string key)
        {
            _existsChecks.Add(key, _transaction.AddCondition(Condition.KeyExists(key)));
            _transaction.KeyDeleteAsync(key);
        }

        public void DeleteHashEntry(string key, string hashField)
        {
            _existsChecks.Add(key, _transaction.AddCondition(Condition.KeyExists(key)));
            _existsChecks.Add($"{key}:{hashField}", _transaction.AddCondition(Condition.HashExists(key, hashField)));
            _transaction.HashDeleteAsync(key, hashField);
        }

        #region Conditions

        public void AddListEmptyCondition(string list)
        {
            _notExistsChecks.Add(list, _transaction.AddCondition(Condition.ListLengthEqual(list, 0)));
        }

        public void AddHashEmptyCondition(string hash)
        {
            _notExistsChecks.Add(hash, _transaction.AddCondition(Condition.HashLengthEqual(hash, 0)));
        }

        public void AddHashEntryExistsCondition(string hash, string key)
        {
            _existsChecks.Add($"{hash}:{key}", _transaction.AddCondition(Condition.HashExists(hash, key)));
        }

        public void AddHashEntryNotExistsCondition(string hash, string key)
        {
            _notExistsChecks.Add($"{hash}:{key}", _transaction.AddCondition(Condition.HashNotExists(hash, key)));
        }

        public void AddHashEntryEqualCondition(string hash, string key, string value)
        {
            _preconditionChecks.Add($"{hash}:{key} == {value}", _transaction.AddCondition(Condition.HashEqual(hash, key, value)));
        }

        public void AddHashEntryNotEqualCondition(string hash, string key, string value)
        {
            _preconditionChecks.Add($"{hash}:{key} != {value}", _transaction.AddCondition(Condition.HashNotEqual(hash, key, value)));
        }

        #endregion

        public void Dispose()
        {
            if (_transaction.Execute())
            {
                return;
            }

            if (_lengthCondition != null && !_lengthCondition.WasSatisfied)
            {
                throw new InsufficientEntriesException();
            }

            var (key, _) = _notExistsChecks.FirstOrDefault(c => !c.Value.WasSatisfied);
            if (key != null)
            {
                throw new EntryAlreadyExistsException(key);
            }

            (key, _) = _existsChecks.FirstOrDefault(c => !c.Value.WasSatisfied);
            if (key != null)
            {
                throw new EntryNotFoundException(key);
            }

            (key, _) = _preconditionChecks.FirstOrDefault(c => !c.Value.WasSatisfied);
            if (key != null)
            {
                throw new FailedConditionException(key);
            }

            // If there's no visible reason for the transaction to fail we can assume it was aborted.
            throw new TransactionAbortedException();
        }
    }
}
