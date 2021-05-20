using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Chain
{
    class Program
    {
        static Socket sender;
        static Socket listener;

        public static void InitiateSocket(int port, string address, int listeningPort)
        {
            // create listener
            IPAddress listeningIpAddress = IPAddress.Any;

            IPEndPoint localEndPoint = new IPEndPoint(listeningIpAddress, listeningPort);

            listener = new Socket(
                listeningIpAddress.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            listener.Bind(localEndPoint);
            listener.Listen(10);

            // create sender
            IPAddress ipAddress;
            if (address == "localhost")
            {
                ipAddress = IPAddress.Loopback;
            }
            else
            {
                ipAddress = IPAddress.Parse(address);
            }

            IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

            sender = new Socket(
                ipAddress.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            bool connected = false;
            while (!connected)
            {
                try
                {
                    sender.Connect(remoteEP);
                    connected = true;
                }
                catch (SocketException e)
                {
                    Thread.Sleep(1000);
                }
            }

            Console.WriteLine("Initiated socket");
        }
        public static int ReceiveMessage(Socket handler)
        {
            byte[] buf = new byte[1024];

            int bytes = handler.Receive(buf);
            string data = Encoding.UTF8.GetString(buf, 0, bytes);

            return Convert.ToInt32(data);
        }

        public static void SendMessage(string message)
        {
            byte[] msg = Encoding.UTF8.GetBytes(message);

            sender.Send(msg);
        }

        static void Main(string[] args)
        {
            if (args.Length < 3 || args.Length > 4)
            {
                Console.WriteLine("Invalid arguments count");
            }
            else
            {
                InitiateSocket(Int32.Parse(args[0]), args[1], Int32.Parse(args[2]));

                Console.WriteLine("\nEnter X...");
                int x = Int32.Parse(Console.ReadLine());

                Socket handler;

                if (args.Length == 4) // initiator
                {
                    SendMessage(x.ToString());
                    handler = listener.Accept();
                    int y = ReceiveMessage(handler);
                    SendMessage(Math.Max(x, y).ToString());
                    x = y;
                }
                else // not initiator
                {
                    handler = listener.Accept();
                    int y = ReceiveMessage(handler);
                    SendMessage(Math.Max(x, y).ToString());
                    x = ReceiveMessage(handler);
                    SendMessage(y.ToString());
                }
                Console.WriteLine(x);
                Console.ReadLine();

                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }
        }
    }
}