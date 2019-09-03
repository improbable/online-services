using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Improbable.OnlineServices.DataModel;

namespace MemoryStore
{
    /// <summary>
    /// Defines the operations which can be atomically performed on a memory store.
    /// </summary>
    public interface ITransaction : IDisposable
    {
        /// <summary>
        /// Atomically creates all given entries in the memory store. 
        /// </summary>
        /// <precondition>
        /// The given entries should not exist in the memory store. If any do, the transaction will fail on Dispose and
        /// throw an <see cref="EntryAlreadyExistsException"/>.
        /// </precondition>
        void CreateAll(IEnumerable<Entry> entries);

        /// <summary>
        /// Atomically enqueues all given entries' keys into their provided queues, with their provided scores.
        /// </summary>
        void EnqueueAll(IEnumerable<QueuedEntry> entries);

        /// <summary>
        /// Attempts to atomically dequeue the specified number of entry keys from the specified queue. Keys will
        /// be dequeued in ascending order by score.
        /// </summary>
        /// <precondition>
        /// The queue must contain at least the specified number of entries. If it does not,
        /// an <see cref="InsufficientEntriesException"/> will be thrown on Dispose, and no keys will be returned.
        /// </precondition>
        /// <param name="queue">The queue name from which to dequeue the keys.</param>
        /// <param name="number">The number of keys to dequeue.</param>
        /// <returns>A task which, on completion, will return an IEnumerable of string-formatted entry keys.</returns>
        Task<IEnumerable<string>> DequeueAsync(string queue, uint number);

        /// <summary>
        /// Atomically removes all given entries' keys from their provided queues.
        /// </summary>
        void RemoveAllFromQueue(IEnumerable<QueuedEntry> entries);

        /// <summary>
        /// Atomically removes all given entries from the memory store.
        /// </summary>
        /// <precondition>
        /// The given entries must exist in the data store. If any do not, an <see cref="EntryNotFoundException"/>
        /// will be thrown on Dispose.
        /// The given entries should not have been modified since the last retrieval. If false, the transaction will
        /// fail on commit and throw a <see cref="TransactionAbortedException"/>.
        /// </precondition>
        void DeleteAll(IEnumerable<Entry> entries);

        /// <summary>
        /// Atomically updates all given entries in the memory store.
        /// </summary>
        /// <precondition>
        /// The given entries must exist in the data store. If any do not, an <see cref="EntryNotFoundException"/>
        /// will be thrown on Dispose.
        /// The given entries should not have been modified since the last retrieval. If false, the transaction will
        /// fail on commit and throw a <see cref="TransactionAbortedException"/>.
        /// </precondition>
        void UpdateAll(IEnumerable<Entry> entries);

        /// <summary>
        /// Create a hash with the given entries in the data store..
        /// </summary>
        /// <precondition>
        /// The given hash must not exist in the data store. If it does, a <see cref="EntryAlreadyExistsException"/>
        /// will be thrown on Dispose.
        /// </precondition>
        /// <param name="hash">The key of the new hash to add entries to.</param>
        /// <param name="hashEntries">A dictionary of hash entries</param>
        void CreateHashWithEntries(string hash, IEnumerable<KeyValuePair<string, string>> hashEntries);

        /// <summary>
        /// Update a hash with the provided entries. The hash does not need to exist already.
        /// Entries whose value is null or an empty string will be deleted, if they exist.
        /// Other entries will be updates to those hash keys.
        /// </summary>
        /// <param name="hash">The key of the hash to update.</param>
        /// <param name="hashEntries">A dictionary of hash keys and values.</param>
        void UpdateHashWithEntries(string hash, IEnumerable<KeyValuePair<string, string>> hashEntries);

        /// <summary>
        /// Removes the object associated with the specified key.
        /// </summary>
        /// <precondition>
        /// The given key must exist in the data store. If it does not,
        /// a <see cref="EntryNotFoundException"/> will be thrown on Dispose.
        /// </precondition>
        /// <param name="key">The key to delete</param>
        void DeleteKey(string key);

        /// <summary>
        /// Removes the specified hash field stored at key.
        /// </summary>
        /// <precondition>
        /// The given hash must exist in the data store, and the given key in that hash. If it does not,
        /// a <see cref="EntryNotFoundException"/> will be thrown on Dispose.
        /// </precondition>
        /// <param name="key">The key of the hash.</param>
        /// <param name="hashField">The field in the hash to delete.</param>
        void DeleteHashEntry(string key, string hashField);

        #region Conditions

        /// <summary>
        /// Add a condition to the transaction which will check that the given List
        /// is empty. If it is not empty, an <see cref="EntryAlreadyExistsException"/>
        /// will be thrown on Dispose.
        /// </summary>
        /// <param name="list">The key of the list to check.</param>
        void AddListEmptyCondition(string list);

        /// <summary>
        /// Add a condition to the transaction which will check that the given Hash
        /// is empty. If it is not empty, an <see cref="EntryAlreadyExistsException"/>
        /// will be thrown on Dispose.
        /// </summary>
        /// <param name="hash">The key of the hash to check.</param>
        void AddHashEmptyCondition(string hash);

        #endregion
    }
}
