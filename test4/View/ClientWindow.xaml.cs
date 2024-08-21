using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;

namespace test4.View
{
    /// <summary>
    /// ClientWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ClientWindow : Window
    {
        private Socket _socket;
        Thread _recivethread;
        private volatile bool _isRunning = true;
        public ClientWindow()
        {
            InitializeComponent();
            btnSender.Click += btnSender_Click;
            btnStartServer.Click += btnConnect_Click;
            Closing += ClientWindows_Closing;

        }


        private void ClientWindows_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            ServerExit(null, _socket);
            _isRunning = false;
            _recivethread?.Join();

        }
        private void btnConnect_Click(object sender, RoutedEventArgs e)
        {


            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            _socket = socket;
            IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse(txtIp.Text), int.Parse(txtPort.Text));
            try
            {
                socket.Connect(iPEndPoint);
            }
            catch (Exception ex)
            {

                MessageBox.Show("连接失败，请重新连接!", "提示");
                return;
            }
            _recivethread = new Thread(ReceiveServerMsg);
            _recivethread.Start(socket);


        }

        private void ReceiveServerMsg(object? state)
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
                        ServerExit(string.Format("客户端：{0}非正常退出", proxSocket.RemoteEndPoint.ToString()), proxSocket);

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
                        ServerExit(string.Format("Server端：{0}非正常退出", proxSocket.RemoteEndPoint.ToString()), proxSocket);


                    }
                    catch (Exception)
                    {

                    }
                    return;
                }
                string msgStr = Encoding.Default.GetString(data, 0, len);
                //拼接字符串
                AppendTxtLogText(string.Format("Server端：{0}", msgStr));

            }

        }

        private void ServerExit(string msg, Socket proxSocekt)
        {
            AppendTxtLogText(msg);
            try
            {
                if (proxSocekt != null)
                {
                    if (proxSocekt.Connected)
                    {
                        proxSocekt.Shutdown(SocketShutdown.Both);
                        proxSocekt.Close(100);
                    }
                }
            }
            catch (Exception ex)
            {

            }

        }
        private void btnSender_Click(object sender, RoutedEventArgs e)
        {
            byte[] data = Encoding.Default.GetBytes(this.txtMsg.Text);
            _socket.Send(data, 0, data.Length, SocketFlags.None);
            AppendTxtLogText($"me:{this.txtMsg.Text}");
            this.txtMsg.Text = null;


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
    }
}
