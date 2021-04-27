using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    class Program
    {
        public static void StartClient(string address, int port, string message)
        {
            try
            {
                // Разрешение сетевых имён
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

                // CREATE
                Socket sender = new Socket(
                    ipAddress.AddressFamily,
                    SocketType.Stream, 
                    ProtocolType.Tcp);

                try
                {
                    // CONNECT
                    sender.Connect(remoteEP);

                    Console.WriteLine("Удалённый адрес подключения сокета: {0}",
                        sender.RemoteEndPoint.ToString());

                    // Подготовка данных к отправке
                    byte[] msg = Encoding.UTF8.GetBytes(message + "<EOF>");

                    // SEND
                    int bytesSent = sender.Send(msg);

                    // RECEIVE
                    byte[] buf = new byte[1024];

                    string history = null;
                    while (true)
                    {
                        // RECEIVE
                        int bytesRec = sender.Receive(buf);

                        history += Encoding.UTF8.GetString(buf, 0, bytesRec);
                        if (history.IndexOf("<EOF>") > -1)
                        {
                            break;
                        }
                    }
                    Console.WriteLine("Ответ: {0}", history.Remove(history.Length - 5, 5));

                    // RELEASE
                    sender.Shutdown(SocketShutdown.Both);
                    sender.Close();

                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        static void Main(string[] args)
        {
            StartClient(args[0], Int32.Parse(args[1]), args[2]);
        }
    }
}
