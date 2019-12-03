using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace WinControlServer
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool createdNew = false;
            Mutex instance = new Mutex(true, Process.GetCurrentProcess().MainModule.FileName.Replace("\\", "/"), out createdNew);
            if (createdNew)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new mainControl());
                //   Application.Run(new wj_pj());
            }

        }
    }
}
