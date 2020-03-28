using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Telefon_serwer
{
    /// <summary>
    /// Class provided info about one contact in users contact list
    /// </summary>
    class ContactItem
    {
        private string nick;
        private bool pinned;

        public string Nick { get { return nick; } set { nick = value; } }
        public bool Pinned { get { return pinned; } set { pinned = value; } }

        public ContactItem() { nick = ""; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="nick">contact nick name</param>
        public ContactItem(string nick)
        {
            this.nick = nick;
            this.pinned = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="nick">contact nick name</param>
        /// <param name="pin">is pinned to the top of the list</param>
        public ContactItem(string nick, bool pin)
        {
            this.nick = nick;
            this.pinned = pin;
        }

        /// <summary>
        /// Change contact nick name
        /// </summary>
        /// <param name="other">new nick name</param>
        /// <returns></returns>
        public int changeNick(string other)
        {
            if (other == "") return 1;
            Nick = other;
            return 0;
        }

        public void pin() { Pinned = true; }
        public void unpin() { Pinned = false; }

        public override string ToString()
        {
            return this.Nick + ' ' + (this.Pinned ? "Przypiety": "Nieprzypięty");
        }
    }


    /// <summary>
    /// Class that contains contact list for one user
    /// </summary>
    class UserContactList
    {
        //key is user login
        private string ownerLogin;
        private ConcurrentDictionary<string, ContactItem> contactList;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ownerLogin">login of contact list owner</param>
        public UserContactList(string ownerLogin)
        {
            this.ownerLogin = ownerLogin;
            contactList = new ConcurrentDictionary<string, ContactItem>();
        }

        public ConcurrentDictionary<string, ContactItem> ContactList
        {
            get { return contactList; }
        }

        /// <summary>
        /// Add new contact item to the list
        /// </summary>
        /// <param name="login">login - key</param>
        /// <param name="item">contact item</param>
        /// <returns></returns>
        public int add(string login, ContactItem item)
        {
            if (contactList.TryAdd(login, item))
            {   
                return 0;
            }
            return 1;
        }

        /// <summary>
        /// Add new contact item to the list
        /// </summary>
        /// <param name="item">pair of (login,item)</param>
        /// <returns></returns>
        public int add(KeyValuePair<string,ContactItem> item)
        {
            if (contactList.TryAdd(item.Key, item.Value))
            { 
                return 0;
            }
            return 1;
        }

        /// <summary>
        /// Delete contact from list
        /// </summary>
        /// <param name="login">login to be removed</param>
        /// <returns></returns>
        public int remove(string login)
        {
            ContactItem ci;
            if (contactList.TryRemove(login, out ci))
            { 
                return 0;
            }
            return 1;
        }

        public ContactItem this[string key]
        {
            get { return contactList[key]; }
        }

        /// <summary>
        /// Converts object to XML file and saves locally
        /// </summary>
        public void writeToXML()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            XmlWriter document = XmlWriter.Create(ownerLogin + "_contact.xml",settings);
            document.WriteStartDocument();
            document.WriteStartElement("contacts");

            document.WriteStartElement("owner");
            document.WriteAttributeString("id", ownerLogin);
            document.WriteEndElement();

            foreach( var elem in contactList)
            {
                document.WriteStartElement("contact");
                document.WriteAttributeString("id", elem.Key);

                document.WriteStartElement("nick");
                document.WriteString(elem.Value.Nick);
                document.WriteEndElement();

                document.WriteStartElement("pinned");
                document.WriteString((elem.Value.Pinned).ToString());
                document.WriteEndElement();
                document.WriteEndElement();
            }
            document.WriteEndDocument();
            document.Close();
        }

        
        /// <summary>
        /// Read from local XML file and return new object.
        /// </summary>
        /// <param name="ownerLogin">list onwer login</param>
        /// <returns>Returns new UserContactList object.</returns>
        public static UserContactList readFromXML(string ownerLogin)
        {
            UserContactList ucl = new UserContactList(ownerLogin);

            XmlDocument document = new XmlDocument();
            document.Load(ownerLogin + "_contact.xml");
            XmlElement root = document.DocumentElement;

            foreach(XmlNode node in root.ChildNodes)
            {
                if(node.NodeType == XmlNodeType.Element && node.Name != "owner")
                {
                    ContactItem ci = new ContactItem();
                    foreach (XmlNode contact in node.ChildNodes)
                    { 
                        if (contact.Name == "nick") ci.Nick = contact.FirstChild.Value;
                        if (contact.Name == "pinned") ci.Pinned = bool.Parse(contact.FirstChild.Value);
                    }
                    KeyValuePair<string, ContactItem> dict_item = new KeyValuePair<string, ContactItem>(node.Attributes["id"].Value, ci);
                    ucl.add(dict_item);
                }
            }

            return ucl;
        }
    }
}
