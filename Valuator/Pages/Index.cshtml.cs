using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using NATS.Client;
using System.Threading.Tasks;
using StorageLibrary;

namespace Valuator.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IStorage _storage;

        public IndexModel(ILogger<IndexModel> logger, IStorage storage)
        {
            _logger = logger;
            _storage = storage;
        }

        public void OnGet()
        {
            
        }

        public IActionResult OnPost(string text, string shardKey)
        {
            _logger.LogDebug(text);

            string id = Guid.NewGuid().ToString();
            _storage.StoreShardKeyToMap(id, shardKey);
            string similarityKey = Constants.SIMILARITY_PREFIX + id;
            var similarity = GetSimilarity(id, text);
            _storage.StoreToShard(similarityKey, similarity.ToString(), shardKey);
            string string_data = similarity.ToString() + " " + id;
            PublishMessageOnSubject("valuator.calculated.similarity", string_data);

            string textKey = Constants.TEXT_PREFIX + id;
            _storage.StoreToShard(textKey, text, shardKey);

            PublishMessageOnSubject("valuator.processing.rank", id);

            return Redirect($"summary?id={id}");
        }

        private int GetSimilarity(string id, string text)
        {
            if (_storage.IsDuplicate(text))
            {
                return 1;
            }

            return 0;
        }

        private void PublishMessageOnSubject(string subject, string string_data)
        {
            ConnectionFactory cf = new ConnectionFactory();

            using (IConnection c = cf.CreateConnection())
            {
                byte[] data = Encoding.UTF8.GetBytes(string_data);
                c.Publish(subject, data);

                c.Drain();
                c.Close();
            }
        }
    }
}