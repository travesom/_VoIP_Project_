using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using System.Security.Cryptography;
using System.Net.Sockets;
using Protocols;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Text.RegularExpressions;
using System.Security;

namespace Telefon_serwer
{


    /// <summary>
    /// port 8086 - login/register <para/>
    /// port 8087 - voice and text communication <para/>
    /// port 8088 - control status, communication <para/>
    /// port 8089 - send xml's
    /// </summary>
    class Program
    {
        public static ActiveSubscriberList asList = new ActiveSubscriberList();
        public static ConnectionsList connectList = new ConnectionsList();
        public static UserAccountList uaList = new UserAccountList();

        public static StreamWriter LogRegister;
        public static X509Certificate2 serverCertificate = null;

        #region Server Tasks
        static async Task loginTask(string ipAddr)
        {             
            TcpListener server = new TcpListener(IPAddress.Parse(ipAddr), 8086);
            server.Start();
            while (true)
            {
                TcpClient client = await server.AcceptTcpClientAsync();
                SslStream sslClient = new SslStream(client.GetStream(), false);
                try
                {
                    sslClient.AuthenticateAsServer(serverCertificate, false, false);
                    //Console.WriteLine("Uwierzyleniono");
                }
                catch(Exception)
                {
                    //Console.WriteLine(ex.Message);
                    await LogRegister.WriteLineAsync(DateTime.Now + ": SSL: failed to authenticate");
                }
                Random r1 = new Random();
                byte[] buffer = new byte[1500];
                //Console.WriteLine(client.Client.RemoteEndPoint.ToString());
                await sslClient.ReadAsync(buffer, 0, buffer.Length).ContinueWith(
                    async (lenght) =>
                    {
                        ULP frame = new ULP(Encoding.ASCII.GetString(buffer, 0, lenght.Result));
                        string data = frame.data;

                        ULP sendFrame = new ULP(frame.OperationType,"");
                        sendFrame.ProtocolStatus = IStatus.OK;
                        //Console.WriteLine(frame.OperationType.ToString());         
                        switch (frame.OperationType)
                        {
                            case ulpOperation.LOGIN:
                                {

                                    string login=""; 
                                    string passwd="";
                                    try
                                    {
                                        login = data.Split(' ')[0];
                                        passwd = data.Split(' ')[1];
                                        Regex rgx = new Regex(@"^[A-Za-z][A-Za-z0-9_\.]*@[A-Za-z]*\.[a-z]{2,3}$");
                                        if (!rgx.IsMatch(login))
                                        {
                                            sendFrame.ProtocolStatus = IStatus.DATA_FAIL;
                                            await LogRegister.WriteLineAsync(DateTime.Now + ": Protocol: " + "error: wrong dataframe");
                                            goto labelSend;
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        sendFrame.ProtocolStatus = IStatus.DATA_FAIL;
                                        await LogRegister.WriteLineAsync(DateTime.Now + ": Protocol: " + "error: wrong dataframe");
                                        goto labelSend;
                                    }
                                    byte[] hash;
                                    
                                    using (SHA256 sha256 = SHA256.Create())
                                    {
                                        hash = sha256.ComputeHash(Encoding.ASCII.GetBytes(passwd));
                                    }
                                    
                                    int result = uaList.login(login, Convert.ToBase64String(hash));
                                    
                                    if (result==0)
                                    {
                                        if (asList.add(login, new SubscriberItem(((IPEndPoint)client.Client.RemoteEndPoint), nvcpOperStatus.READY, DateTime.Now)) == 0)
                                        {
                                            sendFrame.OperationStatus = ulpOperStatus.SUCCESS;
                                            int rnd = r1.Next(1000000,9999999);
                                            sendFrame.data = rnd.ToString();
                                            asList[login].Token = rnd;
                                            await LogRegister.WriteLineAsync(DateTime.Now + ": Account: " + login + " logged in");
                                        }
                                        else
                                        {
                                            sendFrame.OperationStatus = ulpOperStatus.WRONG_LOGIN;
                                            await LogRegister.WriteLineAsync(DateTime.Now + ": Account: " + login + ": already logged in ");
                                        }
                                    }
                                    else if(result == 1)
                                    {
                                        sendFrame.OperationStatus = ulpOperStatus.WRONG_PASS;
                                        await LogRegister.WriteLineAsync(DateTime.Now + ": Account: error: wrong password");
                                    }
                                    else
                                    {
                                        sendFrame.OperationStatus = ulpOperStatus.WRONG_LOGIN;
                                        await LogRegister.WriteLineAsync(DateTime.Now + ": Account: error: there is no account ID: " + login);
                                    }
                                }
                                break;

                            case ulpOperation.REGISTER:
                                {
                                    string login = "";
                                    string passwd = "";
                                    string nickname = "null";
                                    try
                                    {
                                        login = data.Split(' ')[0];
                                        passwd = data.Split(' ')[1];
                                        try
                                        {
                                            nickname = data.Split(' ')[2];
                                        }
                                        catch (Exception) { }
                                        Regex rgx = new Regex(@"^[A-Za-z][A-Za-z0-9_\.]*@[A-Za-z]*\.[a-z]{2,3}$");
                                        if (!rgx.IsMatch(login))
                                        {
                                            sendFrame.ProtocolStatus = IStatus.DATA_FAIL;
                                            await LogRegister.WriteLineAsync(DateTime.Now + ": Protocol: " + "error: wrong dataframe");
                                            goto labelSend;
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        sendFrame.ProtocolStatus = IStatus.DATA_FAIL;
                                        await LogRegister.WriteLineAsync(DateTime.Now + ": Protocol: " + "error: wrong dataframe");
                                        goto labelSend;
                                    }
                                    byte[] hash;
                                    using (SHA256 sha256 = SHA256.Create())
                                    {
                                        hash = sha256.ComputeHash(Encoding.ASCII.GetBytes(passwd));
                                    }
                                    if (uaList.createAccount(login, Convert.ToBase64String(hash), nickname) == 0)
                                        {
                                            sendFrame.OperationStatus = ulpOperStatus.SUCCESS;
                                            await LogRegister.WriteLineAsync(DateTime.Now + ": Account: " + "succesfully registered: " + login);
                                        }
                                    else
                                        {
                                            sendFrame.OperationStatus = ulpOperStatus.WRONG_LOGIN;
                                            await LogRegister.WriteLineAsync(DateTime.Now + ": Account: " + "error: account already exist");
                                    }
                                }
                                break;

                            case ulpOperation.CHANGE_DATA:
                                {
                                    string[] tab = data.Split(' ');
                                        if (asList.exist(tab[0]))
                                        {
                                            if (asList[tab[0]].Token == int.Parse(tab[tab.Length - 1]))
                                            {
                                                int rnd = r1.Next(1000000, 9999999);
                                                if (tab.Length == 4)
                                                {
                                                    byte[] hash;
                                                    byte[] newHash;
                                                    using (SHA256 sha256 = SHA256.Create())
                                                    {
                                                       hash = sha256.ComputeHash(Encoding.ASCII.GetBytes(tab[1]));
                                                       newHash = sha256.ComputeHash(Encoding.ASCII.GetBytes(tab[2]));
                                                    }
                                                    var result = uaList.changePassword(tab[0], Convert.ToBase64String(hash), Convert.ToBase64String(newHash));
                                                    if ( result == 0)
                                                    {
                                                        asList[tab[0]].Token = rnd;
                                                        sendFrame.data = rnd.ToString();
                                                        sendFrame.OperationStatus = ulpOperStatus.SUCCESS;
                                                        //uaList.writeToXML();
                                                        await LogRegister.WriteLineAsync(DateTime.Now + ": Account: " + "succesfully changed password");
                                                    }
                                                    else if (result == 1)
                                                    {
                                                        asList[tab[0]].Token = rnd;
                                                        sendFrame.data = rnd.ToString();
                                                        sendFrame.OperationStatus = ulpOperStatus.WRONG_LOGIN;
                                                        await LogRegister.WriteLineAsync(DateTime.Now + ": Account: " + "error: cannot changed password");
                                                    }
                                                    else
                                                    {
                                                        asList[tab[0]].Token = rnd;
                                                        sendFrame.data = rnd.ToString();
                                                        sendFrame.OperationStatus = ulpOperStatus.WRONG_PASS;
                                                        await LogRegister.WriteLineAsync(DateTime.Now + ": Account: " + "error: cannot changed password");
                                                    }
                                                }
                                                else
                                                {
                                                    if (tab.Length == 3)
                                                    {
                                                        if (uaList.changeNickName(tab[0], tab[1]) == 0)
                                                        {
                                                            asList[tab[0]].Token = rnd;
                                                            sendFrame.data = rnd.ToString();
                                                            sendFrame.OperationStatus = ulpOperStatus.SUCCESS;
                                                            await LogRegister.WriteLineAsync(DateTime.Now + ": Account: " + "succesfully changed nick");
                                                        }
                                                        else
                                                        {
                                                            asList[tab[0]].Token = rnd;
                                                            sendFrame.data = rnd.ToString();
                                                            sendFrame.OperationStatus = ulpOperStatus.WRONG_LOGIN;
                                                            await LogRegister.WriteLineAsync(DateTime.Now + ": Account: " + "error: cannot changed nick");
                                                        }
                                                    }
                                                    else
                                                    {
                                                        asList[tab[0]].Token = rnd;
                                                        sendFrame.data = rnd.ToString();
                                                        sendFrame.ProtocolStatus = IStatus.DATA_FAIL;
                                                        await LogRegister.WriteLineAsync(DateTime.Now + ": Protocol: " + "error: wrong data");
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                sendFrame.ProtocolStatus = IStatus.DATA_FAIL;
                                                await LogRegister.WriteLineAsync(DateTime.Now + ": Autorization: " + "error: wrong token ");
                                            }
                                        }
                                        else
                                        {
                                            sendFrame.OperationStatus = ulpOperStatus.WRONG_LOGIN;
                                            await LogRegister.WriteLineAsync(DateTime.Now + ": Account: " + "error: cannot change nick to user not log in");
                                        }                            
                                }
                                break;
                            default:
                                {
                                    sendFrame.ProtocolStatus = IStatus.OPER_FAIL;
                                    await LogRegister.WriteLineAsync(DateTime.Now + ": Protocol: " + "error: wrong dataframe");
                                }
                                break;
                        }
                        labelSend:
                        byte[] sendBytes = Encoding.ASCII.GetBytes(sendFrame.ToString());
                        await sslClient.WriteAsync(sendBytes, 0, sendBytes.Length);
                    });
            }
        }

        static async Task voiceTask()
        {
            UdpClient server = new UdpClient(8087);
            
            while (true)
            {
                await server.ReceiveAsync().ContinueWith(
                    async (task) =>
                    {
                        NVP frame = new NVP(Encoding.ASCII.GetString(task.Result.Buffer));
                        if (asList.exist(frame.To))
                        {
                            await server.SendAsync(task.Result.Buffer, task.Result.Buffer.Length, asList[frame.To].Address);
                        }                           
                    });
            }
        }

        static async Task controlTask(string ipAddr)
        {
            TcpListener server = new TcpListener(IPAddress.Parse(ipAddr), 8088);
            server.Start();
            while (true)
            {
                TcpClient client = await server.AcceptTcpClientAsync();
                SslStream sslClient = new SslStream(client.GetStream(), false);
                try
                {
                    sslClient.AuthenticateAsServer(serverCertificate, false, false);
                    //Console.WriteLine("Uwierzyleniono");
                }
                catch (Exception)
                {
                    await LogRegister.WriteLineAsync(DateTime.Now + ": SSL: failed to authenticate");
                }
                Random r1 = new Random();
                byte[] buffer = new byte[1500];
                await sslClient.ReadAsync(buffer, 0, buffer.Length).ContinueWith(
                    async (lenght) =>
                    {
                        NVCP frame = new NVCP(Encoding.ASCII.GetString(buffer, 0, lenght.Result));
                        string data = frame.data;

                        NVCP sendFrame = new NVCP(frame.OperationType, "");
                        sendFrame.ProtocolStatus = IStatus.OK;
                        //sendFrame.ProtocolStatus = frame.ProtocolStatus;

                        switch(frame.OperationType)
                        {
                            case nvcpOperation.AVABILITY:
                                {
                                    if (data.Split(' ').Length == 2)
                                    {
                                        var my_login = data.Split(' ')[0];
                                        var other_login = data.Split(' ')[1];
                                        if (asList.exist(my_login))
                                        {
                                            if (asList.exist(other_login))
                                            {
                                                sendFrame.OperationStatus = asList[other_login].Status;
                                                sendFrame.OperationType = nvcpOperation.AVABILITY;
                                            }
                                            else
                                            {
                                                sendFrame.OperationStatus = nvcpOperStatus.UNAVAILABLE;
                                            }
                                        }
                                        else
                                        {
                                            sendFrame.OperationStatus = nvcpOperStatus.NONE;
                                            await LogRegister.WriteLineAsync(DateTime.Now + ": Connection: " + "error: try to check avability without being logged in");
                                        }
                                    }
                                    else
                                    {
                                        sendFrame.ProtocolStatus = IStatus.DATA_FAIL;
                                    }
                                    sendFrame.TimeStamp = DateTime.Now;
                                }
                                break;
                            case nvcpOperation.CONNECT:
                                {

                                    if (data.Split(' ').Length == 3)
                                    {
                                        var my_login = data.Split(' ')[0];
                                        var other_login = data.Split(' ')[1];
                                        var token = int.Parse(data.Split(' ')[2]);

                                        NVCP sendFrame2 = new NVCP(frame.OperationType, "");
                                        if (asList.exist(my_login))
                                        {
                                            if (token == asList[my_login].Token)
                                            {
                                                if (asList.exist(other_login))
                                                {
                                                    if (DateTime.Now.Subtract(asList[other_login].Time).CompareTo(TimeSpan.FromSeconds(5.0)) < 1 && asList[other_login].Status == nvcpOperStatus.READY)
                                                    {
                                                        var rng = r1.Next(1000000, 9999999);
                                                        var rng2 = r1.Next(1000000, 9999999);

                                                        sendFrame.OperationStatus = nvcpOperStatus.WAITING_CONNECTION;
                                                        sendFrame.TimeStamp = DateTime.Now;
                                                        sendFrame.data = other_login + ' ' + rng.ToString();

                                                        asList[my_login].Token = rng;

                                                        sendFrame2.OperationType = nvcpOperation.CONNECT;
                                                        sendFrame2.OperationStatus = nvcpOperStatus.WAITING_CONNECTION;
                                                        sendFrame2.TimeStamp = DateTime.Now;
                                                        sendFrame2.ProtocolStatus = IStatus.OK;
                                                        sendFrame2.data = my_login + ' ' + rng2.ToString();

                                                        asList[other_login].Token = rng2;

                                                        TcpClient second = new TcpClient(asList[other_login].Address);
                                                        SslStream sslClient2 = new SslStream(second.GetStream(), false);
                                                        try
                                                        {
                                                            sslClient2.AuthenticateAsServer(serverCertificate, false, false);
                                                            //Console.WriteLine("Uwierzyleniono");
                                                        }
                                                        catch (Exception)
                                                        {
                                                            await LogRegister.WriteLineAsync(DateTime.Now + ": SSL: failed to authenticate");
                                                        }
                                                        var sendBytes2 = Encoding.ASCII.GetBytes(sendFrame2.ToString());
                                                        await sslClient2.WriteAsync(sendBytes2, 0, sendBytes2.Length);
                                                    }
                                                    else
                                                    {
                                                        sendFrame.OperationStatus = nvcpOperStatus.UNAVAILABLE;
                                                        sendFrame.TimeStamp = DateTime.Now;
                                                        if(DateTime.Now.Subtract(asList[other_login].Time).CompareTo(TimeSpan.FromSeconds(5.0)) < 1) asList.remove(other_login);
                                                    }
                                                }
                                                else
                                                {
                                                    sendFrame.OperationStatus = nvcpOperStatus.UNAVAILABLE;
                                                    sendFrame.TimeStamp = DateTime.Now;
                                                    await LogRegister.WriteLineAsync(DateTime.Now + ": Connection: " + "error: try to connect user doesn't exists");
                                                }
                                            }
                                            else
                                            {
                                                sendFrame.OperationStatus = nvcpOperStatus.NONE;
                                                sendFrame.ProtocolStatus = IStatus.DATA_FAIL;
                                                sendFrame.TimeStamp = DateTime.Now;
                                                await LogRegister.WriteLineAsync(DateTime.Now + ": Protocol: " + "error: wrong token");
                                            }
                                        }
                                        else
                                        {
                                            sendFrame.OperationStatus = nvcpOperStatus.NONE;
                                            sendFrame.TimeStamp = DateTime.Now;
                                            await LogRegister.WriteLineAsync(DateTime.Now + ": Connection: " + "error: try to connect user doesn't logged in");
                                        }
                                    }
                                    else
                                    {
                                        sendFrame.ProtocolStatus = IStatus.DATA_FAIL;
                                        sendFrame.TimeStamp = DateTime.Now;
                                    }
                                }
                                break;
                            case nvcpOperation.MY_STATUS:
                                {
                                    if (data.Split(' ').Length == 2)
                                    {
                                        var my_login = data.Split(' ')[0];
                                        var token = int.Parse(data.Split(' ')[1]);
                                        if (asList.exist(my_login))
                                        {
                                            if (asList[my_login].Token == token)
                                            {
                                                var rng = r1.Next(1000000, 9999999);
                                                if (frame.OperationStatus != nvcpOperStatus.UNAVAILABLE) 
                                                {
                                                    asList[my_login].Status = frame.OperationStatus;
                                                    asList[my_login].Address = (IPEndPoint)client.Client.RemoteEndPoint;
                                                    asList[my_login].Token = rng;
                                                }
                                                else
                                                {
                                                    asList.remove(my_login);
                                                }
                                                sendFrame.OperationStatus = frame.OperationStatus;
                                                sendFrame.TimeStamp = DateTime.Now;
                                                sendFrame.data = rng.ToString();
                                            }
                                            else
                                            {
                                                sendFrame.OperationStatus = nvcpOperStatus.NONE;
                                                sendFrame.OperationType = nvcpOperation.MY_STATUS;
                                                sendFrame.ProtocolStatus = IStatus.DATA_FAIL;
                                                await LogRegister.WriteLineAsync(DateTime.Now + ": Protocol: " + "error: wrong token");
                                            }
                                        }
                                        else
                                        {
                                            sendFrame.OperationStatus = nvcpOperStatus.NONE;
                                            sendFrame.OperationType = nvcpOperation.MY_STATUS;
                                            sendFrame.ProtocolStatus = IStatus.OTHER_FAIL;
                                            sendFrame.TimeStamp = DateTime.Now;
                                            await LogRegister.WriteLineAsync(DateTime.Now + ": User Status: " + "error: " + my_login + " send status without being logged in");
                                        }
                                    }
                                    else
                                    {
                                        sendFrame.ProtocolStatus = IStatus.DATA_FAIL;
                                        sendFrame.TimeStamp = DateTime.Now;
                                        await LogRegister.WriteLineAsync(DateTime.Now + ": User Status: " + "error: wrong dataframe");
                                    }
                                }
                                break;
                            default:
                                {
                                    sendFrame.ProtocolStatus = IStatus.OPER_FAIL;
                                    sendFrame.TimeStamp = DateTime.Now;
                                    await LogRegister.WriteLineAsync(DateTime.Now + ": Protocol: " + "error: wrong dataframe");
                                }
                                break;
                        }

                        byte[] sendBytes = Encoding.ASCII.GetBytes(sendFrame.ToString());
                        await sslClient.WriteAsync(sendBytes, 0, sendBytes.Length);                      
                    });
            }
        }

        static async Task contactListTask(string ipAddr)
        {
            TcpListener server = new TcpListener(IPAddress.Parse(ipAddr), 8089);
            server.Start();
            while (true)
            {
                TcpClient client = await server.AcceptTcpClientAsync();
                SslStream sslClient = new SslStream(client.GetStream(), false);
                try
                {
                    sslClient.AuthenticateAsServer(serverCertificate, false, false);
                    //Console.WriteLine("Uwierzyleniono");
                }
                catch (Exception)
                {
                    await LogRegister.WriteLineAsync(DateTime.Now + ": SSL: failed to authenticate");
                }
                Random r1 = new Random();
                byte[] buffer = new byte[1500];
                await sslClient.ReadAsync(buffer, 0, buffer.Length).ContinueWith(
                    async (lenght) =>
                    {
                        // 'login' 'token' 
                        var tab = Encoding.ASCII.GetString(buffer).Split(' ');
                        if (tab.Length == 2)
                        {
                            if (asList.exist(tab[0]))
                                {
                                if (asList[tab[0]].Token == int.Parse(tab[1]))
                                {
                                    byte[] xmlFile = File.ReadAllBytes(tab[0].Replace('@','-') + "_contact.xml");
                                    int rnd = r1.Next(1000000, 9999999);
                                    asList[tab[0]].Token = rnd;
                                    await sslClient.WriteAsync(Encoding.ASCII.GetBytes(rnd.ToString()), 0, 7);
                                    await sslClient.WriteAsync(xmlFile, 0, xmlFile.Length);
                                }
                                else
                                {
                                    string s = "error";
                                    await sslClient.WriteAsync(Encoding.ASCII.GetBytes(s), 0, s.Length);
                                }
                            }
                            else
                            {
                                string s = "error";
                                await sslClient.WriteAsync(Encoding.ASCII.GetBytes(s), 0, s.Length);
                            }
                        } 
                        else if (tab.Length == 3)
                        {
                            // 'login' 'token' 'data'
                            try
                            {
                                if (asList[tab[0]].Token == int.Parse(tab[1]))
                                {
                                    File.WriteAllText(tab[0].Replace('@','-') + "_contact.xml", tab[2]);
                                }
                            }
                            catch (Exception) { }
                        }
                        else
                        {
                            string s = "error";
                            await sslClient.WriteAsync(Encoding.ASCII.GetBytes(s), 0, s.Length);
                        }
                    });
            }
        }

        static async Task AudioUploadTask(string ipAddr)
        {
            TcpListener server = new TcpListener(IPAddress.Parse(ipAddr), 8091);
            server.Start();
            while (true)
            {
                TcpClient client = await server.AcceptTcpClientAsync();
                
                byte[] buffer = new byte[150];
                //Console.WriteLine(client.Client.RemoteEndPoint.ToString());
                await client.GetStream().ReadAsync(buffer, 0, buffer.Length).ContinueWith(
                    (lenght) =>
                    {
                        var data = Encoding.UTF8.GetString(buffer);
                        var table = data.Substring(0,lenght.Result).Split(' ');
                        if ( table[0] == "UPLOAD")
                        {
                            var fileName = table[1].Replace('.','_') + '_' + DateTime.Now.Ticks.ToString() + ".wav";

                            var response = Encoding.UTF8.GetBytes("OK");
                            client.GetStream().Write(response,0,response.Length);

                            int stop;
                            var voice = new byte[1000];
                            
                            FileStream file = File.Create(fileName);
                            do
                            {
                                stop = client.GetStream().Read(voice, 0, 1000);
                                if(stop != 0) file.Write(voice, 0, stop);
                            } while (stop != 0);
                            file.Flush();
                            file.Close();
                        }
                        else
                        {
                            LogRegister.WriteLine("wrong upload procedure");
                        }
                    });
            }
        }

        static async Task AudioDownloadTask(string ipAddr)
        {
            TcpListener server = new TcpListener(IPAddress.Parse(ipAddr), 8092);
            server.Start();
            while (true)
            {
                TcpClient client = await server.AcceptTcpClientAsync();

                byte[] buffer = new byte[150];
                //Console.WriteLine(client.Client.RemoteEndPoint.ToString());
                await client.GetStream().ReadAsync(buffer, 0, buffer.Length).ContinueWith(
                    (lenght) =>
                    {
                        var data = Encoding.UTF8.GetString(buffer).Substring(0, lenght.Result);
                        var table = data.Split(' ');
                        switch (table[0])
                        {
                            case "CHECK":
                                {
                                    var name = table[1].Replace('.', '_') + '*';
                                    var files = Directory.GetFiles(Directory.GetCurrentDirectory(), name);
                                    if(files.Length > 9) client.GetStream().Write(Encoding.UTF8.GetBytes(files.Length.ToString()), 0, 2);
                                    else client.GetStream().Write(Encoding.UTF8.GetBytes(files.Length.ToString()), 0, 1);
                                } break;
                            case "GET":
                                {
                                    var name = table[1].Replace('.', '_') + '*';
                                    var files = Directory.GetFiles(Directory.GetCurrentDirectory(), name);
                                    if (files.Length != 0)
                                    {
                                        var audioFile = File.OpenRead(files[0]);
                                        while (audioFile.Position != audioFile.Length)
                                        {
                                            var audioData = new byte[1000];
                                            var len = audioFile.Read(audioData, 0, 1000);
                                            client.GetStream().Write(audioData, 0, len);
                                        }
                                        audioFile.Close();
                                        File.Delete(files[0]);
                                    }
                                    
                                }
                                break;
                        }
                        client.Close();
                    });
            }
        }
        #endregion
        static void GetAllAccounts()
        {
            Console.WriteLine("All accounts:");
            foreach (var i in uaList)
            {
                Console.WriteLine(i.Key + ' ' + i.Value.NickName + ' ' + i.Value.PasswordSHA);
            }
        }

        static void GetAllActiveSubscribers()
        {
            Console.WriteLine("All accounts:");
            foreach (var i in asList)
            {
                Console.WriteLine(i.Key + ' ' + i.Value.Status + ' ' + i.Value.Token);
            }
        }

        static Task controlShellTask()
        {
            while (true)
            {
                loginlabel:
                string securePwd = "";
                ConsoleKeyInfo key;

                Console.Write("Please log in: ");
                do
                {
                    key = Console.ReadKey(true);

                    // Ignore any key out of range.
                    if (((int)key.Key) >= 65 && ((int)key.Key <= 120))
                    {
                        // Append the character to the password.
                        securePwd += (key.KeyChar);
                    }
                    // Exit if Enter key is pressed.
                } while (key.Key != ConsoleKey.Enter);
                Console.WriteLine();
                if (securePwd == "admin")
                {
                    while (true)
                    {
                        Console.Write("\nZ:\\> ");
                        string line = Console.ReadLine();
                        var tab = line.Split(' ');
                        switch (tab[0])
                        {
                            case "get":
                                {
                                    try
                                    {
                                        switch (tab[1])
                                        {
                                            case "account":
                                                {
                                                    try
                                                    {
                                                        switch (tab[2])
                                                        {
                                                            case "--all": GetAllAccounts(); break;
                                                            case "": break;
                                                            default:
                                                                {
                                                                    try
                                                                    {
                                                                        Console.WriteLine(tab[2] + ' ' + uaList[tab[2]]);
                                                                    }
                                                                    catch (Exception)
                                                                    {
                                                                        Console.WriteLine("No account exists");
                                                                    }
                                                                }
                                                                break;
                                                        }
                                                    }
                                                    catch (Exception)
                                                    {
                                                        Console.WriteLine("error: get account [<name> | --all] ");
                                                    }
                                                }
                                                break;
                                            case "subscriber":
                                                {
                                                    try
                                                    {
                                                        switch (tab[2])
                                                        {
                                                            case "--all": GetAllActiveSubscribers(); break;
                                                            case "--type":
                                                                {
                                                                    try
                                                                    {
                                                                        switch (tab[3])
                                                                        {
                                                                            case "READY":
                                                                                {
                                                                                    Console.WriteLine("READY:");
                                                                                    foreach (var i in asList)
                                                                                    {
                                                                                        if (i.Value.Status == nvcpOperStatus.READY)
                                                                                        {
                                                                                            Console.WriteLine(i.Key + ' ' + i.Value.Token);
                                                                                        }
                                                                                    }
                                                                                }
                                                                                break;
                                                                            case "BUSY":
                                                                                {
                                                                                    Console.WriteLine("BUSY:");
                                                                                    foreach (var i in asList)
                                                                                    {
                                                                                        if (i.Value.Status == nvcpOperStatus.BUSY || i.Value.Status == nvcpOperStatus.WAITING_CONNECTION)
                                                                                        {
                                                                                            Console.WriteLine(i.Key + ' ' + i.Value.Token);
                                                                                        }
                                                                                    }
                                                                                }
                                                                                break;
                                                                            default: Console.WriteLine("Invalid type"); break;
                                                                        }
                                                                    }
                                                                    catch (Exception)
                                                                    {
                                                                        Console.WriteLine("get subscriber --type <typeName>");
                                                                    }
                                                                }
                                                                break;
                                                            case "": break;
                                                            default:
                                                                {
                                                                    try
                                                                    {
                                                                        Console.WriteLine(tab[2] + ' ' + asList[tab[2]].Status + ' ' + asList[tab[2]].Address.Address + ':' + asList[tab[2]].Address.Port);
                                                                    }
                                                                    catch (Exception)
                                                                    {
                                                                        Console.WriteLine("No active subscriber");
                                                                    }
                                                                }
                                                                break;
                                                        }
                                                    }
                                                    catch (Exception)
                                                    {
                                                        Console.WriteLine("get subscriber [<name> | --all | --type <typeName>] ");
                                                    }
                                                }
                                                break;
                                            default: Console.WriteLine("get [subscriber | account]"); break;
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        Console.WriteLine("error: get [subscriber | account]"); break;
                                    }
                                }
                                break;

                            case "delete":
                                {
                                    try
                                    {
                                        switch (tab[1])
                                        {
                                            case "account":
                                                {
                                                    try
                                                    {
                                                        switch (tab[2])
                                                        {
                                                            case "--all":
                                                                {
                                                                    uaList.clear();
                                                                }
                                                                break;
                                                            case "--nologin":
                                                                {

                                                                    foreach (var elem in uaList)
                                                                    {
                                                                        if (!asList.exist(elem.Key))
                                                                        {
                                                                            uaList.removeAccount(elem.Key);
                                                                        }
                                                                    }
                                                                }
                                                                break;
                                                            default:
                                                                {
                                                                    try
                                                                    {
                                                                        uaList.removeAccount(tab[2]);
                                                                    }
                                                                    catch (Exception)
                                                                    {
                                                                        Console.WriteLine("Account doesn't exist");
                                                                    }
                                                                }
                                                                break;
                                                        }
                                                    }
                                                    catch (Exception)
                                                    {
                                                        Console.WriteLine("get account [<name> | --all | --nologin] ");
                                                    }
                                                }
                                                break;
                                            case "subscriber":
                                                {
                                                    try
                                                    {
                                                        switch (tab[2])
                                                        {
                                                            case "--all":
                                                                {
                                                                    asList.clear();
                                                                }
                                                                break;
                                                            case "--noactive":
                                                                {
                                                                    foreach (var elem in asList)
                                                                    {
                                                                        if (elem.Value.Status == nvcpOperStatus.READY)
                                                                        {
                                                                            asList.remove(elem.Key);
                                                                        }
                                                                    }
                                                                }
                                                                break;
                                                            default:
                                                                {
                                                                    try
                                                                    {
                                                                        asList.remove(tab[2]);
                                                                    }
                                                                    catch (Exception)
                                                                    {
                                                                        Console.WriteLine("Subscriber doesn't exist");
                                                                    }
                                                                }
                                                                break;
                                                        }
                                                    }
                                                    catch (Exception)
                                                    {
                                                        Console.WriteLine("get subscriber [<name> | --all --noactive] ");
                                                    }
                                                }
                                                break;
                                            default: Console.WriteLine("error: detele [subscriber | account]"); break;
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        Console.WriteLine("delete [account | subscriber]");
                                    }
                                }
                                break;
                            case "help":
                                {
                                    try
                                    {
                                        switch (tab[1])
                                        {
                                            case "get":
                                                {
                                                    Console.WriteLine("Get info about subscribers or existing accounts");
                                                    Console.WriteLine("> get account  [<name> | --all] ");
                                                    Console.WriteLine("> get subscriber [<name> | --all | --type <READY | BUSY>] ");

                                                }
                                                break;
                                            case "delete":
                                                {
                                                    Console.WriteLine("Deletes accounts or subscribers");
                                                    Console.WriteLine("> delete account [<name> | --all | --nologin]");
                                                    Console.WriteLine("> delete subscriber [<name> | --all | --noactive]");

                                                }
                                                break;
                                            case "time":
                                                {
                                                    Console.WriteLine("Shows current time");
                                                }
                                                break;
                                            case "info":
                                                {
                                                    Console.WriteLine("Shows info about server");
                                                }
                                                break;
                                            case "clear":
                                                {
                                                    Console.WriteLine("Clear command window");
                                                }
                                                break;
                                            case "logout":
                                                {
                                                    Console.WriteLine("Logout from terminal");
                                                }
                                                break;
                                            case "shutdown":
                                                {
                                                    Console.WriteLine("Close server with optional return value");
                                                }
                                                break;
                                            default:
                                                {
                                                    Console.WriteLine("Simple server dev console help:");
                                                    Console.WriteLine("> get [account | subscriber] [<name> | --all | --type <name>] ");
                                                    Console.WriteLine("> delete [account | subscriber] [<param>]");
                                                    Console.WriteLine("> time");
                                                    Console.WriteLine("> info");
                                                    Console.WriteLine("> clear");
                                                    Console.WriteLine("> logout");
                                                    Console.WriteLine("> shutdown <return code>");
                                                    Console.WriteLine("> help <command>");
                                                }
                                                break;
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        Console.WriteLine("Simple server dev console help:");
                                        Console.WriteLine("> get [account | subscriber] [<name> | --all | --type <name>] ");
                                        Console.WriteLine("> delete [account | subscriber] [<param>]");
                                        Console.WriteLine("> time");
                                        Console.WriteLine("> info");
                                        Console.WriteLine("> clear");
                                        Console.WriteLine("> logout");
                                        Console.WriteLine("> shutdown <return code>");
                                        Console.WriteLine("> help <command>");
                                    }
                                }
                                break;
                            case "info":
                                {
                                    Console.WriteLine("------< VoIP Server >------");
                                    Console.WriteLine("Simple asynchronous server provides VoIP connections. \n");
                                    Console.WriteLine("Github project:\n> https://github.com/travesom/_VoIP_Project_ \n");
                                    Console.WriteLine("Version 1.05a \n");
                                    Console.WriteLine("Certificate thumbprint:");
                                    Console.WriteLine(serverCertificate.Thumbprint);
                                }
                                break;
                            case "time":
                                {
                                    Console.WriteLine("Current time: " + DateTime.Now);
                                }
                                break;
                            case "logout":
                                {
                                    goto loginlabel;
                                }
                            case "shutdown":
                                {
                                    Console.WriteLine("Are You sure ? y/n");
                                    string shut = Console.ReadLine();
                                    if (shut.ToUpper() == "Y" || shut == "YES")
                                    {
                                        LogRegister.Close();
                                        try
                                        {
                                            Environment.Exit(int.Parse(tab[1]));
                                        }
                                        catch (Exception)
                                        {
                                            Environment.Exit(0);
                                        }
                                    }
                                }
                                break;
                            case "clear": Console.Clear(); break;
                            case "cd": Console.WriteLine("Not this time"); break;
                            default: Console.WriteLine("error"); break;
                        }
                    }
                }
                else { Console.WriteLine("Wrong password"); }
            }
        }

        public static void newlog(object state)
        {
            LogRegister = new StreamWriter("_" +  DateTime.Now.Year.ToString()+ '-' + DateTime.Now.DayOfYear.ToString() + '-' +
                                            DateTime.Now.Hour.ToString() + '-' + DateTime.Now.Minute.ToString() + "_log.log");
            LogRegister.AutoFlush = true;
        }

        public static void updateAccountList(object state)
        {
            uaList.writeToXML();
        }

        static void Main(string[] args)
        {
            
            //uaList.createAccount("Marcin", "0x123456");
            Console.Title = "TIP_server";
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.Clear();
            Console.SetWindowSize(64, 18);
           
            uaList.readFromXML();

            foreach (var file in Directory.GetFiles(Directory.GetCurrentDirectory()))
            {
                FileInfo fi = new FileInfo(file);
                if (fi.CreationTime < DateTime.Now.AddDays(-6) && fi.Name.Contains("_log.log")) fi.Delete();
            }

            //uaList.writeToXML();

            Timer saveLog = new Timer(new TimerCallback(newlog));
            saveLog.Change(0, 300000);

            Timer saveXml = new Timer(new TimerCallback(updateAccountList));
            saveXml.Change(0, 60000);


            // ---------------------< GENERATE X.509 CERTIFICATE >-----------------------
            /*
            RSA rsa = RSA.Create(2048);
            X500DistinguishedName name = new X500DistinguishedName("CN=TIPserver");
            CertificateRequest certReq = new CertificateRequest(name, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            X509Store general = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
            general.Open(OpenFlags.ReadOnly);
            X509Certificate2Collection coll = general.Certificates.Find(X509FindType.FindByIssuerName, "Zlociu Cert Root", false);
            byte[] numSer = { 78, 78, 79, 79, 34, 34, 56, 56 };
            X509Certificate2 X509cert = certReq.Create(coll[0], DateTime.Now, DateTime.Now.AddYears(1), numSer);

            File.WriteAllBytes("ServerCertificateNew.pfx", X509cert.Export(X509ContentType.Pfx, (string)null));
            */
            /*
            using (Process p = new Process())
            {
                p.StartInfo = new ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = @"C:\Program Files (x86)\Windows Kits\10\bin\x86\makecert.exe",
                    Arguments = "-n \"CN=TIPserver,O=Zlociu_cert,OU=Poznan University of Techonology,C=PL\" -pe -sr LocalMachine -a sha256 -m 12 -r -len 2048 -ss My ",
                    UseShellExecute = false,
                    Verb = "runas"  
                };
                p.Start();
            }
            */

            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadOnly);
            X509Certificate2Collection col = store.Certificates.Find(X509FindType.FindByIssuerName, "Zlociu Cert Root", true).Find(X509FindType.FindByTimeValid,DateTime.Now,false);
            //X509Certificate2Collection col = store.Certificates;
            //foreach( X509Certificate2 cert in col)
            //{
            //    Console.WriteLine(cert.Thumbprint);
            //}

            Console.WriteLine("Server certificate {0}", (col[0].NotBefore < DateTime.Now && col[0].NotAfter > DateTime.Now) ? "is valid" :"is invalid");
            if (col.Count != 0) serverCertificate = new X509Certificate2(col[0]);
            //else Console.WriteLine("nie ma");

            // -------------------------------< START TASKS >-----------------------------------
            LogRegister.AutoFlush = true;
            string ipAddr = args[0];
            Console.WriteLine("Server is running on IP address: {0}", args[0]);
            Console.Write("Available ports: ");
            Task t1 = loginTask(ipAddr);
            if ((int)t1.Status < 3) Console.Write(8086 + " ");
            Task t2 = controlTask(ipAddr);
            if ((int)t2.Status < 3) Console.Write(8088 + " ");
            Task t3 = voiceTask();
            if ((int)t3.Status < 3) Console.Write(8087 + " ");
            Task t4 = contactListTask(ipAddr);
            if ((int)t4.Status < 3) Console.Write(8089 + " ");
            

            Task t6 = AudioUploadTask(ipAddr);
            if ((int)t6.Status < 3) Console.Write(8091 + " ");
            Task t7 = AudioDownloadTask(ipAddr);
            if ((int)t7.Status < 3) Console.Write(8092 + " ");

            Console.Write('\n');

            Task t5 = controlShellTask();

            t1.Wait();
            t2.Wait();
            t3.Wait();
            t4.Wait();
            t5.Wait();
            t6.Wait();
            t7.Wait();

        }
    }
}
