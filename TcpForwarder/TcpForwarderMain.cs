using System;
using System.Net;

namespace TcpForwarder
{
    public class TcpForwarderMain
    {

        private static void MainCore(string remoteHost, int remotePort, IPEndPoint local)
        {
            var x = new TcpForwarder(remoteHost, remotePort, local);
            Console.CancelKeyPress += (s, e) =>
            {
                Console.WriteLine("Cancel key pressed");
                x.Stop();
            };
            x.Start().Wait();
        }

        public static void Main(string remoteHost, int remotePort, string localIp, int localPort)
        {
            MainCore(remoteHost, remotePort, new IPEndPoint(IPAddress.Parse(localIp), localPort));
        }

        public static void Main(string remoteHost, int remotePort, int localPort)
        {
            MainCore(remoteHost, remotePort, new IPEndPoint(IPAddress.Loopback, localPort));
        }
    }
}
