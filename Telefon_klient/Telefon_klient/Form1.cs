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
    public partial class Form1 : Form
    {
        Int32 _login_port = 8086;
        IPAddress localaddr = IPAddress.Parse("127.0.0.1");
        public Form1()
        {
            
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btn_Sign_in_Click(object sender, EventArgs e)
        {
            if (txt_login.Text == "" || txt_pass.Text == "")
            {
                MessageBox.Show("no login or/and password");
                return;


            }
            else
            {
                TcpClient client = new TcpClient(localaddr.ToString(), _login_port);
                ULP sendFrame = new ULP(ulpOperation.LOGIN,txt_login.Text + ' ' + txt_pass.Text);
                byte[] sendBytes = Encoding.ASCII.GetBytes(sendFrame.ToString());

                client.GetStream().WriteAsync(sendBytes, 0, sendBytes.Length);
                byte[] buffer = new byte[1500];
                client.GetStream().Read(buffer, 0, buffer.Length);
                ULP frame = new ULP(Encoding.ASCII.GetString(buffer, 0, buffer.Length));
                switch (frame.OperationStatus) {
                    case ulpOperStatus.SUCCESS:
                        {
                        this.Hide();
                        Main_form fm = new Main_form();
                        fm.Show();
                        }
                        break;
                    case ulpOperStatus.NONE:
                        {
                            MessageBox.Show("Something wrong, please try again");
                        }
                        break;
                    case ulpOperStatus.WRONG_LOGIN:
                        {
                            MessageBox.Show("Wrong login, please try again");
                        }
                        break;
                    case ulpOperStatus.WRONG_PASS:
                        {
                            MessageBox.Show("Wrong password, please try again");
                        }
                        break;


                }
                
            
            
            }
        }
    }
}
