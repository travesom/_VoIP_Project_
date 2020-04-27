using Protocols;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Telefon_klient
{
    public partial class Form1 : Form
    {   
        public Int32 _login_port = 8086;
        public Int32 _voice_port = 8087;
        public Int32 _control_port = 8088;
        public Int32 _xml_port = 8089;
        public static String serverName=null;
        public static String machineName=null;
        public IPAddress server_addres = IPAddress.Parse("127.0.0.1");
        public Form1()
        {
            
            InitializeComponent();
        }

        /*<summary>
         sends login and password for login/register and return response from server
         </summary>
        */
        private ULP send_login_data(ulpOperation operation, Int32 port) {
            machineName=txt_server_add.Text
            TcpClient client = new TcpClient(machineName, port);
            SslStream sslStream = new SslStream(
                client.GetStream(),
                false,
                new RemoteCertificateValidationCallback(ValidateServerCertificate),
                null
                );
            try
            {
                sslStream.AuthenticateAsClient(serverName);
            }
            catch (AuthenticationException e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
                if (e.InnerException != null)
                {
                    Console.WriteLine("Inner exception: {0}", e.InnerException.Message);
                }
                Console.WriteLine("Authentication failed - closing the connection.");
                client.Close();
                
            }

            ULP sendFrame = new ULP(operation, txt_login.Text + ' ' + txt_pass.Text);
            byte[] sendBytes = Encoding.ASCII.GetBytes(sendFrame.ToString());
            sslStream.Write(sendBytes);
            sslStream.Flush();
            //client.GetStream().WriteAsync(sendBytes, 0, sendBytes.Length);
            byte[] buffer = new byte[1500];
            sslStream.Read(buffer, 0, buffer.Length);
            // Close the client connection.
            client.Close();
            ULP frame = new ULP(Encoding.ASCII.GetString(buffer, 0, buffer.Length));
            return frame;

        }

        private static Hashtable certificateErrors = new Hashtable();

        // The following method is invoked by the RemoteCertificateValidationDelegate.
        public static bool ValidateServerCertificate(
              object sender,
              X509Certificate certificate,
              X509Chain chain,
              SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            Console.WriteLine("Certificate error: {0}", sslPolicyErrors);

            // Do not allow this client to communicate with unauthenticated servers.
            return false;
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
                
                ULP frame = send_login_data(ulpOperation.LOGIN, _login_port);
                switch (frame.OperationStatus) {
                    case ulpOperStatus.SUCCESS:
                        {   
                            var token = frame.data;
                            MessageBox.Show(token);
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
            switch (send_login_data(ulpOperation.REGISTER, _login_port).OperationStatus)
            {
                case ulpOperStatus.SUCCESS:
                    {
                        MessageBox.Show("you can now log in");
                    }
                    break;
                
                case ulpOperStatus.WRONG_LOGIN:
                    {
                        MessageBox.Show("Login you used is taken, please choose different login");
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
