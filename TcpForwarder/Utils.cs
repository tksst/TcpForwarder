using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TcpForwarder
{
    static class Utils
    {
        public static async Task SendAllAsync(this Socket socket, ArraySegment<byte> memory, SocketFlags flags = SocketFlags.None)
        {
            while (true)
            {
                // Xxx: キャンセル可能でなくてよいのか？
                var i = await socket.SendAsync(memory, flags);
                if (i >= memory.Count)
                {
                    break;
                }
                memory = new ArraySegment<byte>(memory.Array, i, memory.Count - i);
            }
        }

        public static void ShutdownSilently(this Socket socket, SocketShutdown how)
        {
            try
            {
                socket.Shutdown(how);
            }
            catch (SocketException)
            {
                // ignore
            }
            catch (ObjectDisposedException)
            {
                // ignore
            }
        }

        public static void CloseHard(this Socket socket)
        {
            // Xxx: これでちゃんとRESETになるのだろうか？
            socket.LingerState = new LingerOption(true, 0);
            socket.Close();
        }
    }
}
