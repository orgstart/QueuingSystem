using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Xilium.CefGlue.WindowsForms;
using System.IO;

namespace Queue_Show_TV
{
    public partial class mainForm : Form
    {
        CefWebBrowser browser = new CefWebBrowser();
        public mainForm()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        #region 窗体事件

        private void mainForm_Load(object sender, EventArgs e)
        {
            browser.Dock = DockStyle.Fill;
           // browser.StartUrl = "https://localhost:44320/QueueShow_TV.html";
            browser.StartUrl = Directory.GetCurrentDirectory()+ "\\sdnWeb\\QueueShow_TV.html";
         //   browser.StartUrl = @"./sdnWeb/QueueShow_TV.html";
            pl_brower.Controls.Add(browser);
            initHttpServer();
        }

        private void initHttpServer()
        {
            httpServer.sdnHttpServer sdn_httpServer = new httpServer.sdnHttpServer("", 8080);
            sdn_httpServer.event_up_tv_queue += RunScirpt;
            new Thread(sdn_httpServer.listen).Start();
        }

        #endregion

        /// <summary>
        /// 执行JS脚本
        /// </summary>
        /// <param name="js">"CreatePage(1,2,3);"</param>
        public void RunScirpt(string js)
        {
            var frame = browser.Browser.GetMainFrame();
            frame.ExecuteJavaScript(js, frame.Url, 0);
        }

        //private void button1_Click(object sender, EventArgs e)
        //{
        //    string strFun = "update_queue('{\"count\":10,\"done\":[{\"que_no\":\"1005\",\"win_no\":\"1\"},{\"que_no\":\"1004\",\"win_no\":\"1\"},{\"que_no\":\"1003\",\"win_no\":\"1\"},{\"que_no\":\"1002\",\"win_no\":\"1\"},{\"que_no\":\"1001\",\"win_no\":\"1\"}],\"wait\":\"1006,1007,1008,1009\"}')";
        //   // Thread.Sleep(10 * 1000);
        //    RunScirpt(strFun);
        //}
    }
}
