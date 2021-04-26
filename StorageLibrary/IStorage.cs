using System.Collections.Generic;

namespace StorageLibrary
{
    public interface IStorage
    {
        void StoreToShard(string key, string value, string shardKey);
        string LoadFromShard(string key, string shardKey);

        void StoreShardKeyToMap(string id, string shardKey);
        string GetShardKeyFromMap(string id);
        bool IsDuplicate(string text);
    }
}