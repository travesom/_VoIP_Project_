using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Telefon_klient
{       
    public partial class Main_form : Form
    {   string user_token;
        string login;
        public Main_form(string token,string login)
        {
            this.user_token = token;
            this.login = login;
            InitializeComponent();
            label_username.Text = this.login;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Hide();
            Form1 f1 = new Form1();
            f1.Show();
        }
    }
}
