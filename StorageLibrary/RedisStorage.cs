using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;

namespace StorageLibrary
{
    public class RedisStorage : IStorage
    {
        private readonly ILogger<RedisStorage> _logger;
        private readonly IConnectionMultiplexer _redisMap;
        private IConnectionMultiplexer _redisRu;
        private IConnectionMultiplexer _redisEu;
        private IConnectionMultiplexer _redisOther;
        private readonly string _host = "localhost";

        public RedisStorage(ILogger<RedisStorage> logger)
        {
            _logger = logger;
            _redisMap = ConnectionMultiplexer.Connect(_host);
            _redisRu = ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable("DB_RUS", EnvironmentVariableTarget.User));
            _redisEu = ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable("DB_EU", EnvironmentVariableTarget.User));
            _redisOther = ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable("DB_OTHER", EnvironmentVariableTarget.User));
        }

        public string LoadFromShard(string key, string shardKey)
        {
            IDatabase db = GetConnection(shardKey).GetDatabase();
            
            return db.StringGet(key);
        }

        public void StoreToShard(string key, string value, string shardKey)
        {
            IDatabase db = GetConnection(shardKey).GetDatabase();
            db.StringSet(key, value);

            if (key.StartsWith(Constants.TEXT_PREFIX))
            {
                db.SetAdd(Constants.TEXTS_SET_KEY, value);
            }
        }

        public void StoreShardKeyToMap(string id, string shardKey)
        {
            IDatabase db = _redisMap.GetDatabase();
            db.StringSet(id, shardKey);
        }

        public string GetShardKeyFromMap(string id)
        {
            IDatabase db = _redisMap.GetDatabase();     

            return db.StringGet(id);
        }

        private IConnectionMultiplexer GetConnection(string shardKey)
        {
            if (shardKey == Constants.RUS_SHARD)
            {
                return _redisRu;
            }
            else if (shardKey == Constants.EU_SHARD)
            {
                return _redisEu;
            }
            else if (shardKey == Constants.OTHER_SHARD)
            {
                return _redisOther;
            }
            else
            {
                _logger.LogWarning("Shard key {0} doesn't exist", shardKey);
                return _redisMap;
            }
        }
        
        public bool IsDuplicate(string text)
        {
            List<IDatabase> shards = new List<IDatabase> { _redisRu.GetDatabase(), _redisEu.GetDatabase(), _redisOther.GetDatabase() };
            foreach (IDatabase shard in shards)
            {
                if (shard.SetContains(Constants.TEXTS_SET_KEY, text))
                {
                    return true;
                }
            }

            return false;
        }
    }
}