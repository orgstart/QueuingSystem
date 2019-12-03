using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Common.Redis;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WinControlServer
{
    public partial class QueueClient : sdnControls.sdnSkinForm.SkinForm
    {
        /// <summary>
        /// redis帮助类
        /// </summary>
        // RedisStackExchangeHelper _redis = new RedisStackExchangeHelper(); //实例化redis帮助类
        public static RedisStackExchangeHelper _redis = null; //实例化redis帮助类
        #region 全局变量
        TcpSocket tcpSocket;
        /// <summary>
        /// 当前窗口号
        /// </summary>
        string calladdr = "1";

        string sdnWaitNum = "0"; //当前等待人数
        string sdnNowPeople = "";//当前办理业务人员
        string sdnNowNo = "";//当前办理业务号码
        string sdnCardNo = "F0";//"888888888888888888";//当前办理人员身份证号码
        string sdnbmdm = "";//部门代码
        string sdnqpxxxlh = "";//取票信息序列号
        string strLocalIp = "127.0.0.1";//本机IP
        string strQueueInfo = "";
        public delegate string del_Get_Curr_Que();
        public event del_Get_Curr_Que event_get_curr_que;
        #endregion
        public QueueClient()
        {
            InitializeComponent();
            InitTcpSocket(); //实例化通讯类
        }

        #region 初始化套节字
        /// <summary>
        /// 初始化
        /// </summary>
        private void InitTcpSocket()
        {
            try
            {
                //实例化通讯类
             //   tcpSocket = new TcpSocket();
              //  tcpSocket.eventTcpSocketValue += TcpSocketValue;

                string strPath = AppDomain.CurrentDomain.BaseDirectory + @"config\sdnsystem.ini";
                if (!string.IsNullOrEmpty(strPath))
                {
                    ReadIniFile sdnReadIni = new ReadIniFile(strPath);
                    calladdr = sdnReadIni.ReadValue("winNum", "value");//当前窗口号
                //    tcpSocket.calladdr = calladdr;
                    strLocalIp = sdnReadIni.ReadValue("address", "ip");//得到本机IP
                 //   tcpSocket.strLocalIp = strLocalIp;
                }
                else
                {
                 //   tcpSocket.calladdr = "1";
                }

                new Thread(tcpSocket.ConnServer).Start(GetLocalIP());
                // new Thread(tcpSocket.ConnServer).Start("192.1.6.143");
            }
            catch (Exception ex)
            {
                // Common.SysLog.WriteLog(ex, strLogPath);
            }
        }

        private void TcpSocketValue(string count, string queNo, string IdNum, string strName, string bmdm, string qpxxxlh)
        {
            this.sdnWaitNum = count;
            if (queNo != "-1")
            {
                this.sdnNowPeople = queNo;
                this.sdnCardNo = IdNum;
                this.lbQueNo.Text = queNo;
                this.sdnbmdm = bmdm;
                this.sdnqpxxxlh = qpxxxlh;
                if (string.IsNullOrWhiteSpace(queNo))
                {
                    set_btns_enabled(new int[] { 1 });
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(queNo))
                {
                    set_btns_enabled(new int[] { 1 });
                }
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

        #region 窗体靠近屏幕边上时 自动隐藏（同QQ）
        /// <summary>
        /// 窗体位置改变时触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Main_Monitor_LocationChanged(object sender, EventArgs e)
        {
            this.mStopAnhor();
        }
        /// <summary>
        /// 窗体隐藏与显现的时间计时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_form_hidden_Tick(object sender, EventArgs e)
        {
            try
            {
                if (this.Bounds.Contains(Cursor.Position))
                {
                    switch (this.StopAanhor)
                    {
                        case AnchorStyles.Top:
                            this.Location = new Point(this.Location.X, 0);
                            break;
                        case AnchorStyles.Left:
                            this.Location = new Point(0, this.Location.Y);
                            break;
                        case AnchorStyles.Right:
                            this.Location = new Point(Screen.PrimaryScreen.Bounds.Width - this.Width, this.Location.Y);
                            break;
                    }
                }
                else
                {
                    switch (this.StopAanhor)
                    {
                        case AnchorStyles.Top:
                            this.Location = new Point(this.Location.X, (this.Height - 2) * (-1));
                            break;
                        case AnchorStyles.Left:
                            this.Location = new Point((-1) * (this.Width - 2), this.Location.Y);
                            break;
                        case AnchorStyles.Right:
                            this.Location = new Point(Screen.PrimaryScreen.Bounds.Width - 2, this.Location.Y);
                            break;
                    }
                }
            }
            catch
            { }
        }
        internal AnchorStyles StopAanhor = AnchorStyles.None;
        private void mStopAnhor()
        {
            if (timer_form_hidden.Enabled == false)
            {
                timer_form_hidden.Start();
            }
            if (this.Top <= 0)
            {
                StopAanhor = AnchorStyles.Top;
            }
            else if (this.Left <= 0)
            {
                StopAanhor = AnchorStyles.Left;
            }
            else if (this.Left >= Screen.PrimaryScreen.Bounds.Width - this.Width)
            {
                StopAanhor = AnchorStyles.Right;
            }
            else
            {
                StopAanhor = AnchorStyles.None;
            }
        }
        #endregion

        #region 叫号端按钮
        /// <summary>
        /// 叫号
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCallNum_Click(object sender, EventArgs e)
        {
            //设置重叫 到达按钮可见
            set_btns_enabled(new int[] { 2, 4 });
            //发送叫号数据
            tcpSocket.SendData("call", "");
        }
        /// <summary>
        /// 重叫
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnReCall_Click(object sender, EventArgs e)
        {
            set_btns_enabled(new int[] { 2, 3, 4 });
            tcpSocket.SendData("recall", sdnNowPeople);

        }
        /// <summary>
        /// 跳号
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSkip_Click(object sender, EventArgs e)
        {
            //设置重叫 到达按钮可见
            set_btns_enabled(new int[] { 1 });
            tcpSocket.SendData("jump", sdnNowPeople);
        }
        /// <summary>
        /// 到达
        /// </summary>
        /// <param name="sender">here</param>
        /// <param name="e"></param>
        private void btnHere_Click(object sender, EventArgs e)
        {
            set_btns_enabled(new int[] { 5 });
            tcpSocket.SendData("here", sdnNowPeople);
        }
        /// <summary>
        /// 结束
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOver_Click(object sender, EventArgs e)
        {
            set_btns_enabled(new int[] { 6 });
            tcpSocket.SendData("dook", sdnNowPeople);
        }
        /// <summary>
        /// 评价
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnScore_Click(object sender, EventArgs e)
        {
            try
            {
                this.btnScore.Enabled = false;
                string strQueue = strQueueInfo; //得到评价信息
                if (!string.IsNullOrEmpty(strQueue))
                {
                    JObject jobj = (JObject)JsonConvert.DeserializeObject(strQueue);
                    if (jobj["winnum"].ToString() == calladdr)
                    {
                        this.lbQueNo.Text = jobj["queue"].ToString();
                        wj_pj sdn_pj = new wj_pj(this.lbQueNo.Text);//吴江评价
                        sdn_pj.ShowDialog();
                        if (sdn_pj.DialogResult == DialogResult.Cancel) //正常评价
                        {
                            MessageBox.Show("评价取消,请再次提交评价！！！");
                            this.btnScore.Enabled = true;
                        }
                        else //意外关闭
                        {
                            MessageBox.Show("评价成功");
                            this.btnScore.Enabled = true;
                            strQueueInfo = ""; //清空当前数据
                        }
                    }
                    else
                    {
                        MessageBox.Show("当前窗口没有办理信息");
                        this.btnScore.Enabled = true;
                    }

                }
                else
                {
                    MessageBox.Show("当前没有办理信息");
                    this.btnScore.Enabled = true;
                }
            }
            catch
            {

            }

        }

        /// <summary>
        /// 设置按钮可用性
        /// </summary>
        /// <param name="arrIds"></param>
        private void set_btns_enabled(int[] arrIds)
        {
            btnCallNum.Enabled = false; //叫号
            btnHere.Enabled = false; //到达
            btnOver.Enabled = false; //结束
            btnReCall.Enabled = false; //重叫
                                       //  btnScore.Enabled = false; //评价
            btnSkip.Enabled = false; //跳号

            foreach (int i in arrIds)
            {
                switch (i)
                {
                    case 1: //叫号
                        btnCallNum.Enabled = true;
                        break;
                    case 2://重叫
                        btnReCall.Enabled = true;
                        break;
                    case 3://跳号
                        btnSkip.Enabled = true;
                        break;
                    case 4://到达
                        btnHere.Enabled = true;
                        break;
                    case 5://完成
                        btnOver.Enabled = true;
                        break;
                    case 6://评价
                        btnCallNum.Enabled = true;
                        btnScore.Enabled = true;
                        break;
                }
            }
        }
        #endregion

        #region 窗口load
        /// <summary>
        /// 窗口 load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QueueClient_Load(object sender, EventArgs e)
        {
            btnCallNum.Enabled = true;
            btnHere.Enabled = false; //到达
            btnOver.Enabled = false; //结束
            btnReCall.Enabled = false; //重叫
                                       //    btnScore.Enabled = false; //评价
            btnSkip.Enabled = false; //跳号
                                     // new Thread(refreshcount).Start();//刷新排队人数,观山外挂软件叫号使用
            try
            {
                //注册钩子事件
                //  scanerHook.Start();
                _redis = new RedisStackExchangeHelper(); //实例化redis帮助类
                sub_msg("sdnsound");
            }
            catch { }
        }

        #endregion

        /// <summary>
        /// 刷新排队人数,观山外挂软件叫号使用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void refreshcount()
        {
            while (true)
            {
                try
                {
                    //发送叫号数据
                    tcpSocket.SendData("count", "");
                }
                catch
                { }
                Thread.Sleep(20 * 1000);
            }
        }

        public void setinfo(string queNo, string IdNum, string strName, string count)
        {
            this.lbQueNo.Text = queNo;

            if (string.IsNullOrWhiteSpace(queNo))
            {
                set_btns_enabled(new int[] { 1 });
            }
        }


        #region redis 订阅函数
        /// <summary>
        /// 得到当前排队信息
        /// </summary>
        /// <returns></returns>
        public string get_curr_queue()
        {
            try
            {
                return strQueueInfo;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// 订阅指定的频道，接收对应信息
        /// </summary>
        /// <param name="channel"></param>
        private async void sub_msg(string channel)
        {
            await _redis.SubscribeAsync(channel, (cha, message) =>
            {
                try
                {
                    strQueueInfo = message;
                    this.lbRunInfo.Text = message;
                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        JObject jobj = (JObject)JsonConvert.DeserializeObject(strQueueInfo);
                        if (jobj["winnum"].ToString() == calladdr)
                        {
                            this.lbQueNo.Text = jobj["queue"].ToString();
                        }
                    }
                   
                }
                catch { }
            });
        }

        #endregion


    }
}
