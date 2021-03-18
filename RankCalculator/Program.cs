using NATS.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Linq;
using StorageLibrary;

namespace RankCalculator
{
    class Program
    {
        static void Main(string[] args)
        {
            RedisStorage storage;
            ILogger<Program> logger;
            using (var lf = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug)))
            {
                var storageLogger = lf.CreateLogger<RedisStorage>();
                storage = new RedisStorage(storageLogger);
                logger = lf.CreateLogger<Program>();
            }

            IConnection c = new ConnectionFactory().CreateConnection();

            IAsyncSubscription s = c.SubscribeAsync("valuator.processing.rank", "rank_calculator", (sender, args) =>
            {
                string id = Encoding.UTF8.GetString(args.Message.Data);

                string textKey = "TEXT-" + id;
                string text = storage.Load(textKey);

                string rankKey = "RANK-" + id;
                double rank = GetRank(text);
                storage.Store(rankKey, GetRank(text).ToString());
                logger.LogDebug("Ranked {0} text {1}", rank, text);
            });

            s.Start();

            Console.WriteLine("Press Enter to exit");
            Console.ReadLine();   

            s.Unsubscribe();

            c.Drain();
            c.Close();   
        }

        private static double GetRank(string text)
        {
            int lettersCount = text.Count(char.IsLetter);
            int nonLettersCount = text.Length - lettersCount;

            return Math.Round((nonLettersCount / (double)text.Length), 2);
        }
    }
}