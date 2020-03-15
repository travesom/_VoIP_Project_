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
    /// > Source IP Address
    /// > Destination IP Address
    /// > Data = optional Data, ex. IP address of the call receiver
    /// 
    /// </summary>

    ///<summary>
    /// Status numbers
    /// </summary>
    /// 

    public enum Operation
    {
        CONNECT = 0,
        AVABILITY = 1,
        MY_STATUS = 2
    }

    public enum OperStatus
    {
        //OK
        READY = 0x20,
        WAITING_CONNECTION = 0x21,
        //NOT AVAILABLE
        BUSY = 0x22,
        UNAVAILABLE = 0x40,
        NONE = 0xFF
    };

    public enum NVCPStatus
    {
        OK = 200,
        OPER_FAIL = 301, //fail operation name
        OPER_STAT_FAIL = 302, //fail operation status name
        KEY_FAIL = 303, //fail key: time# -> tme#
        STATUS_FAIL = 304,
        OTHER_FAIL = 307 //other
    };



    /// <summary>
    /// Network Voice Control Protocol. 
    /// <para></para>
    /// Use for communication client-server.
    /// </summary>
    public class NVCP
    {
        private short version;
        private Operation operationType;
        private OperStatus operationStatus;
        private NVCPStatus protocolStatus;
        private DateTime timeStamp;
        public string data;

        public short Version { get { return version; } set { version = value; } }
        public Operation OperationType { get { return operationType; } set { operationType = value; } }
        public OperStatus OperationStatus { get { return operationStatus; } set { operationStatus = value; } }
        public NVCPStatus ProtocolStatus { get { return protocolStatus; } set { protocolStatus = value; } }
        public DateTime TimeStamp { get { return timeStamp; } set { timeStamp = value; } }
        

        public NVCP(string data)
        {
            encodeMsg(data);
        }

        public NVCP(Operation type, OperStatus state, string data = "")
        {
            this.version = 0x01;
            this.operationStatus = state;
            this.operationType = type;
            this.protocolStatus = NVCPStatus.OK;
            this.timeStamp = DateTime.Now;
            this.data = data;
        }

        public NVCP(Operation type, string data = "")
        {
            this.version = 0x01;
            this.operationStatus = OperStatus.NONE;
            this.operationType = type;
            this.protocolStatus = NVCPStatus.OK;
            this.timeStamp = DateTime.Now;
            this.data = data;
        }

        public void encodeMsg(string msg)
        {
            Regex regrCol = new Regex(@"([a-z]+)#'([0-9A-Za-z_\-\.\:\s]*)'\s*");
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
                                case "CONNECT": this.OperationType = Operation.CONNECT; break;
                                case "AVABILITY": this.OperationType = Operation.AVABILITY; break;
                                case "MY_STATUS": this.OperationType = Operation.MY_STATUS; break;
                                default:
                                    {
                                        this.ProtocolStatus = NVCPStatus.OPER_FAIL; break;
                                    }
                            }
                        }
                        break;
                    case "status":
                        {
                            switch (grCol[2].ToString())
                            {
                                case "READY": this.OperationStatus = OperStatus.READY; break;
                                case "BUSY": this.OperationStatus = OperStatus.BUSY; break;
                                case "WAITING_CONNECTION": this.OperationStatus = OperStatus.WAITING_CONNECTION; break;
                                case "UNAVAILABLE": this.OperationStatus = OperStatus.UNAVAILABLE; break;
                                case "NONE": this.OperationStatus = OperStatus.NONE; break;
                                default:
                                    {
                                        this.ProtocolStatus = NVCPStatus.OPER_STAT_FAIL;
                                        this.OperationStatus = OperStatus.NONE;
                                        break;
                                    }
                            }
                        }
                        break;
                    case "nvcp":
                        {
                            switch (grCol[2].ToString())
                            {
                                case "OK": this.ProtocolStatus = NVCPStatus.OK; break;
                                case "KEY_FAIL": this.ProtocolStatus = NVCPStatus.KEY_FAIL; break;
                                case "OPER_FAIL": this.ProtocolStatus = NVCPStatus.OPER_FAIL; break;
                                case "OPER_STAT_FAIL": this.ProtocolStatus = NVCPStatus.OPER_STAT_FAIL; break;
                                case "STATUS_FAIL": this.ProtocolStatus = NVCPStatus.STATUS_FAIL; break;
                                case "OTHER_FAIL": this.ProtocolStatus = NVCPStatus.OTHER_FAIL; break;
                                default:
                                    {
                                        this.ProtocolStatus = NVCPStatus.STATUS_FAIL;
                                        break;
                                    }
                            }
                        }
                        break;
                    case "time": this.timeStamp = DateTime.Parse(grCol[2].ToString()); break;
                    case "data": this.data = grCol[2].ToString(); break;
                    default: this.ProtocolStatus = NVCPStatus.KEY_FAIL; break;
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
