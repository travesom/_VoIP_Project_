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

        public static StreamWriter LogRegister = new StreamWriter("first_log.log");
        

        static async Task loginTask(string ipAddr)
        {             
            TcpListener server = new TcpListener(IPAddress.Parse(ipAddr), 8086);
            server.Start();
            while (true)
            {
                TcpClient client = await server.AcceptTcpClientAsync();
                Random r1 = new Random();
                byte[] buffer = new byte[1500];
                //Console.WriteLine(client.Client.RemoteEndPoint.ToString());
                await client.GetStream().ReadAsync(buffer, 0, buffer.Length).ContinueWith(
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
                                    var login = data.Split(' ')[0];
                                    var passwd = data.Split(' ')[1];
                                    int res = uaList.login(login, passwd);
                                    if (res==0)
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
                                    else if(res == 1)
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
                                    if (uaList.createAccount(data.Split(' ')[0], data.Split(' ')[1]) == 0)
                                        {
                                            sendFrame.OperationStatus = ulpOperStatus.SUCCESS;
                                            await LogRegister.WriteLineAsync(DateTime.Now + ": Account: " + "succesfully registered: " + data.Split(' ')[0]);
                                            //uaList.writeToXML();
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
                                        if (asList[tab[0]].Token == int.Parse(tab[tab.Length-1]))
                                        {
                                            int rnd = r1.Next(1000000, 9999999);
                                            if (tab.Length == 4)
                                            {
                                                if (uaList.changePassword(tab[0], tab[1], tab[2]) == 0)
                                                {
                                                    asList[tab[0]].Token = rnd;
                                                    sendFrame.data = rnd.ToString();
                                                    sendFrame.OperationStatus = ulpOperStatus.SUCCESS;
                                                    //uaList.writeToXML();
                                                    await LogRegister.WriteLineAsync(DateTime.Now + ": Account: " + "succesfully changed password");
                                                }
                                                else if (uaList.changePassword(tab[0], tab[1], tab[2]) == 1)
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
                                                        //uaList.writeToXML();
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
                        byte[] sendBytes = Encoding.ASCII.GetBytes(sendFrame.ToString());
                        await client.GetStream().WriteAsync(sendBytes, 0, sendBytes.Length);
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
                Random r1 = new Random();
                byte[] buffer = new byte[1500];
                await client.GetStream().ReadAsync(buffer, 0, buffer.Length).ContinueWith(
                    async (lenght) =>
                    {
                        NVCP frame = new NVCP(Encoding.ASCII.GetString(buffer, 0, lenght.Result));
                        string data = frame.data;

                        NVCP sendFrame = new NVCP(frame.OperationType, "");
                        sendFrame.ProtocolStatus = IStatus.OK;

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
                                                        var sendBytes2 = Encoding.ASCII.GetBytes(sendFrame2.ToString());
                                                        await second.GetStream().WriteAsync(sendBytes2, 0, sendBytes2.Length);
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
                                            await LogRegister.WriteLineAsync(DateTime.Now + ": User Status: " + "error: user send status without being logged in");
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
                        await client.GetStream().WriteAsync(sendBytes, 0, sendBytes.Length);                      
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
                Random r1 = new Random();
                byte[] buffer = new byte[1500];
                await client.GetStream().ReadAsync(buffer, 0, buffer.Length).ContinueWith(
                    async (lenght) =>
                    {
                        // 'login' 'tpken' 
                        var tab = Encoding.ASCII.GetString(buffer).Split(' ');
                        byte[] xmlFile = File.ReadAllBytes(tab[0] + "_contact.xml");
                        await client.GetStream().WriteAsync(xmlFile, 0, 2);
                    });
            }
        }

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
                Console.WriteLine(i.Key + ' ' + i.Value);
            }
        }

        static Task controlShellTask()
        {
            while(true)
            {
                string line = Console.ReadLine();
                var tab = line.Split(' ');
                switch(tab[0])
                {
                    case "get":
                        {
                            switch(tab[1])
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
                                                        catch (Exception ex)
                                                        {
                                                            Console.WriteLine("No account exists");
                                                        }
                                                    }
                                                    break;
                                            }
                                        }
                                        catch(Exception ex)
                                        {
                                            Console.WriteLine("get account [<name> | --all] ");
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
                                                case "": break;
                                                default:
                                                    {
                                                        try
                                                        {
                                                            Console.WriteLine(asList[tab[2]]);
                                                        }
                                                        catch (Exception ex)
                                                        {
                                                            Console.WriteLine("No active subscriber");
                                                        }
                                                    }
                                                    break;
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine("get subscriber [<name> | --all] ");
                                        }
                                    }
                                    break;
                                default: Console.WriteLine("get [subscriber | account]"); break;
                            }
                        }
                        break;
                    case "help":
                        {
                            Console.WriteLine("Simple server dev console help:");
                            Console.WriteLine("> get [account | subscriber] [<name> | --all] ");
                            Console.WriteLine("> help -> to see this helpdesk");
                        }
                        break;
                    case "shutdown":
                        {
                            LogRegister.Close();
                            Environment.Exit(0);
                        }
                        break;
                    default: Console.WriteLine("error");break;
                }

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
            //uaList.writeToXML();

            Timer saveLog = new Timer(new TimerCallback(newlog));
            saveLog.Change(0, 300000);

            Timer saveXml = new Timer(new TimerCallback(updateAccountList));
            saveXml.Change(0, 60000);
            /*
            UserContactList ucList = new UserContactList("Marcin");
            ucList.add("Lukasz", new ContactItem(nick: uaList["Lukasz"]));
            ucList.add("Juliusz", new ContactItem(nick: uaList["Juliusz"], pin: true));
            ucList.writeToXML();

            UserContactList ucList2 = UserContactList.readFromXML("Marcin");
            foreach (var elem in ucList2.ContactList)
            {
                Console.WriteLine("{0} {1}", elem.Key, elem.Value.ToString());
            }
            */
            LogRegister.AutoFlush = true;
            string ipAddr = args[0];
            Console.WriteLine("Server is running on IP address: {0}", args[0]);
            Console.WriteLine("Available ports: 8086, 8087, 8088, 8089");
            Task t1 = loginTask(ipAddr);
            Task t2 = controlTask(ipAddr);
            Task t3 = voiceTask();
            Task t4 = contactListTask(ipAddr);
            Task t5 = controlShellTask();
            t1.Wait();
            t2.Wait();
            t3.Wait();
            t4.Wait();
            t5.Wait();
        }
    }
}
