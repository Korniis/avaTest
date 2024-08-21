using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
namespace Talk.Server
{
    public class SocketServer
    {
        private IPAddress ip;
        private IPEndPoint ipe;
        private Socket _socket;
        private volatile bool IsRunning = true;
        private Thread _receiveThread;
        public List<Socket> _customSocket = new List<Socket>();
        public SocketServer(string host, int port)
        {
            ip = IPAddress.Parse(host);
            ipe = new IPEndPoint(ip, port);
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="backLog">队列等待数</param>
        public void Start(int backLog)
        {
            _socket.Bind(ipe);
            _socket.Listen(backLog);
            _receiveThread = new Thread(AcceptClientConnect);
            _receiveThread.Start(_socket);
            Console.WriteLine("已经启动最大人数:" + backLog);
            Console.WriteLine($"启动地址：" + ipe.Address+":"+ipe.Port);
        }
        private async void AcceptClientConnect(object? obj)
        {
            var mainSocket = obj as Socket;
            Console.WriteLine("\"服务端开始接收客户端连接！\"");
            while (IsRunning)
            {
                Socket clientSocket = await _socket.AcceptAsync();
                Console.WriteLine($"{clientSocket.RemoteEndPoint.ToString()}: " + "connected");
                _customSocket.Add(clientSocket);
                _ = Task.Run(() => ReceiveClientMsgAsync(clientSocket));
            }
        }
        /// <summary>
        /// 接收
        /// </summary>
        private async void ReceiveClientMsgAsync(object? obj)
        {
            var clientSocket = obj as Socket;
            byte[] data = new byte[1024 * 1024]; // 缓冲区大小
            while (IsRunning && clientSocket.Connected)
            {
                try
                {
                    // 使用 ArraySegment<byte> 进行 ReceiveAsync 调用
                    int len = await clientSocket.ReceiveAsync(new ArraySegment<byte>(data), SocketFlags.None);
                    if (len > 0)
                    {
                        string message = Encoding.UTF8.GetString(data, 0, len);
                        Console.WriteLine($"{clientSocket.RemoteEndPoint.ToString()}收到消息: {message}");
                        this.Send($"{clientSocket.RemoteEndPoint.ToString()}收到消息: {message}");
                    }
                    else
                    {
                        Console.WriteLine(string.Format("客户端：{0}非正常退出", clientSocket.RemoteEndPoint.ToString()));
                        break;
                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        Console.WriteLine(string.Format("客户端：{0}非正常退出", clientSocket.RemoteEndPoint.ToString()));
                        break;

                    }
                    catch (Exception)
                    {
                    }
                  
                }
            }
            // 清理资源
            clientSocket.Shutdown(SocketShutdown.Both);
            Console.WriteLine(string.Format("客户端：{0}正常退出", clientSocket.RemoteEndPoint.ToString()));

            clientSocket.Close();
        }
      
 
        /// <summary>
        /// 发送信息
        /// </summary>
        /// <param name="message"> 信息</param>
        public void Send(string message)
        {
            foreach (Socket proxSocket in _customSocket)
            {
                if (proxSocket.Connected) // 判断客户端是否还在连接
                {
                    // 去除消息中的数字
                    byte[] data = Encoding.Default.GetBytes(message);
                    // 发送消息
                    proxSocket.Send(data, 0, data.Length, SocketFlags.None); // 指定套接字的发送行为
                  
                }
            }
        }
        /// <summary>
        /// 关闭socket
        /// </summary>
        public void Close(string msg, Socket proxSocket)
        {
            _customSocket.Remove(proxSocket);
            try
            {
                if (proxSocket.Connected)
                {
                    proxSocket.Shutdown(SocketShutdown.Both);
                    proxSocket.Close(100);
                }
            }
            catch (Exception ex) { }

        }
        public void CloseAll() {
            for (int i = 0; i < _customSocket.Count; i++)//向每个客户端说我下线了
            {
                Close(null, _customSocket[i]);
            }
            IsRunning = false;
            _receiveThread?.Join();
           
        }
    }
}
