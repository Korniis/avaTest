using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;

namespace test4.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Thread _serverReciveThread;
        Thread _listenthread;
        private volatile bool _isRunning;

        List<Socket> clientsockets = new List<Socket>();
        public MainWindow()
        {

            InitializeComponent();
            Loaded += MainWindow_Loaded;
            btnStartServer.Click += btnStartServer_Click;
            btnSender.Click += btnSender_Click;
            Closing += MainWindow_Closing;
            _isRunning = true;
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            for (int i = 0; i < clientsockets.Count; i++)//向每个客户端说我下线了
            {
                ClientExit(null, clientsockets[i]);
            }
            _isRunning = false;
            _serverReciveThread?.Join();
            _listenthread?.Join();
           // System.Environment.Exit(0);
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ClientWindow clientWindow = new ClientWindow();
            clientWindow.Show();


        }


        private void btnSender_Click(object sender, RoutedEventArgs e)
        {

            foreach (Socket proxSocket in clientsockets)
            {
                if (proxSocket.Connected) // 判断客户端是否还在连接
                {
                    // 去除消息中的数字
                    byte[] data = Encoding.Default.GetBytes(this.txtMsg.Text);
                    // 发送消息
                    proxSocket.Send(data, 0, data.Length, SocketFlags.None); // 指定套接字的发送行为
                    this.txtMsg.Text = null;
                }
            }
        }

        private void btnStartServer_Click(object sender, RoutedEventArgs e)
        {
            Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(this.txtIp.Text), int.Parse(this.txtPort.Text));
            serverSocket.Bind(ipe);
            serverSocket.Listen(10);
            ThreadPool.QueueUserWorkItem(new WaitCallback(AcceptClientConnect), serverSocket);

        }

        private void AcceptClientConnect(object? state)
        {
            var serverSocket = state as Socket;

            AppendTxtLogText("服务端开始接收客户端连接！");

            //不断接受客户端的连接
            while (_isRunning)
            {
                //5、创建一个负责通信的Socket
                Socket proxSocket = serverSocket.Accept();
                AppendTxtLogText(string.Format("客户端：{0}连接上了！", proxSocket.RemoteEndPoint.ToString()));
                //将连接的Socket存入集合
                clientsockets.Add(proxSocket);
                //6、不断接收客户端发送来的消息
                ThreadPool.QueueUserWorkItem(new WaitCallback(ReceiveClientMsg), proxSocket);

            }

        }

        private void ReceiveClientMsg(object? state)
        {
            var proxSocket = state as Socket;
            byte[] data = new byte[1020 * 1024];
            while (_isRunning)
            {
                int len;
                try
                {
                    len = proxSocket.Receive(data, 0, data.Length, SocketFlags.None);
                }
                catch (Exception ex)
                {
                    try
                    {
                        ClientExit(string.Format("客户端：{0}非正常退出", proxSocket.RemoteEndPoint.ToString()), proxSocket);

                    }
                    catch (Exception)
                    {


                    }
                    return;
                }
                if (len <= 0)
                {
                    try
                    {
                        ClientExit(string.Format("客户端：{0}非正常退出", proxSocket.RemoteEndPoint.ToString()), proxSocket);


                    }
                    catch (Exception)
                    {

                    }
                    return;
                }
                string msgStr = Encoding.Default.GetString(data, 0, len);
                //拼接字符串
                AppendTxtLogText(string.Format("接收到客户端：{0}的消息：{1}", proxSocket.RemoteEndPoint.ToString(), msgStr));

            }
        }
        private void AppendTxtLogText(string str)
        {
            // 如果当前线程没有访问权，使用 Dispatcher 调度更新操作到 UI 线程
            if (!txtLog.Dispatcher.CheckAccess())
            {
                this.Dispatcher.BeginInvoke(new Action<string>(s =>
                {
                    this.txtLog.Text = $"{txtLog.Text}\r\n{s}";
                }), str);
            }
            else
            {
                // 如果当前线程有访问权，直接更新 TextBox 控件的文本
                this.txtLog.Text = $"{txtLog.Text}\r\n{str}";
            }
        }
        private void ClientExit(string msg, Socket proxSocket)
        {
            AppendTxtLogText(msg);
            clientsockets.Remove(proxSocket);
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
    }
}