using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;
using System.Security.Cryptography;


namespace Telefon_serwer
{
    class SubscriberInfo
    {
        private IPAddress address;
        private nvcpOperStatus status;
        private DateTime time;

        public IPAddress Address { get { return address; } set { address = value; } }
        public nvcpOperStatus Status { get { return status; } set { status = value; } }
        public DateTime Time { get { return time; } set { time = value; } }

        public SubscriberInfo() { }

        public SubscriberInfo(IPAddress addr, nvcpOperStatus status, DateTime time)
        {
            this.address = addr;
            this.status = status;
            this.time = time;
        }
    }

    class ActiveSubscriberList
    {
        //sublist - key is user login
        public Dictionary<string, SubscriberInfo> subList;

        public ActiveSubscriberList()
        {
            subList = new Dictionary<string, SubscriberInfo>();
        }

        public bool contain(string login)
        {
            return subList.ContainsKey(login);
        }

        public void add(string login, SubscriberInfo data)
        {
            try
            {
                subList.Add(login, data);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public void remove(string login)
        {
            try
            {
                subList.Remove(login);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public SubscriberInfo this[string login]
        {
            get
            {
                return subList[login];
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {

            //NVCP v1 = new NVCP(Operation.AVABILITY,"0x123abc456");

            /*try
            {
                string msg = v1.ToString();
                NVCP v2 = new NVCP(msg);
                Console.WriteLine(msg);
                Console.WriteLine(v2.ToString());
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            */
            ActiveSubscriberList aslist = new ActiveSubscriberList();
            aslist.add("Marcin", new SubscriberInfo(IPAddress.Parse("192.168.1.12"), nvcpOperStatus.READY, DateTime.Now));
            aslist.add("Lukasz", new SubscriberInfo(IPAddress.Parse("192.168.1.12"), nvcpOperStatus.BUSY, DateTime.Now));
            aslist.add("Szymon", new SubscriberInfo(IPAddress.Parse("192.168.1.12"), nvcpOperStatus.READY, DateTime.Now));
            aslist.add("Juliusz", new SubscriberInfo(IPAddress.Parse("192.168.1.12"), nvcpOperStatus.READY, DateTime.Now));

            UserAccountList UAlist = new UserAccountList();
            UAlist.createAccount("Marcin", new SHA1CryptoServiceProvider().ComputeHash(Encoding.ASCII.GetBytes("0x123456")).ToString());
            UAlist.createAccount("Lukasz", new SHA1CryptoServiceProvider().ComputeHash(Encoding.ASCII.GetBytes("0xabcdef")).ToString());
            UAlist.createAccount("Juliusz", new SHA1CryptoServiceProvider().ComputeHash(Encoding.ASCII.GetBytes("0x123abc")).ToString());
            UAlist.changeNickName("Lukasz", "Lucas");
            UAlist.changeNickName("Juliusz", "Julek");

            UserContactList UClist = new UserContactList("Marcin");
            UClist.add("Lukasz", new ContactItem(nick: UAlist["Lukasz"]));
            UClist.add("Juliusz", new ContactItem(nick: UAlist["Juliusz"], pin: true));
            UClist.writeToXML();

            UserContactList UClist2 = new UserContactList("Marcin");
            UClist2.readFromXML();
            foreach (var elem in UClist2.ContactList)
            {
                Console.WriteLine("{0} {1}", elem.Key, elem.Value.ToString());
            }
        }
    }
}
