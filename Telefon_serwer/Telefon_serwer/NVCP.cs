using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Timers;
using System.Text.RegularExpressions;

namespace Telefon_serwer
{


    /// <summary>
    /// Communication Protocol:
    ///  _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _
    /// | ver#1 |oper#Avability |  status# OK   |  nvcp#OK  |time#2020.01.01:21.58.12 |data#string  |
    /// |_ _ _ _|_ _ _ _ _ _ _ _|_ _ _ _ _ _ _ _|_ _ _ _ _ _|_ _ _ _ _ _ _ _ _ _ _ _ _|_ _ _ _ _ _ _|
    /// 
    /// ver = protocol version, now is 1 
    /// > lenght = total lenght in bytes
    /// > type = type of communicate 
    ///     > 0x0 = connect to
    ///     > 0x1 = check avability
    ///     > 0x2 = my status
    /// > st = status type 0: my status, 1: OK, 2: Cannot establish connection
    /// > st + num (stateNumber)
    ///     ---------------
    ///     > 0x20 = Ready
    ///     > 0x21 = Waiting for connection
    ///     > 0x22 = Busy
    ///     ---------------
    ///     > 0x40 = OK, receiver is avaiable
    ///     > 0x41 = OK, receiver will be waiting for you
    ///     ---------------
    ///     > 0x80 = receiver is busy
    ///     > 0x81 = receiver is waiting for other call
    ///     > 0x82 = receiver is unavailable
    ///     ---------------
    /// > Timestamp
    /// > Data = optional Data, ex. IP address of the call receiver
    /// 
    /// </summary>

    ///<summary>
    /// Status numbers
    /// </summary>
    /// 


    /// <summary>
    /// Network Voice Control Protocol. 
    /// <para></para>
    /// Use for communication client-server.
    /// </summary>
    public class NVCP : IProtocol
    {
        private short version;
        private nvcpOperation operationType;
        private nvcpOperStatus operationStatus;
        private IStatus protocolStatus;
        private DateTime timeStamp;
        public string data;

        public short Version { get { return version; } set { version = value; } }
        public nvcpOperation OperationType { get { return operationType; } set { operationType = value; } }
        public nvcpOperStatus OperationStatus { get { return operationStatus; } set { operationStatus = value; } }
        public IStatus ProtocolStatus { get { return protocolStatus; } set { protocolStatus = value; } }
        public DateTime TimeStamp { get { return timeStamp; } set { timeStamp = value; } }
        

        public NVCP(string data)
        {
            encodeMsg(data);
        }

        public NVCP(nvcpOperation type, nvcpOperStatus state, string data = "")
        {
            this.version = 0x01;
            this.operationStatus = state;
            this.operationType = type;
            this.protocolStatus = IStatus.OK;
            this.timeStamp = DateTime.Now;
            this.data = data;
        }

        public NVCP(nvcpOperation type, string data = "")
        {
            this.version = 0x01;
            this.operationStatus = nvcpOperStatus.NONE;
            this.operationType = type;
            this.protocolStatus = IStatus.OK;
            this.timeStamp = DateTime.Now;
            this.data = data;
        }

        public void encodeMsg(string msg)
        {
            Regex regrCol = new Regex(@"([a-z]+)#'([0-9A-Za-z_\-\.\:\s\{\}]*)'\s*");
            MatchCollection m1;
            m1 = regrCol.Matches(msg);
            foreach (Match e in m1)
            {
                GroupCollection grCol = e.Groups;
                Console.WriteLine("{0} {1}", grCol[1].ToString(), grCol[2]);
                switch (grCol[1].ToString())
                {
                    case "ver": this.version = short.Parse(grCol[2].ToString());break;
                    case "oper":
                        {
                            switch (grCol[2].ToString())
                            {
                                case "CONNECT": this.OperationType = nvcpOperation.CONNECT; break;
                                case "AVABILITY": this.OperationType = nvcpOperation.AVABILITY; break;
                                case "MY_STATUS": this.OperationType = nvcpOperation.MY_STATUS; break;
                                default:
                                    {
                                        this.ProtocolStatus = IStatus.OPER_FAIL; break;
                                    }
                            }
                        }
                        break;
                    case "status":
                        {
                            switch (grCol[2].ToString())
                            {
                                case "READY": this.OperationStatus = nvcpOperStatus.READY; break;
                                case "BUSY": this.OperationStatus = nvcpOperStatus.BUSY; break;
                                case "WAITING_CONNECTION": this.OperationStatus = nvcpOperStatus.WAITING_CONNECTION; break;
                                case "UNAVAILABLE": this.OperationStatus = nvcpOperStatus.UNAVAILABLE; break;
                                case "NONE": this.OperationStatus = nvcpOperStatus.NONE; break;
                                default:
                                    {
                                        this.ProtocolStatus = IStatus.OPER_STAT_FAIL;
                                        this.OperationStatus = nvcpOperStatus.NONE;
                                        break;
                                    }
                            }
                        }
                        break;
                    case "nvcp":
                        {
                            switch (grCol[2].ToString())
                            {
                                case "OK": this.ProtocolStatus = IStatus.OK; break;
                                case "KEY_FAIL": this.ProtocolStatus = IStatus.KEY_FAIL; break;
                                case "OPER_FAIL": this.ProtocolStatus = IStatus.OPER_FAIL; break;
                                case "OPER_STAT_FAIL": this.ProtocolStatus = IStatus.OPER_STAT_FAIL; break;
                                case "STATUS_FAIL": this.ProtocolStatus = IStatus.STATUS_FAIL; break;
                                case "OTHER_FAIL": this.ProtocolStatus = IStatus.OTHER_FAIL; break;
                                default:
                                    {
                                        this.ProtocolStatus = IStatus.STATUS_FAIL;
                                        break;
                                    }
                            }
                        }
                        break;
                    case "time": this.timeStamp = DateTime.Parse(grCol[2].ToString()); break;
                    case "data": this.data = grCol[2].ToString(); break;
                    default: this.ProtocolStatus = IStatus.KEY_FAIL; break;
                }
            }
        }
    
        public override string ToString()
        {
            string s = "";
            s = s + "ver#'" + this.Version + "'";
            s = s + " oper#'" + this.OperationType.ToString() + "'";
            s = s + " status#'" + this.OperationStatus.ToString() + "'";
            s = s + " nvcp#'" + this.ProtocolStatus.ToString() + "'";
            s = s + " time#'" + this.timeStamp.ToString() + "'";
            s = s + " data#'" + this.data + "'";
            return s;
        }     
    }
}
