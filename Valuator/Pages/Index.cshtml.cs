using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using NATS.Client;
using System.Threading;
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
            var similarity = GetSimilarity(text);
            _storage.Store(similarityKey, similarity.ToString());

            string textKey = "TEXT-" + id;
            _storage.Store(textKey, text);

            GetAndStoreRank(id);

            return Redirect($"summary?id={id}");
        }

        private async void GetAndStoreRank(string id)
        {
            CancellationTokenSource ct = new CancellationTokenSource();

            ConnectionFactory cf = new ConnectionFactory();

            using (IConnection c = cf.CreateConnection())
            {
                if (!ct.IsCancellationRequested)
                {
                    byte[] data = Encoding.UTF8.GetBytes(id);
                    c.Publish("valuator.processing.rank", data);
                    await Task.Delay(1000);
                }

                c.Drain();
                c.Close();
            }
        }

        private int GetSimilarity(string text)
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
    }
}