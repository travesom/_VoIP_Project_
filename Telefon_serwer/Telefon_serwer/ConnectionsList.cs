using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telefon_serwer
{
    class ConnectionPair
    {
        public string first { get; set; }
        public string second { get; set; }

        public ConnectionPair() { }
        public ConnectionPair(string first, string second)
        {
            this.first = first;
            this.second = second;
        }
    };


    /// <summary>
    /// Provide list of all active connections between users
    /// </summary>
    class ConnectionsList
    {
        private List<ConnectionPair> list;

        public bool exist(string s, string s2)
        {
           return list.Exists(x => x.first == s && x.second == s2);
        }

        public bool exist(ConnectionPair cp)
        {
            return list.Exists(x => x == cp);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns>0 if Ok, 1 if error</returns>
        public int add(string first, string second)
        {
            if (!exist(first, second))
            {
                list.Add(new ConnectionPair(first, second));
                return 0;
            }
            return 1;
        }

        public int add(ConnectionPair cp)
        {
            if (!exist(cp))
            {
                list.Add(cp);
                return 0;
            }
            return 1;
        }

        public int remove(string first, string second)
        {
            if (exist(first, second))
            {
                list.Remove(new ConnectionPair(first, second));
                return 0;
            }
            return 1;
        }

        public int remove(ConnectionPair cp)
        {
            if (exist(cp))
            {
                list.Remove(cp);
                return 0;
            }
            return 1;
        }
    }
}
