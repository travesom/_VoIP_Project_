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
        Int32 _voice_port = 8087;
        Int32 _control_port = 8088;
        Int32 _xml_port = 8088;
        IPAddress localaddr = IPAddress.Parse("127.0.0.1");
        public Form1()
        {
            
            InitializeComponent();
        }

        private ULP send(ulpOperation operation, Int32 port) {
            TcpClient client = new TcpClient(localaddr.ToString(), port);
            ULP sendFrame = new ULP(operation, txt_login.Text + ' ' + txt_pass.Text);
            byte[] sendBytes = Encoding.ASCII.GetBytes(sendFrame.ToString());

            client.GetStream().WriteAsync(sendBytes, 0, sendBytes.Length);
            byte[] buffer = new byte[1500];
            client.GetStream().Read(buffer, 0, buffer.Length);
            ULP frame = new ULP(Encoding.ASCII.GetString(buffer, 0, buffer.Length));
            return frame;

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
                ULP frame = send(ulpOperation.LOGIN, _login_port);
                switch (frame.OperationStatus) {
                    case ulpOperStatus.SUCCESS:
                        {
                            var token = frame.data;
                            Hide();
                            Main_form fm = new Main_form(token, txt_login.Text);
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
                            MessageBox.Show("Wrong login/or user is logged, please try again");
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

        private void btn_register_Click(object sender, EventArgs e)
        {
            switch (send(ulpOperation.REGISTER, _login_port).OperationStatus)
            {
                case ulpOperStatus.SUCCESS:
                    {
                        MessageBox.Show("you can now log in");
                    }
                    break;
                
                case ulpOperStatus.WRONG_LOGIN:
                    {
                        MessageBox.Show("Login you used is taken, plese choose diffrent login");
                    }
                    break;
                default:
                    {
                        MessageBox.Show("Something wrong, please try again");

                    }
                    break;
                


            }

        }
    }
}
