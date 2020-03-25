using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Telefon_serwer
{
    /// <summary>
    /// 
    /// </summary>
    class ContactItem
    {
        private string nick;
        private bool pinned;

        public string Nick { get { return nick; } set { nick = value; } }
        public bool Pinned { get { return pinned; } set { pinned = value; } }

        public ContactItem() { nick = ""; }
        public ContactItem(string nick)
        {
            this.nick = nick;
            this.pinned = false;
        }
        public ContactItem(string nick, bool pin)
        {
            this.nick = nick;
            this.pinned = pin;
        }

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
    /// Class that contains contact list for all users, each user have a list of his contacts
    /// </summary>
    class UserContactList
    {
        //key is user login
        private string ownerLogin;
        private Dictionary<string, ContactItem> contactList;
        
        public UserContactList(string ownerLogin)
        {
            this.ownerLogin = ownerLogin;
            contactList = new Dictionary<string, ContactItem>();
        }

        public Dictionary<string, ContactItem> ContactList
        {
            get { return contactList; }
        }

        public int add(string login, ContactItem item)
        {
            if (!contactList.ContainsKey(login))
            {
                contactList.Add(login, item);
                return 0;
            }
            else return 1;
        }

        public int add(KeyValuePair<string,ContactItem> item)
        {
            if (!contactList.ContainsKey(item.Key))
            {
                contactList.Add(item.Key,item.Value);
                return 0;
            }
            else return 1;
        }

        public int remove(string login)
        {
            if (!contactList.ContainsKey(login))
            {
                contactList.Remove(login);
                return 0;
            }
            else return 1;
        }

        public ContactItem this[string key]
        {
            get { return contactList[key]; }
        }


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

        public void readFromXML()
        {
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
                    add(dict_item);
                }
            }
        }
    }
}
