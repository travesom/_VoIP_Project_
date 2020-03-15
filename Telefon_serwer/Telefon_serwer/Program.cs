using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Threading;


namespace Telefon_serwer
{
    class SubscriberInfo
    {
        private IPAddress address;
        private OperStatus status;
        private DateTime time;

        public IPAddress Address { get { return address; } set { address = value; } }
        public OperStatus Status { get { return status; } set { status = value; } }
        public DateTime Time { get { return time; } set { time = value; } }

        public SubscriberInfo() { }

        public SubscriberInfo(IPAddress addr, OperStatus status, DateTime time)
        {
            this.address = addr;
            this.status = status;
            this.time = time;
        }
    }

    class SubscriberList
    {
        public Dictionary<string, SubscriberInfo> subList;

        public SubscriberList()
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

        public SubscriberInfo getInfo(string login)
        {
            SubscriberInfo si1 = new SubscriberInfo();
            try
            { 
                si1 = subList[login];
                return si1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return si1;
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
            SubscriberList slist = new SubscriberList();
            slist.add("Marcin", new SubscriberInfo(IPAddress.Parse("192.168.1.12"), OperStatus.READY, DateTime.Now));
            slist.add("Lukasz", new SubscriberInfo(IPAddress.Parse("192.168.1.12"), OperStatus.BUSY, DateTime.Now));
            slist.add("Szymon", new SubscriberInfo(IPAddress.Parse("192.168.1.12"), OperStatus.READY, DateTime.Now));
            slist.add("Juliusz", new SubscriberInfo(IPAddress.Parse("192.168.1.12"), OperStatus.READY, DateTime.Now));


        }
    }
}
