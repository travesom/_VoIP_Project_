using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Telefon_klient
{
    static class Program
    {   
        /// <summary>
        /// Główny punkt wejścia dla aplikacji.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            
            if (args == null || args.Length < 1)
            {
                
            }
            else
            Form1.machineName = args[0];
            if (args.Length < 2)
            {
                Form1.serverName = Form1.machineName;
            }
            else
            {
                Form1.serverName = args[1];
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
