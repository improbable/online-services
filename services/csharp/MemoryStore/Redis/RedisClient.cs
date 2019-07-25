using System.Threading.Tasks;
using Improbable.OnlineServices.DataModel;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace MemoryStore.Redis
{
    public class RedisClient : IMemoryStoreClient
    {
        private readonly IDatabase _internalClient;
        private readonly LoadedLuaScript _zpopMinScript;

        public RedisClient(IDatabase client, LoadedLuaScript zpopMinScript)
        {
            _internalClient = client;
            _zpopMinScript = zpopMinScript;
        }

        public ITransaction CreateTransaction()
        {
            return new RedisTransaction(_internalClient.CreateTransaction(), _zpopMinScript);
        }

        public async Task<T> GetAsync<T>(string id) where T : Entry
        {
            var key = Key.For<T>(id);
            // Execute this asynchronously in order to free up worker threads.
            // This results in much better performance when number of in-flight requests > number of worker threads. 
            var serializedEntry = await _internalClient.StringGetAsync(key);
            if (serializedEntry.IsNullOrEmpty)
            {
                return null;
            }

            var entry = JsonConvert.DeserializeObject<T>(serializedEntry);
            entry.PreviousState = serializedEntry;
            return entry;
        }

        public void Dispose()
        {
        }
    }
}
