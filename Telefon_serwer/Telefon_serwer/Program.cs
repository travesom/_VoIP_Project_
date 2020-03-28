using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using System.Security.Cryptography;
using System.Net.Sockets;
using Protocols;

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

        static async Task loginTask()
        {
            TcpListener server = new TcpListener(IPAddress.Parse("192.168.1.32"), 8086);
            server.Start();
            while (true)
            {
                TcpClient client = await server.AcceptTcpClientAsync();
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
                                    if(uaList.login(login, passwd))
                                    {
                                        if (asList.add(login, new SubscriberItem(((IPEndPoint)client.Client.RemoteEndPoint), nvcpOperStatus.READY, DateTime.Now)) == 0)
                                        {
                                            sendFrame.OperationStatus = ulpOperStatus.SUCCESS;
                                            Console.WriteLine(DateTime.Now + ": Account: " + login + " logged in");
                                        }
                                        else
                                        {
                                            sendFrame.OperationStatus = ulpOperStatus.WRONG_LOGIN;
                                            Console.WriteLine(DateTime.Now + ": Account: " + login + ": already logged in ");
                                        }
                                    }
                                    else
                                    {
                                        sendFrame.OperationStatus = ulpOperStatus.WRONG_PASS;
                                        Console.WriteLine(DateTime.Now + ": Account: error: to log in " + login);
                                    }
                                    
                                }
                                break;

                            case ulpOperation.REGISTER:
                                {
                                    if (uaList.createAccount(data.Split(' ')[0], data.Split(' ')[1]) == 0)
                                        {
                                            sendFrame.OperationStatus = ulpOperStatus.SUCCESS;
                                            Console.WriteLine(DateTime.Now + ": Account: " + "succesfully registered: " + data.Split(' ')[0]);
                                        }
                                    else
                                        {
                                            sendFrame.OperationStatus = ulpOperStatus.WRONG_LOGIN;
                                            Console.WriteLine(DateTime.Now + ": Account: " + "error: account already exist");
                                    }
                                }
                                break;

                            case ulpOperation.CHANGE_DATA:
                                {
                                    string[] tab = data.Split(' ');
                                    if (tab.Length == 3)
                                    {
                                        if (uaList.changePassword(tab[0], tab[1], tab[2]) == 0)
                                        {
                                            sendFrame.OperationStatus = ulpOperStatus.SUCCESS;
                                            Console.WriteLine(DateTime.Now + ": Account: " + "succesfully changed password");
                                        }
                                        else if (uaList.changePassword(tab[0], tab[1], tab[2]) == 1)
                                        {
                                            sendFrame.OperationStatus = ulpOperStatus.WRONG_LOGIN;
                                            Console.WriteLine(DateTime.Now + ": Account: " + "error: cannot changed password");
                                        }
                                        else
                                        {
                                            sendFrame.OperationStatus = ulpOperStatus.WRONG_PASS;
                                            Console.WriteLine(DateTime.Now + ": Account: " + "error: cannot changed password");
                                        }
                                    }
                                    else
                                    {
                                        if (tab.Length == 2)
                                        {
                                            if (uaList.changeNickName(tab[0], tab[1]) == 0)
                                            {
                                                sendFrame.OperationStatus = ulpOperStatus.SUCCESS;
                                                Console.WriteLine(DateTime.Now + ": Account: " + "succesfully changed nick");
                                            }
                                            else
                                            {
                                                sendFrame.OperationStatus = ulpOperStatus.WRONG_LOGIN;
                                                Console.WriteLine(DateTime.Now + ": Account: " + "error: cannot changed nick");
                                            }
                                        }
                                        else
                                        {
                                            sendFrame.ProtocolStatus = IStatus.DATA_FAIL;
                                        }
                                    }                            
                                }
                                break;
                            default:
                                {
                                    sendFrame.ProtocolStatus = IStatus.OPER_FAIL;
                                    Console.WriteLine(DateTime.Now + ": Protocol: " + "error: wrong dataframe");
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

        static async Task controlTask()
        {
            TcpListener server = new TcpListener(IPAddress.Parse("192.168.1.32"), 8088);
            server.Start();
            while (true)
            {
                TcpClient client = await server.AcceptTcpClientAsync();
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
                                            Console.WriteLine(DateTime.Now + ": Connection: " + "error: try to check avability without being logged in");
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

                                    if (data.Split(' ').Length == 2)
                                    {
                                        var my_login = data.Split(' ')[0];
                                        var other_login = data.Split(' ')[1];

                                        NVCP sendFrame2 = new NVCP(frame.OperationType, "");
                                        if (asList.exist(my_login))
                                        {
                                            if (asList.exist(other_login))
                                            {
                                                if (DateTime.Now.Subtract(asList[other_login].Time).CompareTo(TimeSpan.FromSeconds(5.0)) < 1)
                                                {
                                                    sendFrame.OperationStatus = nvcpOperStatus.WAITING_CONNECTION;
                                                    sendFrame2.OperationType = nvcpOperation.CONNECT;
                                                    sendFrame2.OperationStatus = nvcpOperStatus.WAITING_CONNECTION;
                                                    sendFrame.TimeStamp = DateTime.Now;
                                                    sendFrame2.TimeStamp = DateTime.Now;
                                                    sendFrame2.ProtocolStatus = IStatus.OK;

                                                    TcpClient second = new TcpClient(asList[other_login].Address);
                                                    var sendBytes2 = Encoding.ASCII.GetBytes(sendFrame2.ToString());
                                                    await second.GetStream().WriteAsync(sendBytes2, 0, sendBytes2.Length);
                                                }
                                                else
                                                {
                                                    sendFrame.OperationStatus = nvcpOperStatus.UNAVAILABLE;
                                                    sendFrame.TimeStamp = DateTime.Now;
                                                    asList.remove(other_login);
                                                }
                                            }
                                            else
                                            {
                                                sendFrame.OperationStatus = nvcpOperStatus.UNAVAILABLE;
                                                sendFrame.TimeStamp = DateTime.Now;
                                                Console.WriteLine(DateTime.Now + ": Connection: " + "error: try to connect user doesn't exists");
                                            }
                                        }
                                        else
                                        {
                                            sendFrame.OperationStatus = nvcpOperStatus.NONE;
                                            sendFrame.TimeStamp = DateTime.Now;
                                            Console.WriteLine(DateTime.Now + ": Connection: " + "error: try to connect user doesn't logged in");
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
                                    if (data.Split(' ').Length == 1)
                                    {
                                        if (asList.exist(data))
                                        {
                                            asList[data].Status = frame.OperationStatus;
                                            asList[data].Address = (IPEndPoint)client.Client.RemoteEndPoint;
                                            sendFrame.OperationStatus = frame.OperationStatus;
                                            sendFrame.TimeStamp = DateTime.Now;
                                            sendFrame.TimeStamp = DateTime.Now;
                                        }
                                        else
                                        {
                                            sendFrame.OperationStatus = nvcpOperStatus.NONE;
                                            sendFrame.OperationType = nvcpOperation.MY_STATUS;
                                            sendFrame.ProtocolStatus = IStatus.OTHER_FAIL;
                                            sendFrame.TimeStamp = DateTime.Now;
                                            Console.WriteLine(DateTime.Now + ": User Status: " + "error: user send status without being logged in");
                                        }
                                    }
                                    else
                                    {
                                        sendFrame.ProtocolStatus = IStatus.DATA_FAIL;
                                        sendFrame.TimeStamp = DateTime.Now;
                                        Console.WriteLine(DateTime.Now + ": User Status: " + "error: wrong dataframe");
                                    }
                                }
                                break;
                            default:
                                {
                                    sendFrame.ProtocolStatus = IStatus.OPER_FAIL;
                                    sendFrame.TimeStamp = DateTime.Now;
                                    Console.WriteLine(DateTime.Now + ": Protocol: " + "error: wrong dataframe");
                                }
                                break;
                        }

                        byte[] sendBytes = Encoding.ASCII.GetBytes(sendFrame.ToString());
                        await client.GetStream().WriteAsync(sendBytes, 0, sendBytes.Length);                      
                    });
            }
        }

        static void Main(string[] args)
        {     
            uaList.createAccount("Marcin", "0x123456");

            //uaList.writeToXML();
            //UserAccountList uaList2 = UserAccountList.createFromXML();
            //.WriteLine(uaList2.exist("Marcin").ToString());
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
            Console.WriteLine("Server is running on IP address: 192.168.1.13");
            Console.WriteLine("Available ports: 8086, 8087, 8088, 8089");
            Task t1 = loginTask();
            Task t2 = controlTask();
            Task t3 = voiceTask();
            t1.Wait();
            t2.Wait();
            t3.Wait();

        }
    }
}
