using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    }

    /// <summary>
    /// Class use to save all user accounts on server.
    /// Have functions to connect to XML files to get/save data
    /// </summary>
    class UserAccountList
    {
        //key = user login (UNIQUE value to be use everywhere to identify user)
        private Dictionary<string, UserAccount> usersList;  

        public UserAccountList()
        {
            usersList = new Dictionary<string, UserAccount>();
        }

        public bool exist(string login)
        {
            return usersList.ContainsKey(login);
        }

        public int createAccount(string login, string passwd)
        {
            UserAccount u1 = new UserAccount(passwd);
            if (exist(login)) return 1;
            usersList.Add(login, u1);
            return 0;
        }

        public int createAccount(string login, string passwd, string pseudonim)
        {
            UserAccount u1 = new UserAccount(passwd,pseudonim);
            if (exist(login)) return 1;
            usersList.Add(login, u1);
            return 0;
        }
           
        public int removeAccount(string login)
        {
            if (!exist(login)) return 1;
            usersList.Remove(login);
            return 0;
        }

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
                return usersList[key].NickName;
            }
        }
    }

}
