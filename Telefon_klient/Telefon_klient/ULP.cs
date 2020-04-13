using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Text.RegularExpressions;

namespace Protocols
{
    /// <summary>
    /// Communication Protocol:
    ///  _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ 
    /// | ver#1 |oper#LOGIN | status# SUCCESS |  ulp#OK   | data#string |
    /// |_ _ _ _|_ _ _ _ _ _|_ _ _ _ _ _ _ _ _|_ _ _ _ _ _|_ _ _ _ _ _ _|
    /// 
    /// ver = protocol version, now is 1 
    /// > oper = type of communicate 
    /// > status = communicate status
    /// > ulp = ulp protocol status
    /// > Data = optional Data, ex. IP address of the call receiver
    /// > Data usage:
    ///      LOGIN -> (login) (password); response -> (new_token)
    ///      REGISTER -> (login) (password) [(pseudonim)]
    ///      CHANGE_DATA -> (login) (old_pass) (newpass) (token)| (login) (new_nick) (token); response -> (nvcpOper: SUCCESS) (new_token)
    /// </summary>



    /// <summary>
    /// User Login Protocol
    /// <para></para>
    /// Use for login and register users
    /// </summary>
    public class ULP : IProtocol
    {
        private short version;
        private ulpOperation operationType;
        private ulpOperStatus operationStatus;
        private IStatus protocolStatus;
        public string data;

        public short Version { get { return version; } set { version = value; } }
        public ulpOperation OperationType { get { return operationType; } set { operationType = value; } }
        public ulpOperStatus OperationStatus { get { return operationStatus; } set { operationStatus = value; } }
        public IStatus ProtocolStatus { get { return protocolStatus; } set { protocolStatus = value; } }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="data">string to be converted into ULP object</param>
        public ULP(string data)
        {
            encodeMsg(data);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">ulp operation type: login/register</param>
        /// <param name="state">status of operation</param>
        /// <param name="data">additional data to be pass</param>
        public ULP(ulpOperation type, ulpOperStatus state, string data = "")
        {
            this.version = 0x01;
            this.operationStatus = state;
            this.operationType = type;
            this.protocolStatus = IStatus.OK;
            this.data = data;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">ulp operation type: login/register</param>
        /// <param name="data">additional data to be pass</param>
        public ULP(ulpOperation type, string data = "")
        {
            this.version = 0x01;
            this.operationStatus = ulpOperStatus.NONE;
            this.operationType = type;
            this.protocolStatus = IStatus.OK;
            this.data = data;
        }

        /// <summary>
        /// Converts string into ULP object
        /// </summary>
        /// <param name="msg">input message</param>
        public void encodeMsg(string msg)
        {
            Regex regrCol = new Regex(@"([a-z]+)#'([0-9A-Za-z_\-\.\:\s\{\}]*)'\s*");
            MatchCollection m1;
            m1 = regrCol.Matches(msg);
            foreach (Match e in m1)
            {
                GroupCollection grCol = e.Groups;
                //Console.WriteLine("{0} {1}", grCol[1].ToString(), grCol[2]);
                switch (grCol[1].ToString())
                {
                    case "ver": this.version = short.Parse(grCol[2].ToString()); break;
                    case "oper":
                        {
                            switch (grCol[2].ToString())
                            {
                                case "LOGIN": this.OperationType = ulpOperation.LOGIN; break;
                                case "REGISTER": this.OperationType = ulpOperation.REGISTER; break;
                                case "CHANGE_DATA": this.OperationType = ulpOperation.CHANGE_DATA; break;
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
                                case "SUCCESS": this.OperationStatus = ulpOperStatus.SUCCESS; break;
                                case "WRONG_LOGIN": this.OperationStatus = ulpOperStatus.WRONG_LOGIN; break;
                                case "WRONG_PASS": this.OperationStatus = ulpOperStatus.WRONG_PASS; break;
                                case "NONE": this.OperationStatus = ulpOperStatus.NONE; break;
                                default:
                                    {
                                        this.ProtocolStatus = IStatus.OPER_STAT_FAIL;
                                        this.OperationStatus = ulpOperStatus.NONE;
                                        break;
                                    }
                            }
                        }
                        break;
                    case "ulp":
                        {
                            switch (grCol[2].ToString())
                            {
                                case "OK": this.ProtocolStatus = IStatus.OK; break;
                                case "KEY_FAIL": this.ProtocolStatus = IStatus.KEY_FAIL; break;
                                case "OPER_FAIL": this.ProtocolStatus = IStatus.OPER_FAIL; break;
                                case "OPER_STAT_FAIL": this.ProtocolStatus = IStatus.OPER_STAT_FAIL; break;
                                case "STATUS_FAIL": this.ProtocolStatus = IStatus.STATUS_FAIL; break;
                                case "DATA_FAIL": this.ProtocolStatus = IStatus.DATA_FAIL;break;
                                case "OTHER_FAIL": this.ProtocolStatus = IStatus.OTHER_FAIL; break;
                                default:
                                    {
                                        this.ProtocolStatus = IStatus.STATUS_FAIL;
                                        break;
                                    }
                            }
                        }
                        break;
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
            s = s + " ulp#'" + this.ProtocolStatus.ToString() + "'";
            s = s + " data#'" + this.data + "'";
            return s;
        }
    }
}
