using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Timers;
using System.Text.RegularExpressions;

namespace Protocols
{

    /// <summary>
    /// Communication Protocol:
    ///  _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ 
    /// | ver#1 |to#login |voice#binary data| text#string |
    /// |_ _ _ _|_ _ _ _ _|_ _ _ _ _ _ _ _ _|_ _ _ _ _ _ _|
    /// 
    /// > Voice = voice data (binary)
    /// > Text = optional text chat data
    /// 
    /// </summary>

    /// <summary>
    /// Network Voice Protocol. 
    /// <para></para>
    /// Use for communication client-client (via server).
    /// </summary>
    public class NVP : IProtocol
    {
        private short version;
        private string loginTo;   
        private string voiceData;
        private string textData;

        public short Version { get { return version; } set { version = value; } }
        public string Voice { get { return voiceData; } set { voiceData = value; } }
        public string Text { get { return textData; } set { textData = value; } }
        public string To { get { return loginTo; } set { loginTo = value; } }

        /// <summary>
        /// Constructor that convert string into NVP object
        /// </summary>
        /// <param name="data"></param>
        public NVP(string data)
        {
            encodeMsg(data);
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public NVP()
        {
            this.version = 0x01;  
            this.voiceData = "";
            this.textData = "";
            this.loginTo = "";
        }
 

        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="Vdata">voice data</param>
        /// <param name="Tdata">text data</param>
        /// <param name="user">destination user id</param>
        public NVP(string user, string Vdata, string Tdata)
        {
            this.version = 0x01;
            this.voiceData = Vdata;
            this.textData = Tdata;
            this.loginTo = user;
        }


        /// <summary>
        /// Converts string into NVP object
        /// </summary>
        /// <param name="msg">input string</param>
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
                    case "to": this.loginTo = grCol[2].ToString();break;
                    case "voice": this.voiceData = grCol[2].ToString(); break;
                    case "text": this.textData = grCol[2].ToString(); break;    
                }
            }
        }

        public override string ToString()
        {
            string s = "";
            s = s + "ver#'" + this.Version + "'";
            s = s + " to#'" + this.To + "'";
            s = s + " voice#'" + this.voiceData + "'";
            s = s + " text#'" + this.textData + "'";
            return s;
        }

    }
}

