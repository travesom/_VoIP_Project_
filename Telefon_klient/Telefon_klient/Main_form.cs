using Protocols;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Telefon_klient
{
    public partial class Main_form : Form
    {
        int user_token;
        string login;
        public IPAddress localaddr = IPAddress.Parse("127.0.0.1");
        public Int32 _login_port = 8086;
        public Int32 _voice_port = 8087;
        public Int32 _control_port = 8088;
        public Int32 _xml_port = 8089;
        public Main_form(string token, string login)
        {
            this.user_token = int.Parse(token);
            this.login = login;
            InitializeComponent();

            label_username.Text = this.login;
        }
        /*<summary>
           send to server login and status and return response from server
         */
        private NVCP Send_MYSTATUS(nvcpOperStatus status)
        {
            TcpClient client = new TcpClient(localaddr.ToString(), _control_port);
            NVCP sendFrame = new NVCP(nvcpOperation.MY_STATUS, status, login + ' ' + user_token.ToString());
            byte[] sendBytes = Encoding.ASCII.GetBytes(sendFrame.ToString());
            client.GetStream().WriteAsync(sendBytes, 0, sendBytes.Length);
            byte[] buffer = new byte[1500];
            client.GetStream().Read(buffer, 0, buffer.Length);
            NVCP frame = new NVCP(Encoding.ASCII.GetString(buffer, 0, buffer.Length));
            tokenupdate(frame);
            return frame;
        }
        private void tokenupdate(NVCP frame)
        {
            if (frame.ProtocolStatus == IStatus.OK)
                user_token = int.Parse(frame.data);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Send_MYSTATUS(nvcpOperStatus.UNAVAILABLE);
            this.Hide();
            Form1 f1 = new Form1();
            f1.Show();
        }

        private void rad_status_ready_CheckedChanged(object sender, EventArgs e)
        {
            if (rad_status_ready.Checked)
            {
                Send_MYSTATUS(nvcpOperStatus.READY);
            }
        }

        private void rad_status_busy_CheckedChanged(object sender, EventArgs e)
        {
            if (rad_status_busy.Checked)
            {
                Send_MYSTATUS(nvcpOperStatus.BUSY);
            }
        }


    }
}
