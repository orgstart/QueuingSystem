using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace WinControlServer
{
    public partial class wj_pj : Form
    {

        #region 全局变量
        //    private static AxSCOREOCXLib.AxScoreOcx sdnOCX = new AxSCOREOCXLib.AxScoreOcx();
        private string _strCardNo;//身份证号码
        private string _strBMDM;//部门代码
        private string _strQueSN;//24位取票序列号
        private string comNo = "1";//评价器串口地址
        private string comAddr = "1";//评价器地址
        private string StarNo = "1";//星评分数


        #endregion

        public wj_pj(string strQueSN)
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            //this._strCardNo = strCardNo;//身份证号码
            //this._strBMDM = strBMDM;//部门代码
            this._strQueSN = strQueSN;
        }

        #region 评价按钮

        /// <summary>
        /// 非常满意
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn5Star_Click(object sender, EventArgs e)
        {
            StarNo = "5";
            btn5Star.Enabled = false;
            btn4Star.Enabled = false;
            btn3Star.Enabled = false;
            btn2Star.Enabled = false;
            sdnAddPjdata();

        }

        /// <summary>
        /// 满意
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn4Star_Click(object sender, EventArgs e)
        {
            StarNo = "4";
            btn5Star.Enabled = false;
            btn4Star.Enabled = false;
            btn3Star.Enabled = false;
            btn2Star.Enabled = false;
            sdnAddPjdata();
        }
        /// <summary>
        /// 一般
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn3Star_Click(object sender, EventArgs e)
        {
            StarNo = "3";
            btn5Star.Enabled = false;
            btn4Star.Enabled = false;
            btn3Star.Enabled = false;
            btn2Star.Enabled = false;
            sdnAddPjdata();
        }
        /// <summary>
        /// 不满意
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn2Star_Click(object sender, EventArgs e)
        {
            StarNo = "2";
            btn5Star.Enabled = false;
            btn4Star.Enabled = false;
            btn3Star.Enabled = false;
            btn2Star.Enabled = false;
            sdnAddPjdata();
        }

        #endregion


        #region 插入评价数据
        /// <summary>
        /// 通过webservice 上传评价数据
        /// </summary>
        /// <param name="arrobj"></param>
        private void sdnAddPjdata()
        {
            try
            {
                string strRes = "0";
                using (examService.ExamServiceSoapClient client = new examService.ExamServiceSoapClient())
                {
                    strRes = client.AddEvaluatData(GetLocalIP(), _strCardNo, comAddr, _strQueSN, StarNo);
                }
                if (strRes == "1") //上传评价信息成功
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("上传评价信息失败，请再次上传!");
                    btn5Star.Enabled = true;
                    btn4Star.Enabled = true;
                    btn3Star.Enabled = true;
                    btn2Star.Enabled = true;
                }
            }
            catch { }
        }

        #endregion

        #region 获取本机IP

        private string GetLocalIP()
        {
            try
            {
                string HostName = Dns.GetHostName(); //得到主机名
                IPHostEntry IpEntry = Dns.GetHostEntry(HostName);
                for (int i = 0; i < IpEntry.AddressList.Length; i++)
                {
                    //从IP地址列表中筛选出IPv4类型的IP地址
                    //AddressFamily.InterNetwork表示此IP为IPv4,
                    //AddressFamily.InterNetworkV6表示此地址为IPv6类型
                    if (IpEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                    {
                        return IpEntry.AddressList[i].ToString();
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                MessageBox.Show("获取本机IP出错:" + ex.Message);
                return "";
            }
        }

        #endregion
        
        /// <summary>
        /// 窗口加载事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void wj_pj_Load(object sender, EventArgs e)
        {
            try
            {
                string strPath = AppDomain.CurrentDomain.BaseDirectory + "\\config\\sdnsystem.ini";
                ReadIniFile readIni = new ReadIniFile(strPath);
                comNo = readIni.ReadValue("pj", "com");
                comAddr = readIni.ReadValue("pj", "addr");
                this._strBMDM = readIni.ReadValue("bmdm", "value"); //得到部门代码
            }
            catch (Exception ex)
            {
               // lbLogs.Text = ex.Message;
            }
        }
        /// <summary>
        /// 窗体关闭事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void wj_pj_FormClosed(object sender, FormClosedEventArgs e)
        {
        }
        /// <summary>
        /// 关闭当前页面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancle_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
