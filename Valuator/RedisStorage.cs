using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Valuator
{
    public class RedisStorage : IStorage
    {
        private readonly ILogger<RedisStorage> _logger;
        private readonly IConnectionMultiplexer _redis;
        private readonly string _host = "localhost";
        private readonly int _port = 6379;

        public RedisStorage(ILogger<RedisStorage> logger)
        {
            _logger = logger;
            _redis = ConnectionMultiplexer.Connect(_host);
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
        }

        public List<string> GetTextKeys()
        {
            var server = _redis.GetServer(_host, _port);

            List<string> stringKeys = new List<string>();
            var keys = server.Keys(pattern: "TEXT-*");
            foreach (var key in keys)
            {
                stringKeys.Add(key);
            }

            return stringKeys;
        }
    }
}