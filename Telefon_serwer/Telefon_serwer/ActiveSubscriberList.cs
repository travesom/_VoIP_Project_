﻿using System;
using System.Collections.Concurrent;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Protocols;
using System.Collections.Generic;

namespace Telefon_serwer
{
    /// <summary>
    /// Info about active user
    /// </summary>
    class SubscriberItem
    {

        private IPEndPoint address;
        private nvcpOperStatus status;
        private int token;
        private DateTime time;

        public IPEndPoint Address { get { return address; } set { address = value; } }
        public nvcpOperStatus Status { get { return status; } set { status = value; } }
        public int Token { get { return token; } set { token = value; } }
        public DateTime Time { get { return time; } set { time = value; } }

        public SubscriberItem() { }

        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="addr">user IP address, e.g. 192.168.1.13</param>
        /// <param name="status">nvcp operation status ready/busy</param>
        /// <param name="time">Current time</param>
        public SubscriberItem(IPEndPoint addr, nvcpOperStatus status, DateTime time)
        {
            this.address = addr;
            this.status = status;
            this.token = 0;
            this.time = time;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="addr">user IP address, e.g. 192.168.1.13</param>
        /// <param name="status">nvcp operation status ready/busy</param>
        /// <param name="time">Current time</param>
        /// <param name="token">initial value of token, used for autorization</param>
        public SubscriberItem(IPEndPoint addr, nvcpOperStatus status, DateTime time, int token)
        {
            this.address = addr;
            this.status = status;
            this.token = token;
            this.time = time;
        }
    }

    /// <summary>
    /// Class contains subscriber list
    /// </summary>
    class ActiveSubscriberList
    {
        //sublist - key is user login
        public ConcurrentDictionary<string, SubscriberItem> subList;

        public ActiveSubscriberList()
        {
            subList = new ConcurrentDictionary<string, SubscriberItem>();
        }

        /// <summary>
        /// Check if dictionary contain element with key
        /// </summary>
        /// <param name="login">key</param>
        /// <returns>true if contains</returns>
        public bool exist(string login)
        {
            return subList.ContainsKey(login);
        }

        /// <summary>
        /// Add new subscriber to list 
        /// </summary>
        /// <param name="login">identifier number</param>
        /// <param name="data">SubscriberItem object</param>
        public int add(string login, SubscriberItem data)
        {
            if (subList.TryAdd(login, data))
            {
                return 0;
            }
            return 1;
           
        }

        /// <summary>
        /// Delete element from dictionary with key
        /// </summary>
        /// <param name="login">key</param>
        public int remove(string login)
        {
            SubscriberItem ac;
            if(subList.TryRemove(login, out ac))
            {    
                return 0;
            }
            return 1;
        }


        /// <summary>
        /// Removes all subscribers
        /// </summary>
        /// <returns>0 if ok, 1 if error</returns>
        public int clear()
        {
            try
            {
                subList.Clear();
                return 0;
            }
            catch(Exception)
            {
                return 1;
            }
        }

        public SubscriberItem this[string login]
        {
            get
            {
                return subList[login];
            }
        }

        public IEnumerator<KeyValuePair<string, SubscriberItem>> GetEnumerator()
        {
            return subList.GetEnumerator();
        }
    }
}
