using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace TCPChat
{
    public partial class Client : Form
    {
        private string Username = "Thanh Hung";
        private StreamWriter swSender;
        private StreamReader srReceiver;
        private TcpClient tcpServer;

        private delegate void UpdateLogCallback(string strMessage);

        private delegate void CloseConnectionCallback(string strReason);
        private Thread thrMessaging;
        private IPAddress ipAddr;
        private bool Connected;

        public Client()
        {
            Application.ApplicationExit += new EventHandler(OnApplicationExit);
            InitializeComponent();
            
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            if (Connected == true)
            {
                Connected = false;
                swSender.Close();
                srReceiver.Close();
                tcpServer.Close();
            }
        }

        

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (Connected == false)
            {
                initConnection(); //thiết lập kết nối
            }
            else
            {
                CloseConnection("Ngắt kết nối bởi user");
            }
        }

        private void initConnection()
        {
            ipAddr = IPAddress.Parse(txtIpServer.Text);

            tcpServer = new TcpClient();
            tcpServer.Connect(ipAddr,2014);

            Connected = true;
            Username = txtUsername.Text;

            txtIpServer.Enabled = false;
            txtUsername.Enabled = false;
            txtSend.Enabled = true;
            btnConnect.Text = "Disconnect";

            swSender = new StreamWriter(tcpServer.GetStream());
            swSender.WriteLine(txtUsername.Text);
            swSender.Flush();

            thrMessaging = new Thread(new ThreadStart(ReceiveMessage));
            thrMessaging.Start();
        }

        private void ReceiveMessage()
        {
            srReceiver = new StreamReader(tcpServer.GetStream());
            string ConResponse = srReceiver.ReadLine();
            if (ConResponse[0] == '1')
            {
                this.Invoke(new UpdateLogCallback(this.UpdateLog), new object[] { "Kết nối thành công!" });
            }
            else
            {
                string Reason = "Không kết nối!";
                Reason += ConResponse.Substring(2, ConResponse.Length - 2);
                this.Invoke(new CloseConnectionCallback(this.CloseConnection), new object[] { Reason });
                return;
            }
            while (Connected)
            {
                //Hiển  thị tin nhắn trong textbox chat
                if (ConResponse[0] == '1')
                this.Invoke(new UpdateLogCallback(this.UpdateLog), new object[] { srReceiver.ReadLine() });

            }
        }

        private void UpdateLog(string strMessage)
        {
            txtChat.AppendText(strMessage + "\r\n");
        }
        private void CloseConnection(string Reason)
        {
            txtChat.AppendText(Reason + "\r\n");
            txtIpServer.Enabled = true;
            txtUsername.Enabled = true;
            txtSend.Enabled = false;
           btnConnect.Text = "Connect";

            Connected = false;
            swSender.Close();
            srReceiver.Close();
           
            tcpServer.Close();
        }

        private void SendMessage()
        {
            if (txtSend.Lines.Length >= 1)
            {
                swSender.WriteLine(txtSend.Text);
                swSender.Flush();
                txtUsername.Lines = null;
            }
            txtSend.Text = "";
        }
    

        private void Client_Load(object sender, EventArgs e)
        {

        }

        private void Client_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseConnection("");
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            SendMessage();
        }

        private void txtSend_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                SendMessage();
            }
        }
    }
}
