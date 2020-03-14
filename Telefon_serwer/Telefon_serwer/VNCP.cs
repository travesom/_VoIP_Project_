using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Telefon_serwer
{
    /// <summary>
    /// Voice Network Control Protocol. 
    /// <para></para>
    /// Use for communication client-server.
    /// </summary>

    /// <remarks>
    /// Communication Protocol:
    ///                8               16              24              32
    ///  _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ 
    /// |                                                               |
    /// |_ _ _ _ _ _ _ _|_ _ _ _ _ _ _ _|_ _ _ _ _ _ _ _|_ _ _ _ _ _ _ _|
    /// |                    Destination IP Address                     |
    /// |_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _|
    /// |                      Source IP Address                        |
    /// |_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _|
    /// |                       (optional) DATA                         |
    /// |                                                               |
    /// |_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _|
    /// 
    /// </remarks>
    class VNCP
    {
        private IPAddress source;
        private IPAddress destination;
        private string Data;
    }
}
