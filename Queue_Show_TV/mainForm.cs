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
        private string strPort = "9999";
        CefWebBrowser browser = new CefWebBrowser();
        public mainForm()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        #region 窗体事件

        private void mainForm_Load(object sender, EventArgs e)
        {
            string strUrl = "";
            if (File.Exists(Directory.GetCurrentDirectory() + "\\system.ini"))
            {
                ReadIniFile readIni = new ReadIniFile(Directory.GetCurrentDirectory() + "\\system.ini");
                string strLocal = readIni.ReadValue("url", "local");
                string strValue = readIni.ReadValue("url", "value");
                strPort = readIni.ReadValue("port", "value");
                if (strLocal.Trim() == "1") //为本地文件
                {
                    strUrl = Directory.GetCurrentDirectory() + strValue;
                }
                else
                {
                    strUrl = strValue;
                }
                browser.Dock = DockStyle.Fill;
                // browser.StartUrl = "https://localhost:44320/QueueShow_TV.html";
                browser.StartUrl = strUrl;
                pl_brower.Controls.Add(browser);
                initHttpServer();
            }
            else
            {
                MessageBox.Show("配置文件system.ini不存在或不可用");
                return;
            }
          
        }

        private void initHttpServer()
        {
            httpServer.sdnHttpServer sdn_httpServer = new httpServer.sdnHttpServer("", Convert.ToInt32(strPort));
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
