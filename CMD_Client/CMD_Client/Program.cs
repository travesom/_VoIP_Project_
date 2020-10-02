using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using Protocols;

namespace CMD_Client
{
    class Program
    {
        public static bool ValidateServerCertificate(
          object sender,
          X509Certificate certificate,
          X509Chain chain,
          SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None) return true;
            return false;
        }

        public static string Name;
        public static WaveFileWriter waveFile;
        public static WaveFileReader waveFileReader;

        static void waveSource_DataAvailable(object sender, WaveInEventArgs e)
        {
            waveFile.Write(e.Buffer, 0, e.BytesRecorded);
            waveFile.Flush();
        }

        private static void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            waveFileReader.Dispose();
        }

        #region funkcje
        static void Nagraj()
        {
            // do kogo
            Console.WriteLine("Wpisz login osoby do której chcesz wysłać wiadomość");
            var login = Console.ReadLine();

            // NAudio nagranie mikro
            WaveInEvent waveSource = new WaveInEvent
            {
                //DeviceNumber = 0,
                WaveFormat = new WaveFormat(44100, 1)
            };

            waveSource.DataAvailable += new EventHandler<WaveInEventArgs>(waveSource_DataAvailable);

            string tempFile = login.Replace('.','_') + ".wav";
            waveFile = new WaveFileWriter(tempFile, waveSource.WaveFormat);

            Console.WriteLine("Press enter to start recording");
            Console.ReadLine();

            waveSource.StartRecording();
            Console.WriteLine("Press enter to stop");
            Console.ReadLine();
            waveSource.StopRecording();
            waveFile.Dispose();
            Console.WriteLine("Nagrano wiadomość");

            // wysłanie pliku na serwer

            var IP = IPAddress.Parse("127.0.0.1");
            TcpClient tcpClient = new TcpClient(new IPEndPoint(IP, 0));
            tcpClient.Connect(IPAddress.Parse("127.0.0.1"), 8091);

            var buffer = Encoding.UTF8.GetBytes("UPLOAD " + login);
            tcpClient.GetStream().Write(buffer, 0, buffer.Length);
            tcpClient.GetStream().Read(buffer, 0, 2);

            var audioData = new byte[1000];
            var file = File.OpenRead(tempFile);
            if (file.CanRead) Console.WriteLine("wysyłam");
            while(file.Position != file.Length)
            {
                var lenght = file.Read(audioData, 0, 1000);
                tcpClient.GetStream().Write(audioData, 0, lenght);
            }
            Console.WriteLine("wyslano");
            tcpClient.Close();
            file.Close();
            Console.WriteLine("\n");
        }

        static void Sprawdz()
        {
            var IP = IPAddress.Parse("127.0.0.1");
            TcpClient tcpClient = new TcpClient(new IPEndPoint(IP, 0));
            tcpClient.Connect(IPAddress.Parse("127.0.0.1"), 8092);
            string data = "CHECK " + Name;
            var buffer = Encoding.UTF8.GetBytes(data);
            var liczbaAudio = new byte[2]; 
            tcpClient.GetStream().Write(buffer, 0, buffer.Length);
            tcpClient.GetStream().Read(liczbaAudio, 0, 2);
            var liczba = int.Parse(Encoding.UTF8.GetString(liczbaAudio));
            Console.WriteLine("Masz nieodebranych: {0} wiadomości", liczba);
            Console.WriteLine("\n");
        }

        static void Odbierz()
        {
            var IP = IPAddress.Parse("127.0.0.1");
            TcpClient tcpClient = new TcpClient(new IPEndPoint(IP, 0));
            tcpClient.Connect(IPAddress.Parse("127.0.0.1"), 8092);
            string data = "GET " + Name;
            var buffer = Encoding.UTF8.GetBytes(data);

            tcpClient.GetStream().Write(buffer,0,buffer.Length);

            int stop;
            
            FileStream file = File.Create("odbior.wav");
            do
            {
                var voice = new byte[1000];
                stop = tcpClient.GetStream().Read(voice, 0, 1000);
                if (stop != 0) file.Write(voice, 0, stop);
            } while (stop != 0);
            file.Flush();

            tcpClient.Close();
            file.Close();
            //odtworz audio

            var waveOut = new WaveOutEvent();
            waveOut.PlaybackStopped += OnPlaybackStopped;
            waveFileReader = new WaveFileReader("odbior.wav");
            waveOut.Init(waveFileReader);
            waveOut.Play();
            Console.WriteLine("\n");
        }

        

        static void Pomoc()
        {
            Console.WriteLine("Pomoc programu");
            Console.WriteLine("nagraj - umożliwia nagranie wiadomości dla konkretnej osoby");
            Console.WriteLine("sprawdź - umożliwia sprawdzenie czy masz jakieś nieodebrane wiadomości");
            Console.WriteLine("odbierz - odbiera pojedynczo kolejne wiadomości");
            Console.WriteLine("wyloguj - wylogowuje użytkownika");
        }

        static void Wyloguj(string token)
        {
            var IP = IPAddress.Parse("127.0.0.1");
            TcpClient tcpClient = new TcpClient(new IPEndPoint(IP, 0));
            tcpClient.Connect(IPAddress.Parse("127.0.0.1"), 8088);
            SslStream sslClient = new SslStream(tcpClient.GetStream(), false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);
            try
            {
                sslClient.AuthenticateAsClient("TIPserver");
                Console.WriteLine("Uwierzytelniono");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            NVCP cmd = new NVCP(nvcpOperation.MY_STATUS, nvcpOperStatus.UNAVAILABLE, Name + ' ' + token);
            string msg = cmd.ToString();
            sslClient.Write(ASCIIEncoding.ASCII.GetBytes(msg), 0, msg.Length);
            byte[] tab = new byte[150];
            int i = sslClient.Read(tab, 0, 150);
            NVCP result = new NVCP(Encoding.ASCII.GetString(tab));
            //Console.WriteLine(Encoding.ASCII.GetString(tab),0,i);
            //Console.WriteLine("Odebrano: " + result.ToString());
            //Console.WriteLine("");
            sslClient.Close();
            tcpClient.Close();
            Environment.Exit(0);
        }
        #endregion

        static string Logowanie()
        {
            ////////////////////////////////////////
            Console.WriteLine("Zaloguj się podając login i hasło:");
            var login = Console.ReadLine();
            var haslo = Console.ReadLine();
            Name = login;

            var data = new StringBuilder(login);
            data.Append(" ");
            data.Append(haslo);
            /////////////////////////////////////////

            var IP = IPAddress.Parse("127.0.0.1");
            TcpClient tcpClient = new TcpClient(new IPEndPoint(IP, 0));
            tcpClient.Connect(IPAddress.Parse("127.0.0.1"), 8086);
            SslStream sslClient = new SslStream(tcpClient.GetStream(), false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null);
            try
            {
                sslClient.AuthenticateAsClient("TIPserver");
                //Console.WriteLine("Uwierzytelniono");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            ULP cmd = new ULP(ulpOperation.LOGIN, data.ToString());
            //Console.WriteLine("Wysłano: " + cmd.ToString());

            string msg = cmd.ToString();
            sslClient.Write(ASCIIEncoding.ASCII.GetBytes(msg), 0, msg.Length);
            byte[] tab = new byte[150];
            int i = sslClient.Read(tab, 0, 150);
            ULP result = new ULP(Encoding.ASCII.GetString(tab));
            Console.WriteLine(result.OperationStatus.ToString());
            //Console.WriteLine(Encoding.ASCII.GetString(tab),0,i);
            //Console.WriteLine("Odebrano: " + result.ToString());
            //Console.WriteLine("");
            sslClient.Close();
            tcpClient.Close();
            return result.data;
        }

        static void Menu(string token)
        {
            Console.WriteLine("Dzień dobry");
            Console.WriteLine("Wybierz operację:");
            Console.WriteLine("nagraj");
            Console.WriteLine("sprawdz");
            Console.WriteLine("odbierz");
            Console.WriteLine("pomoc");
            Console.WriteLine("wyloguj");
            Console.WriteLine("\n");

            var opcja = Console.ReadLine();
            switch (opcja)
            {
                case "nagraj": Nagraj(); break;
                case "sprawdz": Sprawdz(); break;
                case "odbierz": Odbierz(); break;
                case "pomoc": Pomoc(); break;
                case "wyloguj": Wyloguj(token); break;
                default: Console.WriteLine("niepoprawna komenda"); break;
            }
        }

        static void Main(string[] args)
        {
            var token = Logowanie();
            Console.WriteLine(token);
            Console.WriteLine("\n");

            while(true)
            {
                Menu(token);
            }    
        }
    }
}
