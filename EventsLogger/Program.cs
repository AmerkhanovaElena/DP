using System;
using System.Text;
using NATS.Client;
using Microsoft.Extensions.Logging;

namespace EventsLogger
{
    class Program
    {
        static void Main(string[] args)
        {
            ILogger<Program> logger;
            using (var lf = LoggerFactory.Create(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug)))
            {
                logger = lf.CreateLogger<Program>();
            }

            IConnection c = new ConnectionFactory().CreateConnection();

            IAsyncSubscription s = c.SubscribeAsync("rankCalculator.calculated.rank", (sender, args) =>
            {
                string[] string_data = GetStringData(args.Message.Data);
                logger.LogDebug("On subject {0} calculated rank {1} for text at id {2}", args.Message.Subject, string_data[0], string_data[1]);
            });

            s = c.SubscribeAsync("valuator.calculated.similarity", (sender, args) =>
            {
                string[] string_data = GetStringData(args.Message.Data);
                logger.LogDebug("On subject {0} calculated similarity {1} for text at id {2}", args.Message.Subject, string_data[0], string_data[1]);
            });

            s.Start();

            Console.WriteLine("Press Enter to exit");
            Console.ReadLine();

            s.Unsubscribe();

            c.Drain();
            c.Close();
        }

        private static string[] GetStringData(byte[] data)
        {
            string concatenated_string_data = Encoding.UTF8.GetString(data);

            return concatenated_string_data.Split(' ');
        }
    }
}