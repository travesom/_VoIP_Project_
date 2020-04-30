using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocols
{
    public enum nvcpOperation
    {
        CONNECT = 0,
        AVABILITY = 1,
        MY_STATUS = 2,
    }

    public enum nvcpOperStatus
    {
        //OK
        READY = 0x20,
        WAITING_CONNECTION = 0x21,
        //NOT AVAILABLE
        BUSY = 0x30,
        UNAVAILABLE = 0x40,
        NONE = 0xFF
    };

    public enum ulpOperation
    {
        LOGIN = 0,
        REGISTER = 1,
        CHANGE_DATA = 2
    }

    public enum ulpOperStatus
    {
        //OK
        SUCCESS = 0x20,
        //NOT AVAILABLE
        WRONG_PASS = 0x30,
        WRONG_LOGIN = 0x31,

        NONE = 0xFF
    };

    public enum IStatus
    {
        OK = 200,
        OPER_FAIL = 301, //fail operation name
        OPER_STAT_FAIL = 302, //fail operation status name
        KEY_FAIL = 303, //fail key: time# -> tme#
        STATUS_FAIL = 304,
        DATA_FAIL = 305, // if data format/size is wrong, not enough or too much data
        OTHER_FAIL = 307 //other
    };


    interface IProtocol
    {
        void encodeMsg(string msg);
    }
}
