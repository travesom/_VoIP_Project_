using System;
using System.Collections.Concurrent;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Collections.Generic;

namespace Telefon_serwer
{
    /// <summary>
    /// Class to store info about user that allow to login from any device.
    /// 
    /// </summary>
    class UserAccount
    {
        
        private string passwordSHA; // password hashed with sha256
        private string nickName; // name use by account owner

        public string PasswordSHA { get { return passwordSHA; } set { passwordSHA = value; } }
        public string NickName { get { return nickName; } set { nickName = value; } }

        public UserAccount() { }

        public UserAccount(string password)
        {
            this.passwordSHA = password;
            this.nickName = "null";
        }

        public UserAccount(string password, string name)
        {
            this.passwordSHA = password;
            this.nickName = name;
        }

        public override string ToString()
        {
            return NickName + ' ' + PasswordSHA;
        }

    }

    /// <summary>
    /// Class use to save all user accounts on server.
    /// Have functions to connect to XML files to get/save data
    /// </summary>
    class UserAccountList
    {
        //key = user login (UNIQUE value to be use everywhere to identify user)
        private ConcurrentDictionary<string, UserAccount> usersList;  

        /// <summary>
        /// Default constructor
        /// </summary>
        public UserAccountList()
        {
            usersList = new ConcurrentDictionary<string, UserAccount>();
        }


        /// <summary>
        /// Check if parameter password is correct
        /// </summary>
        /// <param name="login"></param>
        /// <param name="password"></param>
        /// <returns>true if parameter is same as hash store in list</returns>
        public int login(string login, string password)
        {
            if(exist(login))
            {
                if (password != usersList[login].PasswordSHA) return 1;
                return 0;
            }
            return 2;
            
        }

        /// <summary>
        /// Check if exist element with key
        /// </summary>
        /// <param name="login">key to be checked</param>
        /// <returns>true if exists, else false</returns>
        public bool exist(string login)
        {
            return usersList.ContainsKey(login);
        }

        /// <summary>
        /// Add new account
        /// </summary>
        /// <param name="login">identifier number</param>
        /// <param name="passwd">account password to log in</param>
        /// <returns>0 if success, 1 if exists</returns>
        public int createAccount(string login, string passwd)
        {
            UserAccount u1 = new UserAccount(passwd);
            if (usersList.TryAdd(login, u1)) return 0;  
            return 1;
        }

        /// <summary>
        /// Add new account
        /// </summary>
        /// <param name="login">identifier number</param>
        /// <param name="passwd">account password to log in</param>
        /// <param name="pseudonim">user nick name</param>
        /// <returns>0 if success, 1 if already exist</returns>
        public int createAccount(string login, string passwd, string pseudonim)
        {
            UserAccount u1 = new UserAccount(passwd,pseudonim);
            if (usersList.TryAdd(login, u1)) return 0;
            return 1;
        }
         
        /// <summary>
        /// Delete account, remove from dictionary
        /// </summary>
        /// <param name="login">identifier name</param>
        /// <returns>0 if success, 1 if don't exist</returns>         
        public int removeAccount(string login)
        {
            UserAccount ua;
            if (usersList.TryRemove(login, out ua)) return 0;   
            return 1;
        }

        /// <summary>
        /// Remove all accounts
        /// </summary>
        /// <returns></returns>
        public int clear()
        {
            try
            {
                usersList.Clear();
                return 0;
            }
            catch (Exception)
            {
                return 1;
            }
        }

        /// <summary>
        /// Change user password 
        /// </summary>
        /// <param name="login">identifier number</param>
        /// <param name="oldPasswd">current user password</param>
        /// <param name="newPasswd">new user password</param>
        /// <returns>0 if success, 1 if login don't exist, 2 if wrong old password</returns>
        public int changePassword(string login, string oldPasswd, string newPasswd)
        {
            if (!exist(login)) return 1;
            if (usersList[login].PasswordSHA == oldPasswd)
            {
                 usersList[login].PasswordSHA = newPasswd;
                 return 0;
            }
            else return 2;    
        }

        /// <summary>
        /// Change user nickname
        /// </summary>
        /// <param name="login">identifier number</param>
        /// <param name="newNick">new nick name</param>
        /// <returns></returns>
        public int changeNickName(string login, string newNick)
        {
            if (!exist(login)) return 1;
            usersList[login].NickName = newNick;
            return 0;
        }

        public string this[string key]
        {
            get
            {
                return usersList[key].NickName + ' ' + usersList[key].PasswordSHA;
            }
        }

        public IEnumerator<KeyValuePair<string,UserAccount>> GetEnumerator()
        {
            return usersList.GetEnumerator();
        }

        /// <summary>
        /// Create and save XML file, which contains all usersList elements
        /// </summary>
        public void writeToXML()
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            XmlWriter document = XmlWriter.Create("users.xml", settings);
            document.WriteStartDocument();
            document.WriteStartElement("users");

            foreach (var elem in usersList)
            {
                document.WriteStartElement("user");
                document.WriteAttributeString("id", elem.Key);

                document.WriteStartElement("password");
                document.WriteString(elem.Value.PasswordSHA);
                document.WriteEndElement();

                document.WriteStartElement("nickname");
                document.WriteString(elem.Value.NickName);
                document.WriteEndElement();
                document.WriteEndElement();
            }
            document.WriteEndElement();
            document.WriteEndDocument();
            document.Close();
        }

        /// <summary>
        /// Read content of XML file and save into this object.
        /// </summary>
        public void readFromXML()
        {
            XmlDocument document = new XmlDocument();
            document.Load("users.xml");
            XmlElement root = document.DocumentElement;

            foreach (XmlNode node in root.ChildNodes)
            {
                if (node.NodeType == XmlNodeType.Element)
                {
                    UserAccount ua = new UserAccount();
                    foreach (XmlNode contact in node.ChildNodes)
                    {
                        if (contact.Name == "password") ua.PasswordSHA = contact.FirstChild.Value;
                        if (contact.Name == "pinned") ua.NickName = contact.FirstChild.Value;
                    }
                    usersList.TryAdd(node.Attributes["id"].Value, ua);
                }
            }
        }

        /// <summary>
        /// Create new UserAccountList and save content of XML file
        /// </summary>
        /// <returns>new UserAccountList object</returns>
        public static Task<UserAccountList> createFromXMLAsync()
        {
            return Task.Run(() =>
            {
                var dict = new UserAccountList();
                XmlDocument document = new XmlDocument();
                document.Load("users.xml");
                XmlElement root = document.DocumentElement;

                foreach (XmlNode node in root.ChildNodes)
                {
                    if (node.NodeType == XmlNodeType.Element)
                    {
                        UserAccount ua = new UserAccount();
                        foreach (XmlNode contact in node.ChildNodes)
                        {
                            if (contact.Name == "password") ua.PasswordSHA = contact.FirstChild.Value;
                            if (contact.Name == "pinned") ua.NickName = contact.FirstChild.Value;
                        }
                        dict.usersList.TryAdd(node.Attributes["id"].Value, ua);
                    }
                }
                return dict;
            });
        }
    }

}
