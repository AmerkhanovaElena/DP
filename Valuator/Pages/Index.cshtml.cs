using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

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

            string rankKey = "RANK-" + id;
            _storage.Store(rankKey, GetRank(text).ToString());

            string similarityKey = "SIMILARITY-" + id;
            var similarity = GetSimilarity(text);
            _storage.Store(similarityKey, similarity.ToString());

            if (similarity == 0)
            {
                string textKey = "TEXT-" + id;
                _storage.Store(textKey, text);
            }

            return Redirect($"summary?id={id}");
        }

        private double GetRank(string text)
        {
            int lettersCount = text.Count(char.IsLetter);

            return Math.Round(((text.Length - lettersCount) / (double)text.Length), 2);
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
