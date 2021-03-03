using System.Collections.Generic;
namespace Valuator
{
    public interface IStorage
    {
        void Store(string key, string value);
        string Load(string key);
        List<string> GetTextKeys();
    }
}