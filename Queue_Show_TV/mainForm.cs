using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Xilium.CefGlue.WindowsForms;

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
            browser.StartUrl = "http://www.baidu.com";
          
            pl_brower.Controls.Add(browser);
        //    browser.Browser.GetMainFrame().LoadUrl("http://opensource.spotify.com/cefbuilds/index.html");
        }

        #endregion
    }
}
