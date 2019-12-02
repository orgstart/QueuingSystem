﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace WinControlServer
{
    public partial class mainControl : Form
    {
        public mainControl()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        #region 全局变量
        private string strLocalIp = "";//本机IP
        private delegate void delsdnOpenWin(string strCardNo, string strBMDM, string strQueSN);
        delsdnOpenWin delOpen = null;
        QueueClient sdnClient = null;//取票客户端
        #endregion

        #region 窗体事件

        private void mainControl_Load(object sender, EventArgs e)
        {

            strLocalIp = GetLocalIP();//获取本地IP
            initTcpServer();
            delOpen = new delsdnOpenWin(openPJwin);//实例化委托
            sdnClient = new QueueClient();
            sdnClient.Show();
        }

        /// <summary>
        /// 窗口尺寸改变函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mainControl_SizeChanged(object sender, EventArgs e)
        {
            //判断是否选择的是最小化按钮
            if (WindowState == FormWindowState.Minimized)
            {
                //隐藏任务栏区图标
                //this.ShowInTaskbar = false;
                //图标显示在托盘区
                sdnnotifyIcon.Visible = true;
            }
        }
        /// <summary>
        /// 设置按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void settingMenuItem_Click(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// 退出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExitMenuItem_Click(object sender, EventArgs e)
        {
            MessageBoxButtons messButton = MessageBoxButtons.OKCancel;
            DialogResult dr = MessageBox.Show("退出本系统后综合平台某些业务无法使用，确定退出？", "退出系统", messButton);
            if (dr == DialogResult.OK) //确定要退出
            {
                this.Close();
                Environment.Exit(-1); //强制退出系统
            }
        }
        /// <summary>
        /// 打开取票客户端
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void queClient_Click(object sender, EventArgs e)
        {

            if (sdnClient == null)
            {
                sdnClient = new QueueClient();
            }
            else
            {
                sdnClient.Close();
                sdnClient = new QueueClient();
            }
            sdnClient.Show();
        }

        #endregion

        #region 初始化通信服务
        /// <summary>
        /// 初始化TCP服务
        /// </summary>
        private void initTcpServer()
        {
            string srtIp = "127.0.0.1";
            if (!string.IsNullOrEmpty(strLocalIp))
            {
                srtIp = strLocalIp;
            }
            else
            {
                string strPath = AppDomain.CurrentDomain.BaseDirectory + "\\config\\sdnsystem.ini";
                ReadIniFile readini = new ReadIniFile(strPath);
                srtIp = readini.ReadValue("address", "ip");
            }
            TcpSocketServer tcpServer = new TcpSocketServer(srtIp);
            tcpServer.eventOpenWin += sdnPenIeWin;
            tcpServer.eventSetQueue += setQueueWin;
            // new Thread(tcpServer.Start).Start();//开启tcp服务
            //插入一个新线程用于处理验证码
            //   Thread thd = new Thread(new ParameterizedThreadStart(tcpServer.Start));
            Thread thd = new Thread(tcpServer.Start);
            thd.SetApartmentState(ApartmentState.STA);//关键设置
            thd.IsBackground = true;
            thd.Start();
            thd.Join();//主线程等待，临时线程开始处理

            //临时线程结束，主线程继续运行
        }
        #endregion

        #region UI响应服务事件
        /// <summary>
        /// 打开IE窗口
        /// </summary>
        private void sdnPenIeWin(string strCardNo, string strBMDM, string strQueSN)
        {
            //IEShowWin sdnIeShow = new IEShowWin(strCardNo, strBMDM);
            //sdnIeShow.ShowDialog();
            // sdnIeShow.Show();
            //this.StartPosition = FormStartPosition.CenterScreen;
            ////this.ShowDialog();
            //this.ShowInTaskbar = true;
            //this.WindowState = FormWindowState.Normal;
            //string strWebUrl = ConfigurationManager.ConnectionStrings["WebUrl"].ConnectionString; //网址URL
            //string strAllUrl = strWebUrl + "?BMDM=" + strBMDM + "&JSZH=" + strCardNo;
            ////Process.Start("IExplore.exe", "www.baidu.com");

            //Process.Start("IExplore.exe", strAllUrl);
            this.BeginInvoke(delOpen, strCardNo, strBMDM, strQueSN);

        }

        /// <summary>
        /// invoke 打开窗口
        /// </summary>
        /// <param name="strCardNo"></param>
        /// <param name="strBMDM"></param>
        /// <param name="strQueSN"></param>
        private void openPJwin(string strCardNo, string strBMDM, string strQueSN)
        {
            try
            {
                PJ_WIN sdn_pj = new PJ_WIN(strCardNo, strBMDM, strQueSN);
                sdn_pj.Show();
            }
            catch (Exception ex)
            {

            }
        }

        private void setQueueWin(string queuenum, string cardnum, string xm,string count)
        {
            this.Invoke(new Action <string,string,string,string >(uifun),queuenum,cardnum,xm,count);
        }

        private void uifun(string queuenum, string cardnum, string xm,string count)
        {
            if (sdnClient != null)
            {
                sdnClient.setinfo(queuenum, cardnum, xm,count);
            }
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





    }
}