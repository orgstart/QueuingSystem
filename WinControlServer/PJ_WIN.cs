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
using WinControlServer.Properties;

namespace WinControlServer
{
    public partial class PJ_WIN : Form
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

        #region 析构函数
        public PJ_WIN()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strCardNo">身份证号</param>
        /// <param name="strBMDM">部门编码</param>
        public PJ_WIN(string strCardNo, string strBMDM, string strQueSN)
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            this._strCardNo = strCardNo;//身份证号码
            this._strBMDM = strBMDM;//部门代码
        }
        #endregion

        #region 窗体事件
        /// <summary>
        /// 窗口加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PJ_WIN_Load(object sender, EventArgs e)
        {
            try
            {
                
        //        ojOCX = sdnOCX;
                lbCardNo.Text = _strCardNo; //主界面显示评价人证件号
                string strPath = AppDomain.CurrentDomain.BaseDirectory + "\\config\\sdnsystem.ini";
                ReadIniFile readIni = new ReadIniFile(strPath);
                comNo = readIni.ReadValue("pj", "com");
                comAddr = readIni.ReadValue("pj", "addr");
                picStar5.BackgroundImage = Resources.star_gray;
                picStar4.BackgroundImage = Resources.star_gray;
                picStar3.BackgroundImage = Resources.star_gray;
                picStar2.BackgroundImage = Resources.star_gray;
                picStar1.BackgroundImage = Resources.star_gray;
                lbLogs.Text = "请求评价成功";

            }
            catch (Exception ex)
            {
                lbLogs.Text = ex.Message;
            }
        }

        /// <summary>
        /// 提交数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSubmit_Click(object sender, EventArgs e)
        {

            btnSubmit.Enabled = false;//不可用
            sdnAddPjdata();

        }
        #endregion
        /// <summary>
        /// ocx回调 得到评价值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ojOCX_OnScore(object sender,object e)
        {
            lbLogs.Text = "评价完成";
            //1、向服务器提交评价信息
            StarNo = e.ToString();
            //页面根据分数变化
            switch (StarNo)
            {
                case "3"://非常满意 5
                    picStar5.BackgroundImage = Resources.star;
                    picStar4.BackgroundImage = Resources.star;
                    picStar3.BackgroundImage = Resources.star;
                    picStar2.BackgroundImage = Resources.star;
                    picStar1.BackgroundImage = Resources.star;
                    break;
                case "2"://满意 4
                    picStar5.BackgroundImage = Resources.star;
                    picStar4.BackgroundImage = Resources.star;
                    picStar3.BackgroundImage = Resources.star;
                    picStar2.BackgroundImage = Resources.star;
                    picStar1.BackgroundImage = Resources.star_gray;
                    break;
                case "1"://一般 3
                    picStar5.BackgroundImage = Resources.star;
                    picStar4.BackgroundImage = Resources.star;
                    picStar3.BackgroundImage = Resources.star;
                    picStar2.BackgroundImage = Resources.star_gray;
                    picStar1.BackgroundImage = Resources.star_gray;
                    break;
                case "0"://不满意 2
                    picStar5.BackgroundImage = Resources.star;
                    picStar4.BackgroundImage = Resources.star;
                    picStar3.BackgroundImage = Resources.star_gray;
                    picStar2.BackgroundImage = Resources.star_gray;
                    picStar1.BackgroundImage = Resources.star_gray;
                    break;
                default:  //默认 非常满意 5
                    picStar5.BackgroundImage = Resources.star;
                    picStar4.BackgroundImage = Resources.star;
                    picStar3.BackgroundImage = Resources.star;
                    picStar2.BackgroundImage = Resources.star;
                    picStar1.BackgroundImage = Resources.star;
                    break;
            }
            //2、提交成功后，关闭评价器
            //btnSubmit.Visible = true;
            //btnSubmit.Enabled = true;//不可用
            btnSubmit.Visible = false;
            btnSubmit.Enabled = false;//不可用
            //评价之后直接提交，上传数据

            sdnAddPjdata();
        }

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
                    strRes= client.AddEvaluatData(GetLocalIP(), _strCardNo, comAddr, _strQueSN, StarNo);
                }
                if (strRes == "1") //上传评价信息成功
                {
                    this.Close();
                }else
                {
                    MessageBox.Show("上传评价信息失败，请再次上传!");
                    btnSubmit.Visible = true;
                    btnSubmit.Enabled = true;//不可用
                }
            }
            catch { }
        }



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


    }
}
