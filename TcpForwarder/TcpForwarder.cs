using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace TcpForwarder
{

    public class TcpForwarder
    {

        private static readonly int BufSize = 128 * 1024;
        private readonly string rhost;
        private readonly int rport;

        private readonly CancellationTokenSource cts = new CancellationTokenSource();
        private readonly CancellationToken ct;

        private readonly TcpListener server;

        public TcpForwarder(string remoteHost, int remotePort, IPEndPoint local)
        {
            rhost = remoteHost;
            rport = remotePort;
            server = new TcpListener(local);
            ct = cts.Token;
        }

        [Conditional("DEBUG")]
        private void DebugWriteLine(int id, string direction, string message, params object[] args)
        {
            if (direction != null)
            {
                Debug.WriteLine(String.Format("{0} {1}:\t", id, direction) + message, args);
            }
            else
            {
                Debug.WriteLine(String.Format("{0}:\t", id) + message, args);
            }
        }

        // ソケット間でデータをコピーする
        private async Task<bool> CopyBetweenSockets(Socket from, Socket to, string direction, int connectionId)
        {

            byte[] buf = new byte[BufSize];

            while (!ct.IsCancellationRequested)
            {
                var i = 0;

                try
                {
                    DebugWriteLine(connectionId, direction, "Receiving...");
                    // Xxx: キャンセル可能でなくて良いのか？
                    i = await from.ReceiveAsync(new ArraySegment<byte>(buf), SocketFlags.None);
                    DebugWriteLine(connectionId, direction, "Received {0} bytes", i);
                }
                catch (SocketException e)
                {
                    // timedout, connection reset, ...
                    DebugWriteLine(connectionId, direction, "SocketException on receiving, ErrorCode: {0}", e.SocketErrorCode);
                    break;
                }

                if (i == 0)
                {
                    // from側が切断した
                    break;
                }

                try
                {
                    DebugWriteLine(connectionId, direction, "Sending...");
                    // 受信した物はキャンセル指令が出てもキャンセルしない
                    // TODO: キャンセル指令が出たらタイムアウトをもうけたい
                    await to.SendAllAsync(new ArraySegment<byte>(buf, 0, i));
                    DebugWriteLine(connectionId, direction, "Sent");
                }
                catch (SocketException e)
                {
                    DebugWriteLine(connectionId, direction, "Send failed, ErrorCode: {0}", e.SocketErrorCode);
                    // 送信失敗したので受信側をリセットする
                    return false;
                }
            }
            DebugWriteLine(connectionId, direction, "Shutdown sockets");
            // 双方のソケットに終了を通知する
            from.ShutdownSilently(SocketShutdown.Receive);
            to.ShutdownSilently(SocketShutdown.Send);
            return true;
        }

        // 将来的に、接続数制限を付けるなどを考えて、tasksで管理する
        private TaskCollection tasks = new TaskCollection();

        private int m_connId = 0;

        private async Task Foo(Socket client)
        {
            using (client)
            using (var remote = new TcpClient())
            {
                var id = Interlocked.Increment(ref m_connId);
                try
                {
                    await remote.ConnectAsync(rhost, rport);
                    DebugWriteLine(id, null, "Connected Remote Server");
                }
                catch (SocketException e)
                {
                    Console.WriteLine("{0}:\tリモートサーバへの接続に失敗, SocketException ErrorCode: {1}", id, e.ErrorCode);
                    client.CloseHard();
                    return;
                }

                var r = await Task.WhenAll(CopyBetweenSockets(client, remote.Client, "client -> remote", id), CopyBetweenSockets(remote.Client, client, "remote -> client", id));
                if(!r[0]){
                    client.CloseHard();
                }
                if(!r[1]){
                    remote.Client.CloseHard();
                }
            }
        }

        private async Task AcceptAndSpawn()
        {
            while (!ct.IsCancellationRequested)
            {

                Socket c = await server.AcceptSocketAsync();
                var x = Foo(c).ContinueWith(it =>
                {
                    if (it.Exception != null)
                    {
                        Console.WriteLine("unexpected Exception occurs");
                        Console.WriteLine(it.Exception);
                    }
                });
                tasks.Add(x);
            }
        }

        public async Task Start()
        {
            try
            {
                server.Start();
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException on server Starting. errorCode: {0}", e.SocketErrorCode);
                return;
            }

            await AcceptAndSpawn().ContinueWith(it =>
            {
                server.Stop();
            });
        }

        public void Stop()
        {
            server.Stop();
            cts.Cancel();
        }
    }
}
