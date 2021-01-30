namespace TcpForwarder
{
    class Program
    {
        static void Main(string[] args)
        {
            int localPort = int.Parse(args[1]);
            int remotePort = int.Parse(args[2]);

            TcpForwarderMain.Main(args[0], localPort, remotePort);
        }
    }
}
