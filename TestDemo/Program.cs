using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestDemo
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            UpdateOnline.Form1 f1 = new UpdateOnline.Form1();
            if (f1.WhetherUpdate())
            {
                System.Diagnostics.Process ps = new System.Diagnostics.Process();
                var path = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)
                             + Path.DirectorySeparatorChar.ToString();
                ps.StartInfo.FileName = $@"{path}\UpdateOnline.exe";
                ps.Start();
                Application.Exit();
            }
            else
            {
                
                Application.Run(new Form1());
            }
        }
    }
}
