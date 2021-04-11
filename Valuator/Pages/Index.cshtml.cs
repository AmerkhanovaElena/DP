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

        public IActionResult OnPost(string text)
        {
            _logger.LogDebug(text);

            string id = Guid.NewGuid().ToString();

            string similarityKey = "SIMILARITY-" + id;
            var similarity = GetSimilarity(id, text);
            _storage.Store(similarityKey, similarity.ToString());
            string string_data = similarity.ToString() + " " + id;
            PublishMessageOnSubject("valuator.calculated.similarity", string_data);

            string textKey = "TEXT-" + id;
            _storage.Store(textKey, text);

            PublishMessageOnSubject("valuator.processing.rank", id);

            return Redirect($"summary?id={id}");
        }

        private int GetSimilarity(string id, string text)
        {
            var keys = _storage.GetTextKeys();
            foreach (var key in keys)
            {
                if (text == _storage.Load(key))
                {
                    return 1;
                }
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