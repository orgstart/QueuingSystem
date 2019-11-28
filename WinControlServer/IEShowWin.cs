using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WinControlServer
{
    public partial class IEShowWin : Form
    {
        #region 全局变量

        private string _strCardNo;//身份证号码
        private string _strBMDM;//部门代码


        #endregion

        #region 构造函数

        public IEShowWin()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strCardNo">身份证号</param>
        /// <param name="strBMDM">部门编码</param>
        public IEShowWin(string strCardNo,string strBMDM)
        {
            InitializeComponent();
            this._strCardNo = strCardNo;//身份证号码
            this._strBMDM = strBMDM;//部门代码
        }

        #endregion

        #region 窗体事件

        /// <summary>
        /// 窗体加载事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IEShowWin_Load(object sender, EventArgs e)
        {
            string strWebUrl = ConfigurationManager.ConnectionStrings["WebUrl"].ConnectionString; //网址URL
            Uri sdnUrl = new Uri(strWebUrl+"/BMDM="+_strBMDM+"&JSZH="+_strCardNo);
            sdnWebBrowser.Url = sdnUrl;
            sdnWebBrowser.ObjectForScripting = this;

         //   btnSInName.Enabled = false;
            btnSinNo.Enabled = false;
            btnPrint.Enabled = false;
            btnNoPrint.Enabled = false;
            
        }



        #endregion

        #region html与JS交互
        /// <summary>
        /// 关闭当前页面
        /// </summary>
        public void sdnClose()
        {
            this.Close();
        }
        /// <summary>
        ///签字 姓名
        /// </summary>
        public void sdnSinName()
        {
            // MessageBox.Show(address);
            sdnWebBrowser.Document.InvokeScript("GetForQuePrint");
        }
        /// <summary>
        /// 签名无异议
        /// </summary>
        public void sdnSinNo()
        {
            sdnWebBrowser.Document.InvokeScript("GetForQuePrint");
        }
        /// <summary>
        /// 确认打印
        /// </summary>
        public void sdnPrint()
        {
            sdnWebBrowser.Document.InvokeScript("GetForQuePrint");
        }
        /// <summary>
        /// 确认不打印
        /// </summary>
        public void sdnNoPrint()
        {
            sdnWebBrowser.Document.InvokeScript("GetForQuePrint");
        }


        #endregion

        #region 按钮事件
        /// <summary>
        /// 签字 姓名
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSInName_Click(object sender, EventArgs e)
        {
            btnSInName.Enabled = false;
            btnSinNo.Enabled = true;
            btnPrint.Enabled = false;
            btnNoPrint.Enabled = false;
            sdnSinName();
        }
        /// <summary>
        /// 签字 无异议
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSinNo_Click(object sender, EventArgs e)
        {
            btnSInName.Enabled = false;
            btnSinNo.Enabled = false;
            btnPrint.Enabled = true;
            btnNoPrint.Enabled = true;
            sdnSinNo();
        }
        /// <summary>
        /// 确认打印
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPrint_Click(object sender, EventArgs e)
        {
            sdnPrint();
        }
        /// <summary>
        /// 确认不打印
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnNoPrint_Click(object sender, EventArgs e)
        {
            sdnNoPrint();
        }

        #endregion



    }
}
