using CefSharp;
using CefSharp.WinForms;
using System;
using System.Windows.Forms;

namespace QueuingSystem
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new sdnMainForm());
            const bool simpleSubProcess = true;

            // Cef.EnableHighDPISupport();

            //NOTE: Using a simple sub processes uses your existing application executable to spawn instances of the sub process.
            //Features like JSB, EvaluateScriptAsync, custom schemes require the CefSharp.BrowserSubprocess to function
            if (simpleSubProcess)
            {
                //var exitCode = Cef.ExecuteProcess();

                var settings = new CefSettings();
                // settings.BrowserSubprocessPath = "CefSharp.WinForms.Example.exe";

                Cef.Initialize(settings);

                var browser = new sdnMainForm();
                Application.Run(browser);
            }
            Cef.Shutdown();

        }
    }
}
