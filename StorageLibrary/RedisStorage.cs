using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace StorageLibrary
{
    public class RedisStorage : IStorage
    {
        private readonly ILogger<RedisStorage> _logger;
        private readonly IConnectionMultiplexer _redis;
        private readonly string _host = "localhost";
        private readonly int _port = 6379;
        private List<string> _textKeys;

        public RedisStorage(ILogger<RedisStorage> logger)
        {
            _logger = logger;
            _redis = ConnectionMultiplexer.Connect(_host);

            var server = _redis.GetServer(_host, _port);
            List<string> stringKeys = new List<string>();
            var keys = server.Keys(pattern: "TEXT-*");
            foreach (var key in keys)
            {
                stringKeys.Add(key);
            }
            _textKeys = stringKeys;
        }

        public string Load(string key)
        {
            IDatabase db = _redis.GetDatabase();
            
            return db.StringGet(key);
        }

        public void Store(string key, string value)
        {
            IDatabase db = _redis.GetDatabase();
            db.StringSet(key, value);

            if (key.StartsWith("TEXT-"))
            {
                _textKeys.Add(key);
            }
        }

        public List<string> GetTextKeys()
        {
            return _textKeys;
        }
    }
}