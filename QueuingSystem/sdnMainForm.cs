using Common.Redis;
using QueueSys.Model;
using QueuingSystem.hardware;
using QueuingSystem.OperQueue;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace QueuingSystem
{
    [ComVisible(true)]
    public partial class sdnMainForm : Form
    {
        #region 全局变量

        private string strExePath = ""; //exe 程序所在路径
        private string strHtmlPath = ""; //html页面路径
        private QueueList sdnQueList = null;//队列 list 正常排队队列
        private QueueList sdnQueList_YY = null;//网上预约排队队列

        private QueueList sdnQueList_YQ = null;//园区 其他业务排队
        private Dictionary<string, int> dicNoTimes = null;//证件号与取票次数字典
        private int iQueNo = 0;//排队初始号码  数字格式，排队总人数
        private string strQueNo = "001";//排队初始化排队号码  标准格式
        private string sdn_CardNo = "";//身份证号码
        private string strQueType = "1"; //取票种类
        private int iMaxA = 0, iMaxB = 0, iMaxC = 0; //分别以A B C 开头的队列最大值
        private int iMaxD = 0;//园区特殊屏  

        private string isWebShow = "0";//是否显示web网页
        private int i_showType = 7; //排队信息展示类型 默认为LED1 显示
        private string strDS = "0";//电视信息是否显示（一机双屏）默认不显示

        private int iAllCount = 0;//总取票人数
        private int iNoDealCount = 0;//等待叫号人数
        private int iDealCount = 0;//处理完成人数

        private int iMaxCount = 2;//当天最大取票次数
        private string strAdress = "交通违法处理点"; //违法处理地点
        private string strBBDM = "320506005300";//违法处理部门网点
        private string strMachine = "001000001";//唯一机器码，前三位机器种类后六位唯一编号
        private string strJH = "";//配置警号

        private string strUrl = "";//远程数据库地址

        private int isNet = 0;//是否联网 0否 1是

        private int sdn_iDay = DateTime.Now.DayOfYear;//一年中的第几天

        private string strServerIp = "127.0.0.1";//本机Ip

        //   private Dictionary<string, string> dicServerInfo = new Dictionary<string, string>(); //服务器备案信息

        DataTable dtServerInfo = null;//服务器备案信息

        private QueueList queuelist_BC;//补传取票记录
        httpServer.sdnHttpServer httpserver = null;//全局http服务

        private Dictionary<string, string> dicPause = new Dictionary<string, string>(); //暂停字典表 IP与窗口号

        /// <summary>
        /// redis帮助类
        /// </summary>
        RedisStackExchangeHelper _redis = null;//实例化redis帮助类
        #endregion

        /// <summary>
        /// 窗体构造函数
        /// </summary>
        public sdnMainForm()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
            this.sdnWebBrowser.ObjectForScripting = this;
            sdnQueList = new QueueList(); //实例化排队队列
            sdnQueList_YY = new QueueList();//网上预约排队队列
            queuelist_BC = new QueueList();//补传取票信息队列表
            sdnQueList_YQ = new QueueList();//园区其他排队
            dicNoTimes = new Dictionary<string, int>();//证件号与取票次数字典
            strExePath = AppDomain.CurrentDomain.BaseDirectory; //得到exe程序所在位置
            //设置初始化时，程序的显示位置
            //try
            //{
            //    if (System.Windows.Forms.Screen.AllScreens.Count() > 1)
            //    {
            //        if (System.Windows.Forms.Screen.AllScreens[1].Primary == false)
            //        {
            //            this.Left = System.Windows.Forms.Screen.AllScreens[1].Bounds.Left;
            //            this.Top = System.Windows.Forms.Screen.AllScreens[1].Bounds.Top;
            //            this.Width = System.Windows.Forms.Screen.AllScreens[1].Bounds.Width;
            //            this.Height = System.Windows.Forms.Screen.AllScreens[1].Bounds.Height;
            //        }
            //        else
            //        {
            //            this.Left = System.Windows.Forms.Screen.AllScreens[0].Bounds.Left;
            //            this.Top = System.Windows.Forms.Screen.AllScreens[0].Bounds.Top;
            //            this.Width = System.Windows.Forms.Screen.AllScreens[0].Bounds.Width;
            //            this.Height = System.Windows.Forms.Screen.AllScreens[0].Bounds.Height;
            //        }
            //    }
            //}
            //catch
            //{ }
        }


        #region 窗体事件
        /// <summary>
        /// load事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sdnMainForm_Load(object sender, EventArgs e)
        {

            // string strContent = "{\"count\":0,\"done\":[{\"que_no\":\"1005\",\"win_no\":\"5\"},{\"que_no\":\"1004\",\"win_no\":\"5\"}],\"wait\":\"16,17\"}";
            //string strContent = "{\"count\":12,\"done\":[{\"que_no\":\"1005\",\"win_no\":\"1\"},{\"que_no\":\"1004\",\"win_no\":\"1\"},{\"que_no\":\"1003\",\"win_no\":\"1\"},{\"que_no\":\"1002\",\"win_no\":\"1\"},{\"que_no\":\"1001\",\"win_no\":\"1\"}],\"wait\":\"1006,1007,1008,1009\"}";
            //LED_Util.zhonghe.showMsg_zh.sendMsg2Screen(strContent);

            sdnReadIniFile();//从本地配置文件中读取信息
            sdnCheckUpdate();//检测是否自动更新
            //1 第一次加载的时候把数据库中当天数据加载到队列中
            //注意，这里访问sqlserver数据库 要先保证本地数据库已经正常启动
            //InitDBQueue(); ;//初始化数据库队列
            new Thread(InitDBQueue).Start();//初始化数据库队列
            initSocketServer();//初始化套接字
            initHttpServer();//初始化http服务
            //   initHttpServer();//初始化http服务
            switch (strDS)
            {
                case "0"://不显示
                case "2"://电视盒子
                    break;
                case "1"://一机双屏
                    //ShowForm sdnShow = new ShowForm();
                    //sdnShow.Show();//排队信息显示屏幕打开
                    break;
                default: //其他情况
                    break;
            }
            if (i_showType == 8)//园区特殊屏
            {
                new Thread(ZongHeShowMsg.ZongheShow.CLEDSender.sendData2LEDYQCGS).Start();//综合屏显示
                strHtmlPath = strExePath + @"sdnWeb\HomePage_YQ.html"; //url路径
            }
            else
            {
                strHtmlPath = strExePath + @"sdnWeb\HomePage.html"; //url路径
            }
            Uri sdnUrl = new Uri(strHtmlPath);
            sdnWebBrowser.Url = sdnUrl;
            sdnWebBrowser.ObjectForScripting = this;

            new Thread(sdnClearQueue).Start();//隔天自动清空队列

            //sdnGetShowQueNum();
            //inputMsgQueue("F000214", "C");

            //  this.sdnWebBrowser.Visible = false;//浏览器不可见
            //  this.lbShowMsg.Visible = true;//显示信息可见

            iniBAinfo();//获取备案信息

            try
            {
                _redis = new RedisStackExchangeHelper(); //实例化redis帮助类
            }
            catch { }

        }
        /// <summary>
        /// 窗口关闭事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void sdnMainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //1.初始化LED屏幕

            //2.系统完全退出
            Environment.Exit(-1);//全部退出

        }
        /// <summary>
        /// 点击退出按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_exit_Click(object sender, EventArgs e)
        {
            ExitPassword form = null;
            if (form == null)
            {
                form = new ExitPassword();
            }
            if (form.ShowDialog() == DialogResult.Yes)
            {
                this.Close();
            }
        }

        #endregion

        #region 叫号服务与响应
        /// <summary>
        /// 初始化Socket服务
        /// </summary>
        private void initSocketServer()
        {
            string strPath = AppDomain.CurrentDomain.BaseDirectory + @"configs\sdnSystem.ini";
            if (!string.IsNullOrEmpty(strPath))
            {
                operConfig.ReadIniFile readIni = new operConfig.ReadIniFile(strPath);
                string strIp = readIni.ReadValue("ServerIp", "Ip");//服务IP
                strServerIp = strIp;
                int iPort = Convert.ToInt32(readIni.ReadValue("ServerPort", "Port"));//服务端口
                TcpSocketServer tcpserver = new TcpSocketServer(strIp, iPort, (int)i_showType);
                tcpserver.strBBDM = strBBDM;//部门编码
                tcpserver.eventGetQueueItem += GetQueueItem;//叫号
                tcpserver.eventGetQueueCount += GetQueueCount;//获取排队总数
                tcpserver.eventUpdateQueue += UpdateQueueItemState;//更新队列状态
                tcpserver.eventRemoveQueueItem += RemoveQueueItem;//移除 调号用
                tcpserver.eventIsPause += IsPause;
                tcpserver.eventGetMachineNum += getMachineSerialNum;//得到序列号
                tcpserver.eventGetQueueItemwuli += GetQueueItemwuli;
                tcpserver.eventFindQueueByIP += GetQueueByIp;//根据IP得到对应的队列
                tcpserver.eventGetQueueItemwuliRe += GetQueueItemwuliRe;
                //  tcpserver.strQHXLH =getMachineSerialNum();//得到16位取票序列号
                new Thread(tcpserver.Start).Start();
                if (i_showType == 8)
                    tcpserver.InitComPort();
            }
        }
        /// <summary>
        /// 初始化 httpserver服务
        /// </summary>
        private void initHttpServer()
        {
            string strPath = AppDomain.CurrentDomain.BaseDirectory + @"configs\sdnSystem.ini";
            if (!string.IsNullOrEmpty(strPath)) //httpserver
            {
                operConfig.ReadIniFile readIni = new operConfig.ReadIniFile(strPath);
                string strIp = readIni.ReadValue("ServerIp", "Ip");//服务IP
                int iPort = Convert.ToInt32(readIni.ReadValue("httpserver", "port"));//服务端口
                httpserver = new httpServer.sdnHttpServer(strIp, iPort, (int)i_showType);
                httpserver.strBBDM = strBBDM;//部门编码
                httpserver.strJH = strJH;//经办人警号
                httpserver.eventGetItemByKey += GetQueueItemRe;//根据key得到queueItem
                httpserver.eventGetQueueCount += GetQueueCount;//得到排队总数
                httpserver.eventGetQueueItem += GetQueueItem;//叫号
                httpserver.eventGetWinNum += getWinNum;//根据24位排队流水号得到窗口号
                httpserver.eventIsPause += IsPause; //暂停
                httpserver.eventRemoveQueueItem += RemoveQueueItem;//移除 调号用
                httpserver.eventUpdateQueue += UpdateQueueItemState;//更新队列状态
                httpserver.eventAddBCqueue += AddBCqueue;//新增补传取票队列
                httpserver.eventFindQueueByIP += GetQueueByIp;//根据IP得到对应的队列
                httpserver.eventCheckPauseState += isPause;//检测暂停状态
                httpserver.eventControlCall += sdnControlCall;//检测
                httpserver.eventGoBack += sdnGoBack;//回滚
                httpserver.event_pub_msg += pub_msg_event;//发布信息

                new Thread(httpserver.listen).Start();
                Log.WriteOptDisk("初始化httpserver服务");
            }

        }

        #region TcpSocketServer类调用的函数
        /// <summary>
        /// 重叫获取队列中的最先一笔
        /// 按正常情况下，重叫应该无需再次读取队列
        /// </summary>
        /// <param name="strKey"></param>
        /// <returns></returns>
        private QueueItem GetQueueItemRe(string strKey)
        {
            try
            {
                if (strKey.Contains("C"))
                {
                    QueueItem item = sdnQueList_YY.Find(strKey);
                    if (item != null)
                        return item;
                }

                return sdnQueList.Find(strKey);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取队列中的最先一笔，网上预约最优先，TCP服务使用
        /// </summary>
        /// <returns></returns>
        private QueueItem GetQueueItem()
        {
            try
            {
                //1 先排队预约队列内是否有值
                if (sdnQueList_YY.Count > 0) //预约队列内有值
                {
                    //获取预约排队队列最新一笔
                    QueueItem item = sdnQueList_YY.GetTopOutQueue();
                    if (item != null) //如果叫号有值
                    {
                        iDealCount++;//处理数量加1
                        return item;
                    }
                    else //如果预约队列中没有待叫号的值
                    {//从普通队列中取值
                        QueueItem item2 = sdnQueList.GetTopOutQueue();
                        if (item2 != null) //如果叫号有值
                        {
                            iDealCount++;//处理数量加1
                        }
                        return item2;
                    }

                }
                else //没有预约信息
                {
                    //获取正常排队队列最新一笔
                    QueueItem item = sdnQueList.GetTopOutQueue();
                    if (item != null) //如果叫号有值
                    {
                        iDealCount++;//处理数量加1
                    }
                    return item;
                }


            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取物理叫号第一条数据
        /// </summary>
        /// <returns></returns>
        private QueueItem GetQueueItemwuli(string winnum)
        {
            QueueItem item = sdnQueList_YQ.GetTopOutQueue(winnum);
            if (item != null) //如果叫号有值
            {
                iDealCount++;//处理数量加1
            }
            return item;
        }

        private QueueItem GetQueueItemwuliRe(string winnum)
        {
            try
            {
                QueueItem item = sdnQueList_YQ.FindByWinnum(winnum);

                return item;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取队列中的最先一笔，网上预约最优先，HTTP服务使用
        /// </summary>
        /// <returns></returns>
        private QueueItem GetQueueItem(string strWinIp, string strWinNum)
        {
            try
            {
                //1 先排队预约队列内是否有值
                if (sdnQueList_YY.Count > 0) //预约队列内有值
                {
                    //获取预约排队队列最新一笔
                    QueueItem item = sdnQueList_YY.GetTopOutQueue(strWinIp, strWinNum);
                    if (item != null) //如果叫号有值
                    {
                        iDealCount++;//处理数量加1
                        return item;
                    }
                    else //如果预约队列中没有待叫号的值
                    {//从普通队列中取值
                        QueueItem item2 = sdnQueList.GetTopOutQueue(strWinIp, strWinNum);
                        if (item2 != null) //如果叫号有值
                        {
                            iDealCount++;//处理数量加1
                        }
                        return item2;
                    }

                }
                else //没有预约信息
                {
                    //获取正常排队队列最新一笔
                    QueueItem item = sdnQueList.GetTopOutQueue(strWinIp, strWinNum);
                    if (item != null) //如果叫号有值
                    {
                        iDealCount++;//处理数量加1
                    }
                    return item;
                }


            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 获取总排队人数
        /// </summary>
        /// <returns></returns>
        private string GetQueueCount()
        {
            try
            {
                if (sdnQueList_YY != null && sdnQueList != null && sdnQueList_YQ != null)// && sdnQueList.Count != 0)
                {
                    //状态为0的总数
                    int yycount = sdnQueList_YY.GetDealQueue()[2];
                    int count = sdnQueList.GetDealQueue()[2];
                    int yqcount = sdnQueList_YQ.GetDealQueue()[2];
                    //return (sdnQueList_YY.Count + (sdnQueList.Count - 1)).ToString();
                    return (yycount + (count) + yqcount).ToString();
                }
                else
                {
                    return "0";
                }
            }
            catch
            {
                return "0";
            }
        }
        /// <summary>
        /// 更新某个键的状态
        /// </summary>
        /// <param name="strKey"></param>
        /// <param name="iState"></param>
        private void UpdateQueueItemState(string strKey, int iState)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(strKey))
                {
                    //  if (strKey.Contains("A")) //如果为正常叫号
                    if (strKey.Substring(20,1).Equals("1")) //如果为正常叫号
                    {
                        sdnQueList.UpdateQueue(strKey, iState);
                    }
                    else if (strKey.Contains("D")) //如果为正常叫号
                  //  else if (strKey.Substring(19, 1).Equals("1")) //如果为正常叫号
                    {
                        sdnQueList_YQ.UpdateQueue(strKey, iState);
                    }
                    else
                    {
                        sdnQueList_YY.UpdateQueue(strKey, iState);
                    }
                    if (isNet == 1) //判定是否联网（是否具有远程数据库）
                    {
                        UpdateQueueState(strKey.Substring(18, 4), iState);//更新远程数据库状态,截取到对应的叫号
                    }
                }

            }
            catch
            { }
        }

        /// <summary>
        /// 移除一笔QueueItem
        /// </summary>
        private void RemoveQueueItem(string strKey)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(strKey))
                {
                    //  if (strKey.Contains("A")) //如果包含A 则为正常排队队列
                    if (strKey.Substring(20, 1).Equals("1")) //如果包含A 则为正常排队队列
                    {
                        //移除队列里数据
                        sdnQueList.Remove(strKey);
                    }
                    else if (strKey.Contains("D")) //如果包含A 则为正常排队队列
                    {
                        //移除队列里数据
                        sdnQueList_YQ.Remove(strKey);
                    }
                    else
                    {
                        sdnQueList_YY.Remove(strKey);//从预约队列中移除数据
                    }
                }

            }
            catch
            { }
        }

        /// <summary>
        /// 设置主窗口取号功能是否可用
        /// </summary>
        /// <param name="strCmd">pause-暂停 不可用 restart-恢复 可用</param>
        private void IsPause(string strCmd)
        {
            try
            {
                if (strCmd == "pause") //如果是暂停命令
                {
                    // this.sdnWebBrowser = "WarningInfo.html";
                    // strHtmlPath = strExePath + @"sdnWeb\HomePage.html"; //url路径
                    strHtmlPath = strExePath + @"sdnWeb\WarningInfo.html"; //url路径
                    Uri sdnUrl = new Uri(strHtmlPath);
                    sdnWebBrowser.Url = sdnUrl;
                    sdnWebBrowser.ObjectForScripting = this;
                }
                else //如果不是暂停命令 即恢复暂停
                {
                    if (i_showType == 8)
                    {
                        strHtmlPath = strExePath + @"sdnWeb\HomePage_YQ.html"; //url路径
                    }
                    else
                    {
                        strHtmlPath = strExePath + @"sdnWeb\HomePage.html"; //url路径
                    }
                    //  strHtmlPath = strExePath + @"sdnWeb\WarningInfo.html"; //url路径
                    Uri sdnUrl = new Uri(strHtmlPath);
                    sdnWebBrowser.Url = sdnUrl;
                    sdnWebBrowser.ObjectForScripting = this;
                }
            }
            catch (Exception ex)
            { }
        }
        /// <summary>
        /// 根据ip查找排队信息
        /// </summary>
        /// <param name="_strWinIp"></param>
        private QueueItem GetQueueByIp(string _strWinIp)
        {
            try
            {
                //1 先排队预约队列内是否有值
                if (sdnQueList_YY.Count > 0) //预约队列内有值
                {
                    //获取预约排队队列最新一笔
                    QueueItem item = sdnQueList_YY.FindByIp(_strWinIp);
                    if (item != null) //如果叫号有值
                    {
                        return item;
                    }
                    else
                    {
                        return sdnQueList.FindByIp(_strWinIp);
                    }

                }
                else //没有预约信息
                {
                    //获取正常排队队列最新一笔
                    QueueItem item = sdnQueList.FindByIp(_strWinIp);

                    return item;
                }
            }
            catch
            {
                return null;
            }
        }

        #endregion



        #endregion

        #region 从配置项中读取系统运行基本信息

        private void sdnReadIniFile()
        {
            try
            {
                operConfig.ReadIniFile rnd = new operConfig.ReadIniFile(AppDomain.CurrentDomain.BaseDirectory + "configs\\sdnSystem.ini");
                strBBDM = rnd.ReadValue("BMDM", "value");//从配置文件中读取部门代码
                strJH = rnd.ReadValue("BMDM", "jbr");//经办人警号
                string systemid = rnd.ReadValue("systemid", "value");
                strDS = rnd.ReadValue("systemid", "ds");//是否一机双屏显示
                if (!string.IsNullOrWhiteSpace(systemid))
                {
                    i_showType = Int32.Parse(systemid);//;系统种类 0：无显示 1:双屏  2：电视盒子  3：综合屏  4：综合屏+条屏  5：LED条屏1  6：LED条屏2
                }
                iMaxCount = Convert.ToInt32(rnd.ReadValue("Counts", "counts"));
                strMachine = rnd.ReadValue("machine", "value");//唯一机器码
                //   strBMDM = rnd.ReadValue("BMDM", "value");//部门编码
                strAdress = rnd.ReadValue("Address", "address");//取票网点地址
                strUrl = rnd.ReadValue("htmladdress", "value");//取票操作界面地址
                isNet = string.IsNullOrWhiteSpace(rnd.ReadValue("isnet", "value")) ? 0 : Convert.ToInt32(rnd.ReadValue("isnet", "value").Trim());//是否联网，远程数据库
                operConfig.ReadIniFile rnd1 = new operConfig.ReadIniFile(AppDomain.CurrentDomain.BaseDirectory + "configs\\web.ini");
                isWebShow = rnd1.ReadValue("isWebShow", "value"); //服务器IP 如苏州206



            }
            catch (Exception ex)
            {
                MessageBox.Show("读取系统config配置文件失败===" + ex.Message);
            }
        }

        #endregion

        #region html 页面js与cs 交互
        /// <summary>
        /// 读二代卡取票 （用于js函数调用）
        /// 境内取票A
        /// </summary>
        /// <returns></returns>
        public string readCardQueue(string _strQueType)
        {

            //1 弹出读卡界面
            //2 读卡
            //3 获取读卡信息

            string strRes = "";
            string strMsg = "";
            string strQueueNo = "";
            string strSerialNum = "";//24位排队序列号
            IDCard sdnIdCard = new IDCard();//身份证信息类 
            IdentityCard identity = new IdentityCard();
            sdnIdCard = identity.GetBaseMsg(1, out strMsg);
            if (sdnIdCard != null && sdnIdCard.CartNo != "")
            {
                if (dicNoTimes.ContainsKey(sdnIdCard.CartNo) && dicNoTimes[sdnIdCard.CartNo] >= iMaxCount)
                {
                    strRes = string.Format("{{\"flag\":\"{0}\",\"cardno\":\"{1}\",\"msg\":\"{2}\"}}", "false", "err", "该证件取票过于频繁");
                    return strRes;
                }
                if (sdnQueList.List.Count > 0)
                {
                    QueueItem lastqueue = sdnQueList.List.Values[sdnQueList.List.Count - 1]; //上个队列数据
                    if (lastqueue.msgCardNo == sdnIdCard.CartNo)
                    {
                        strRes = string.Format("{{\"flag\":\"{0}\",\"cardno\":\"{1}\",\"msg\":\"{2}\"}}", "false", "err", "该证件取票过于频繁");
                        return strRes;
                    }
                }
                else if (dicNoTimes.ContainsKey(sdnIdCard.CartNo)) //包含相应的键值对
                {
                    dicNoTimes[sdnIdCard.CartNo]++;

                }
                else
                {
                    dicNoTimes.Add(sdnIdCard.CartNo, 1);
                }
                //4 添加队列
                QueueItem sdnQueueItem = new QueueItem();
                sdnQueueItem.msgCardNo = sdnIdCard.CartNo;//身份证号
                sdnQueueItem.msgName = sdnIdCard.Name;
                strQueueNo = GetNewQueNo(_strQueType);
                strSerialNum = produceSerialNum(strQueueNo); //得到24位编码序列号
                sdnQueueItem.msgQueueNo = strQueueNo;
                sdnQueueItem.serialNum = strSerialNum;
                sdnQueueItem.msgState = 0;//初始状态为0
                sdnQueueItem.XH = "";//序号
                sdnQueueItem.mWay = "1";//正常读卡
                sdnQueueItem.strqhsj = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                sdnQueList.Add(strSerialNum, sdnQueueItem);//添加排队数据到正常排队队列中
                //5 添加数据库
                switch (_strQueType)
                {
                    case "1":
                        sdnAddLocalDB(sdnIdCard, strQueueNo, _strQueType, iMaxA, strSerialNum); //插入数据到本地数据库
                        sdnAddCardMsg(sdnIdCard, _strQueType, strSerialNum);//插入身份证信息到本地库
                        break;
                    case "B":
                        sdnAddLocalDB(sdnIdCard, strQueueNo, _strQueType, iMaxB, strSerialNum); //插入数据到本地数据库
                        sdnAddCardMsg(sdnIdCard, _strQueType, strSerialNum);//插入身份证信息到本地库
                        break;
                    case "C":
                        sdnAddLocalDB(sdnIdCard, strQueueNo, _strQueType, iMaxC, strSerialNum); //插入数据到本地数据库
                        sdnAddCardMsg(sdnIdCard, _strQueType, strSerialNum);//插入身份证信息到本地库
                        break;
                    default:
                        sdnAddLocalDB(sdnIdCard, strQueueNo, _strQueType, iMaxA, strSerialNum); //插入数据到本地数据库
                        sdnAddCardMsg(sdnIdCard, _strQueType, strSerialNum);//插入身份证信息到本地库
                        break;
                }

                //6 插入远程数据库  远程数据库 什么时候插入根据web网页存放的位置有关
                if (isNet == 1)
                {
                    AddQueueData(sdnQueueItem, _strQueType, 1); //异步调用API插入远程数据库
                }
                sdn_CardNo = sdnIdCard.CartNo; //给程序赋值全局变量——身份证号
                // MessageBox.Show(strAdress);
                // strAdress = System.Web.HttpUtility.UrlEncode(strAdress,Encoding.UTF8);
                string strQueMsg = string.Format("[{{ \"nonum\":\"{0}\",\"cardno\":\"{1}\",\"name\":\"{2}\",\"queno\":\"{3}\",\"address\":\"{4}\"}}]", (iAllCount - iDealCount) + "", sdnIdCard.CartNo, sdnIdCard.Name, strQueueNo, strAdress);
                strRes = string.Format("{{\"flag\":\"{0}\",\"cardno\":\"{1}\",\"msg\":{2}}}", "true", sdnIdCard.CartNo, strQueMsg);
            }
            else
            {

                strRes = string.Format("{{\"flag\":\"{0}\",\"cardno\":\"{1}\",\"msg\":\"{2}\"}}", "false", "1", "读卡失败");
                return strRes;
            }
            iAllCount++; //排队总数增加
            //sdnQuePrintJS((iAllCount - iDealCount) + "", sdnIdCard.CartNo, sdnIdCard.Name, strQueueNo, strBBDM);
            return strRes;
        }
        /// <summary>
        /// 输入信息取票 （用于js函数调用）
        /// 手动输入取票信息 类型 B
        /// </summary>
        /// <returns></returns>
        public string inputMsgQueue(string strCardNo, string _strQueType)
        {
            string strRes = "";
            if ((strCardNo.Length == 15 || strCardNo.Length == 18) && !strCardNo.ToUpper().Contains("F"))
            {
                strRes = string.Format("{{\"flag\":\"{0}\",\"cardno\":\"{1}\",\"msg\":\"{2}\"}}", "false", "1", "输入的证件号码不正确！");
                return strRes;
            }
            string strQueueNo = "";
            string strSerialNum = "";//24位排队序列号
            IDCard sdnIdCard = new IDCard();//身份证信息类 
            if (!string.IsNullOrWhiteSpace(strCardNo))
            {

                sdnIdCard.CartNo = strCardNo;
                sdnIdCard.Name = "手工输入";
                if (sdnQueList.List.Count > 0)
                {
                    QueueItem lastqueue = sdnQueList.List.Values[sdnQueList.List.Count - 1]; //上个队列数据
                    if (lastqueue.msgCardNo == sdnIdCard.CartNo)
                    {
                        strRes = string.Format("{{\"flag\":\"{0}\",\"cardno\":\"{1}\",\"msg\":\"{2}\"}}", "false", "1", "该证件取票过于频繁");
                        return strRes;
                    }
                }
                if (dicNoTimes.ContainsKey(sdnIdCard.CartNo) && dicNoTimes[sdnIdCard.CartNo] >= iMaxCount)
                {
                    strRes = string.Format("{{\"flag\":\"{0}\",\"cardno\":\"{1}\",\"msg\":\"{2}\"}}", "false", "1", "该证件取票过于频繁");
                    return strRes;
                }
                else if (dicNoTimes.ContainsKey(sdnIdCard.CartNo)) //包含相应的键值对
                {
                    dicNoTimes[sdnIdCard.CartNo]++;

                }
                else
                {
                    dicNoTimes.Add(sdnIdCard.CartNo, 1);
                }
                //4 添加队列
                QueueItem sdnQueueItem = new QueueItem();
                sdnQueueItem.msgCardNo = strCardNo;//身份证号
                sdnQueueItem.msgName = "手工输入";
                strQueueNo = GetNewQueNo(_strQueType);
                strSerialNum = produceSerialNum(strQueueNo); //得到24位编码序列号
                sdnQueueItem.serialNum = strSerialNum;
                sdnQueueItem.msgQueueNo = strQueueNo;
                sdnQueueItem.msgState = 0;//初始状态为0
                sdnQueueItem.XH = "";//序号
                sdnQueueItem.mWay = "2";//手工输入
                sdnQueueItem.strqhsj = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                sdnQueList.Add(strSerialNum, sdnQueueItem);//添加排队数据到正常排队队列中
                //5 添加数据库
                //sdnAddLocalDB(sdnIdCard, strQueueNo, _strQueType, iMaxA); //插入数据到本地数据库
                switch (_strQueType)
                {
                    case "1":
                        sdnAddLocalDB(sdnIdCard, strQueueNo, _strQueType, iMaxA, strSerialNum); //插入数据到本地数据库
                        sdnAddCardMsg(sdnIdCard, _strQueType, strSerialNum);//插入身份证信息到本地库
                        break;
                    case "B":
                        sdnAddLocalDB(sdnIdCard, strQueueNo, _strQueType, iMaxB, strSerialNum); //插入数据到本地数据库
                        sdnAddCardMsg(sdnIdCard, _strQueType, strSerialNum);//插入身份证信息到本地库
                        break;
                    case "C":
                        sdnAddLocalDB(sdnIdCard, strQueueNo, _strQueType, iMaxC, strSerialNum); //插入数据到本地数据库
                        sdnAddCardMsg(sdnIdCard, _strQueType, strSerialNum);//插入身份证信息到本地库
                        break;
                    default:
                        sdnAddLocalDB(sdnIdCard, strQueueNo, _strQueType, iMaxA, strSerialNum); //插入数据到本地数据库
                        sdnAddCardMsg(sdnIdCard, _strQueType, strSerialNum);//插入身份证信息到本地库
                        break;
                }
                //6 插入远程数据库  远程数据库 什么时候插入根据web网页存放的位置有关
                if (isNet == 1)
                {
                    AddQueueData(sdnQueueItem, _strQueType, 2); //异步调用API插入远程数据库
                }

                sdn_CardNo = sdnIdCard.CartNo; //给程序赋值全局变量——身份证号
                //    strAdress = System.Web.HttpUtility.UrlEncode(strAdress);
                string strQueMsg = string.Format("[{{ \"nonum\":\"{0}\",\"cardno\":\"{1}\",\"name\":\"{2}\",\"queno\":\"{3}\",\"address\":\"{4}\"}}]", (iAllCount - iDealCount) + "", sdnIdCard.CartNo, "手工输入", strQueueNo, strAdress);
                strRes = string.Format("{{\"flag\":\"{0}\",\"cardno\":\"{1}\",\"msg\":{2}}}", "true", sdnIdCard.CartNo, strQueMsg);
            }
            else
            {
                strRes = string.Format("{{\"flag\":\"{0}\",\"cardno\":\"{1}\",\"msg\":\"{2}\"}}", "false", "1", "输入证件号错误");
                return strRes;
            }
            iAllCount++; //排队总数增加

            //   sdnQuePrintJS((iAllCount - iDealCount) + "", sdnIdCard.CartNo, "手工输入", strQueueNo, strBBDM);
            return strRes;
        }
        /// <summary>
        /// 读取二代身份证信息
        /// </summary>
        /// <returns></returns>
        public string sdnReadCard()
        {
            string strRes = "";
            string strMsg = "";
            IDCard sdnIdCard = new IDCard();//身份证信息类 
            IdentityCard identity = new IdentityCard();
            sdnIdCard = identity.GetBaseMsg(1, out strMsg);
            if (sdnIdCard != null && sdnIdCard.CartNo != "")
            {
                strRes = string.Format("{{\"flag\":\"{0}\",\"cardno\":\"{1}\",\"msg\":\"{2}\"}}", "true", sdnIdCard.CartNo, "读卡成功");
                return strRes;
            }
            else
            {
                strRes = string.Format("{{\"flag\":\"{0}\",\"cardno\":\"{1}\",\"msg\":\"{2}\"}}", "false", "1", "读卡失败,请重写读取或手工输入证件号！");
                return strRes;
            }
        }

        /// <summary>
        /// 预约排队  （用于js函数调用）
        /// </summary>
        /// <param name="sdnType">操作类型（读卡 1/手工输入 2）</param>
        /// <returns></returns>
        public string YYMsgQueue(string sdnType, string strCardNo, string _strQueType, string XH)
        {
            string strRes = "";
            string strMsg = "";
            string strQueueNo = "";
            string strSerialNum = "";//24位编码序列号
            IDCard sdnIdCard = new IDCard();//身份证信息类 

            if (sdnType == "1") //如果是读取身份证
            {
                IdentityCard identity = new IdentityCard();
                sdnIdCard = identity.GetBaseMsg(1, out strMsg);
            }
            else //手工输入
            {
                if (!string.IsNullOrWhiteSpace(strCardNo))
                {

                    sdnIdCard.CartNo = strCardNo;
                    sdnIdCard.Name = "手工输入";
                }
                else
                {
                    strRes = string.Format("{{\"flag\":\"{0}\",\"cardno\":\"{1}\",\"msg\":\"{2}\"}}", "false", "1", "输入证件号错误");
                    return strRes;
                }
            }
            if (sdnIdCard != null && !string.IsNullOrWhiteSpace(sdnIdCard.CartNo))
            {
                if (sdnQueList_YY.List.Count > 0)
                {
                    QueueItem lastqueue = sdnQueList_YY.List.Values[sdnQueList_YY.List.Count - 1]; //上个队列数据
                    if (lastqueue.msgCardNo == sdnIdCard.CartNo)
                    {
                        strRes = string.Format("{{\"flag\":\"{0}\",\"cardno\":\"{1}\",\"msg\":\"{2}\"}}", "false", "1", "该证件取票过于频繁");
                        return strRes;
                    }
                }
                if (dicNoTimes.ContainsKey(sdnIdCard.CartNo) && dicNoTimes[sdnIdCard.CartNo] >= iMaxCount)
                {
                    strRes = string.Format("{{\"flag\":\"{0}\",\"cardno\":\"{1}\",\"msg\":\"{2}\"}}", "false", "1", "该证件取票过于频繁");
                    return strRes;
                }
                else if (dicNoTimes.ContainsKey(sdnIdCard.CartNo)) //包含相应的键值对
                {
                    dicNoTimes[sdnIdCard.CartNo]++;

                }
                else
                {
                    dicNoTimes.Add(sdnIdCard.CartNo, 1);
                }
                //4 添加队列
                QueueItem sdnQueueItem = new QueueItem();
                sdnQueueItem.msgCardNo = strCardNo;//身份证号
                sdnQueueItem.msgName = sdnIdCard.Name;
                strQueueNo = GetNewQueNo(_strQueType);
                strSerialNum = produceSerialNum(strQueueNo); //得到24位编码序列号
                sdnQueueItem.msgQueueNo = strQueueNo;
                sdnQueueItem.serialNum = strSerialNum;
                sdnQueueItem.msgState = 0;//初始状态为0
                sdnQueueItem.XH = XH;//序号
                sdnQueueItem.mWay = "4";//正常读卡
                sdnQueueItem.strqhsj = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                sdnQueList_YY.Add(strSerialNum, sdnQueueItem);//添加排队数据到正常排队队列中
                //5 添加数据库
                //  sdnAddLocalDB(sdnIdCard, strQueueNo, _strQueType, iMaxA); //插入数据到本地数据库
                switch (_strQueType)
                {
                    case "1":
                        sdnAddLocalDB(sdnIdCard, strQueueNo, _strQueType, iMaxA, strSerialNum); //插入数据到本地数据库
                        sdnAddCardMsg(sdnIdCard, _strQueType, strSerialNum);//插入身份证信息到本地库
                        break;
                    case "B":
                        sdnAddLocalDB(sdnIdCard, strQueueNo, _strQueType, iMaxB, strSerialNum); //插入数据到本地数据库
                        sdnAddCardMsg(sdnIdCard, _strQueType, strSerialNum);//插入身份证信息到本地库
                        break;
                    case "C":
                        sdnAddLocalDB(sdnIdCard, strQueueNo, _strQueType, iMaxC, strSerialNum); //插入数据到本地数据库
                        sdnAddCardMsg(sdnIdCard, _strQueType, strSerialNum);//插入身份证信息到本地库
                        break;
                    default:
                        sdnAddLocalDB(sdnIdCard, strQueueNo, _strQueType, iMaxA, strSerialNum); //插入数据到本地数据库
                        sdnAddCardMsg(sdnIdCard, _strQueType, strSerialNum);//插入身份证信息到本地库
                        break;
                }
                //6 插入远程数据库  远程数据库 什么时候插入根据web网页存放的位置有关
                if (isNet == 1)
                {
                    AddQueueData(sdnQueueItem, _strQueType, 4); //异步调用API插入远程数据库
                }

                sdn_CardNo = sdnIdCard.CartNo; //给程序赋值全局变量——身份证号
                // strAdress = System.Web.HttpUtility.UrlEncode(strAdress);
                string strQueMsg = string.Format("[{{ \"nonum\":\"{0}\",\"cardno\":\"{1}\",\"name\":\"{2}\",\"queno\":\"{3}\",\"address\":\"{4}\"}}]", (iAllCount - iDealCount) + "", sdnIdCard.CartNo, sdnIdCard.Name, strQueueNo, strAdress);
                strRes = string.Format("{{\"flag\":\"{0}\",\"cardno\":\"{1}\",\"msg\":{2}}}", "true", sdnIdCard.CartNo, strQueMsg);
            }
            else
            {
                strRes = string.Format("{{\"flag\":\"{0}\",\"cardno\":\"{1}\",\"msg\":\"{2}\"}}", "false", "1", "输入证件号错误");
                return strRes;
            }
            iAllCount++; //排队总数增加

            //   sdnQuePrintJS((iAllCount - iDealCount) + "", sdnIdCard.CartNo, "手工输入", strQueueNo, strBBDM);
            return strRes;

        }
        /// <summary>
        /// 获取排队情况
        /// </summary>
        /// <returns></returns>
        public string sdnGetShowQueNum()
        {
            //return "2222";
            // MessageBox.Show(string.Format("{\"quenum\";\"{0}\",\"donum\":\"{1}\",\"nonum\":\"{2}\"}", iAllCount, iDealCount, iAllCount - iDealCount));
            string strQueNum = string.Format("{{\"quenum\":\"{0}\",\"donum\":\"{1}\",\"nonum\":\"{2}\"}}", iAllCount + "", iDealCount + "", (iAllCount - iDealCount) + "");

            return strQueNum;
        }
        /// <summary>
        /// 得到部门代码
        /// </summary>
        /// <returns></returns>
        public string sdnGetBMDM()
        {
            return this.strBBDM;//得到部门代码
        }
        /// <summary>
        /// 得到机器唯一码（或MAC)
        /// </summary>
        /// <returns></returns>
        public string sdnGetMac()
        {
            return strMachine;
        }
        /// <summary>
        /// 得到远程URL
        /// </summary>
        /// <returns></returns>
        public string sdnGetHtmlURL()
        {
            if (string.IsNullOrWhiteSpace(strUrl)) //如果为空，配置文件没有配置
            {
                return AppDomain.CurrentDomain.BaseDirectory + @"sdnWeb\";
                return strHtmlPath;
            }
            return strUrl;
        }

        private void sdnQuePrintJS(string nonum, string cardno, string name, string queno, string address)
        {
            // MessageBox.Show(address);
            sdnWebBrowser.Document.InvokeScript("GetForQuePrint", new string[] { nonum, cardno, name, queno, address });
        }

        /// <summary>
        /// 显示排队队列号码 (此函数调用js页面的 js函数 sdnShowQueNum
        /// </summary>
        /// queunum 排队总数
        /// donum   已经办理总数
        /// nonum   当前排队人数
        /// <returns></returns>
        private void sdnShowQueNum(int queunum, int donum, int nonum)
        {
            string strQueNum = string.Format("{{\"quenum\";\"{0}\",\"donum\":\"{1}\",\"nonum\":\"{2}\"}}", queunum, donum, nonum);
            sdnWebBrowser.Document.InvokeScript("sdnShowQueNum", new string[] { strQueNum });
        }
        /// <summary>
        /// 暂停取票
        /// <param name="strPause">暂停/开始</param>
        /// </summary>
        private void sdnPauseQueue(string strPause)
        {

        }

        #endregion

        #region 更新远程数据库
        /// <summary>
        /// 添加新的排队数据到远程数据库
        /// </summary>
        /// <param name="queuItem">排队队列</param>
        /// <param name="strQueueType">排队类型</param>
        /// <param name="queu_way">取票途径</param>
        /// <returns></returns>
        private bool AddQueueData(QueueItem queuItem, string strQueueType, int queu_way)
        {
            try
            {
                PDJH_QUEUE_DATA data = new PDJH_QUEUE_DATA();
                data.BUSINESS_TYPE = strQueueType;
                data.NAME = queuItem.msgName;
                data.CARDNO = queuItem.msgCardNo;
                data.QUEUE_NO = Convert.ToInt32(queuItem.msgQueueNo.Substring(1, 3).Trim());
                data.QUEUE_NUM = queuItem.msgQueueNo;
                data.QUEUE_WAY = queu_way;
                data.STATE = 0;
                data.DEPART_CODE = strBBDM;
                data.GET_TIMES = 0;
                data.REMARK = queuItem.XH;//
                string json = JsonHelper.JsonSerializerBySingleData<PDJH_QUEUE_DATA>(data);
                OperQueueData2DB service = new OperQueueData2DB(strBBDM);
                delAddDBbyAPI sdnAddDBQueue = new delAddDBbyAPI(service.AddDBbyAPI); //实例化委托
                IAsyncResult iasynRes = sdnAddDBQueue.BeginInvoke(json, null, null);
                Thread.Sleep(10);

                if (queu_way == 4) //如果为预约取票
                {
                    delUpdateWJWYYDBbyAPI sdnUpdateWJWYY = new delUpdateWJWYYDBbyAPI(service.UpdateWJWYYDBbyAPI);
                    IAsyncResult ir = sdnUpdateWJWYY.BeginInvoke(queuItem.XH, 1, null, null);
                    //service.UpdateWJWYYDBbyAPI(queuItem.XH, 1);//更新预约取票状态为1 成功办理
                }
                // return  service.AddDBbyAPI(json)==1?true:false;
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 更新排队数据状态
        /// </summary>
        /// <param name="strQueueNo">排队号码</param>
        /// <param name="istate">状态</param>
        /// <returns></returns>
        private bool UpdateQueueState(string strQueueNo, int istate)
        {
            OperQueueData2DB service = new OperQueueData2DB(strBBDM);
            delUpdateDBbyAPI sdnUpdateQueueState = new delUpdateDBbyAPI(service.UpdateDBbyAPI);
            sdnUpdateQueueState.BeginInvoke(strQueNo, istate, null, null);
            return true;
        }

        #endregion

        #region 自动更新

        private void sdnCheckUpdate()
        {
            try
            {
                if (AutoUpdateHelper.AutoUpdate.CheckAndUpdate()) //检测是否需要更新
                {
                    Environment.Exit(0);//强制性退出
                }

            }
            catch
            {
                MessageBox.Show("系统网络或者配置项错误,异常代码为：X001002");
            }
        }

        #endregion

        #region 循环读取实时排队信息数据

        private void sdnWhileGetQueNum()
        {
            while (true)
            {
                Thread.Sleep(5000);
                sdnShowQueNum(iAllCount, iDealCount, iAllCount - iDealCount);

            }
        }

        #endregion

        #region 排队号码池
        /// <summary>
        /// 得到排队号码
        /// </summary>
        /// <returns>三位数的号码</returns>
        private string GetNewQueNo(string strQueueType)
        {
            //号码池 当前是 每次取票加1 ,如果考虑到多台取票机的话，这里可以采用号段存储到中间数据库等
            iQueNo++;
            switch (strQueueType)
            {
                case "1":
                    iMaxA++;
                    strQueNo = strQueueType + iMaxA.ToString("000"); //得到
                    return strQueNo;
                    break;
                case "B":
                    iMaxB++;
                    strQueNo = strQueueType + iMaxB.ToString("000"); //得到
                    return strQueNo;
                    break;
                case "C":
                    iMaxC++;
                    strQueNo = strQueueType + iMaxC.ToString("000"); //得到
                    return strQueNo;
                    break;
                case "D":
                    iMaxD++;
                    strQueNo = strQueueType + iMaxD.ToString("000"); //得到
                    return strQueNo;
                    break;
                default:
                    iMaxA++;
                    strQueNo = strQueueType + iMaxA.ToString("000"); //得到
                    return strQueNo;
                    break;
            }

            //   strQueNo = iQueNo.ToString("000");//格式化三位字符串
            //   return strQueNo;
        }
        /// <summary>
        /// 清空排队信息
        /// </summary>
        private void sdnClearQueue()
        {
            while (true)
            {
                try
                {
                    int iCurrDay = DateTime.Now.DayOfYear;//当前为一年中的第几天
                    if (sdn_iDay < iCurrDay)
                    {
                        sdn_iDay = iCurrDay;//
                        sdnQueList.ClearAllQue();
                        sdnQueList = null;
                        sdnQueList = new QueueList();
                        sdnQueList_YY.ClearAllQue();
                        sdnQueList_YY = null;
                        sdnQueList_YY = new QueueList();
                        sdnQueList_YQ.ClearAllQue();
                        sdnQueList_YQ = null;
                        sdnQueList_YQ = new QueueList();
                    }
                }
                catch
                {

                }
                Thread.Sleep(600000); //线程每十分钟执行一次
            }
        }

        #endregion

        #region 操作本地数据库
        /// <summary>
        /// 初始化队列 DB 获取相应的队列信息
        /// </summary>
        private void InitDBQueue()
        {
            try
            {
                // 1 获取当天排队数据
                DateTime dtnow = DateTime.Now;
                DataTable dt = new QueueSys.BLL.T_SYS_QUEUE().GetAllQueueData(dtnow.ToShortDateString());
                //2 分析数据计入队列 区分是否叫号
                if (dt != null && dt.Rows.Count > 0)
                {
                    //获取到 A  B  C 开头的三个queueNO 最大值
                    iMaxA = new QueueSys.BLL.T_QUE_MSG().GetMaxNoList(" left(QUEUE_NO,1)='1'");
                    iMaxB = new QueueSys.BLL.T_QUE_MSG().GetMaxNoList(" left(QUEUE_NO,1)='B'");
                    iMaxC = new QueueSys.BLL.T_QUE_MSG().GetMaxNoList(" left(QUEUE_NO,1)='C'");
                    iMaxD = new QueueSys.BLL.T_QUE_MSG().GetMaxNoList(" left(QUEUE_NO,1)='D'");
                    //MessageBox.Show(iMaxC+"");
                    foreach (DataRow dr in dt.Rows)
                    {
                        iAllCount++; //排队总数增加
                        QueueItem sdnQueItem = new QueueItem();
                        sdnQueItem.msgCardNo = dr["CARDNO"].ToString(); //身份证号
                        sdnQueItem.msgName = dr["NAME"].ToString();//姓名
                        sdnQueItem.msgQueueNo = dr["QUEUE_NUM"].ToString();//队列号码
                        sdnQueItem.serialNum = dr["REMARK"].ToString();//remark 当前24位取票序列号使用
                        sdnQueItem.windowNum = dr["CALL_NUM"].ToString();
                        sdnQueItem.strqhsj = Convert.ToDateTime(dr["FIRST_TIME"]).ToString("yyyy-MM-dd HH:mm:ss");
                        if (Convert.ToUInt32(dr["STATE"]) == 1) //如果是正在被叫号
                        {
                            sdnQueItem.msgState = 0;//当前号码状态
                        }
                        else if (Convert.ToUInt32(dr["STATE"]) > 1)
                        {
                            iDealCount++; //已经处理数增加
                            sdnQueItem.msgState = Convert.ToUInt32(dr["STATE"]);//当前号码状态
                        }
                        else
                        {
                            sdnQueItem.msgState = Convert.ToUInt32(dr["STATE"]);//当前号码状态
                        }
                        if (dr["QUEUE_NUM"].ToString().Substring(0, 1) == "1") //如果正常就添加到正常队列
                        {
                            // sdnQueList.Add(dr["QUEUE_NUM"].ToString(), sdnQueItem); //添加队列信息
                            sdnQueList.Add(dr["REMARK"].ToString(), sdnQueItem); //添加队列信息
                        }
                        else if (dr["QUEUE_NUM"].ToString().Substring(0, 1) == "D") //如果园区特殊屏，添加到园区队列
                        {
                            // sdnQueList.Add(dr["QUEUE_NUM"].ToString(), sdnQueItem); //添加队列信息
                            sdnQueList_YQ.Add(dr["REMARK"].ToString(), sdnQueItem); //添加队列信息
                        }
                        else
                        {
                            //sdnQueList_YY.Add(dr["QUEUE_NUM"].ToString(), sdnQueItem); //添加队列信息  如果是预约信息添加到预约队列
                            sdnQueList_YY.Add(dr["REMARK"].ToString(), sdnQueItem); //添加队列信息  如果是预约信息添加到预约队列
                        }

                        if (dicNoTimes.ContainsKey(sdnQueItem.msgCardNo)) //如果有值
                        {
                            dicNoTimes[sdnQueItem.msgCardNo]++;

                        }
                        else //该证件未取过值
                        {
                            dicNoTimes.Add(sdnQueItem.msgCardNo, 1);//初次 字典添加数据
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("初始化检测到数据库异常==" + ex.Message);
            }
        }

        /// <summary>
        /// 添加排队信息到本地数据库
        /// </summary>
        /// <param name="sdnIdCard">身份证信息</param>
        /// <param name="strQueNo">排队号码</param>
        /// <param name="_strQueType">号码类型</param>
        /// <param name="iTemp">号码数字</param>
        /// <param name="strSerial">24位排队序列号</param>
        private void sdnAddLocalDB(hardware.IDCard sdnIdCard, string strQueNo, string _strQueType, int iTemp, string strSerial)
        {
            try
            {
                #region  向 系统排队队列（T_SYS_QUEUE）插入数据
                T_SYS_QUEUE t_sys_queue = new T_SYS_QUEUE();
                t_sys_queue.CARDNO = sdnIdCard.CartNo;
                t_sys_queue.FIRST_TIME = DateTime.Now;
                t_sys_queue.GET_TIMES = 1;
                t_sys_queue.LAST_TIME = DateTime.Now;
                t_sys_queue.NAME = sdnIdCard.Name;
                t_sys_queue.QUEUE_NUM = strQueNo;
                t_sys_queue.STATE = 0;
                t_sys_queue.REMARK = strSerial;//24位排队序列号

                new QueueSys.BLL.T_SYS_QUEUE().Add(t_sys_queue);
                #endregion

                #region  向 系统排队队列（T_QUE_MSG）插入数据
                T_QUE_MSG Mt_que_msg = new T_QUE_MSG();
                Mt_que_msg.NO = iTemp;
                Mt_que_msg.QUEUE_NO = strQueNo;
                Mt_que_msg.CLIENT_NO = DateTime.Now.ToString("yyyy/MM/dd");
                Mt_que_msg.CREATE_TIME = DateTime.Now;
                new QueueSys.BLL.T_QUE_MSG().Add(Mt_que_msg);
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show("数据库连接错误==" + ex.Message);

            }
        }
        /// <summary>
        /// 保存取票身份证信息到本地数据库
        /// </summary>
        /// <param name="sdnIdCard"></param>
        /// <param name="_strQueType"></param>
        /// <param name="strSerial"></param>
        private void sdnAddCardMsg(hardware.IDCard sdnIdCard, string _strQueType, string strSerial)
        {
            try
            {
                T_CARD_MSG cardMsg = new T_CARD_MSG();
                cardMsg.ADDRESS = sdnIdCard.Address;
                cardMsg.CARD_NUM = sdnIdCard.CartNo;
                cardMsg.CREATTIME = DateTime.Now;
                if (!string.IsNullOrEmpty(sdnIdCard.Birthday))
                {
                    string[] arrBirth = sdnIdCard.Birthday.Split('-');
                    if (arrBirth.Length > 2)
                    {
                        cardMsg.YEAR = arrBirth[0];
                        cardMsg.MONTH = arrBirth[1];
                        cardMsg.DAY = arrBirth[2];
                    }
                }
                cardMsg.ENDTIME = sdnIdCard.End_validity;
                cardMsg.NAME = sdnIdCard.Name;
                cardMsg.NATION = sdnIdCard.Nationality;
                string strPhotoPath = AppDomain.CurrentDomain.BaseDirectory + "cardPhoto\\";//拼接路径

                if (!Directory.Exists(strPhotoPath)) //图片存放路径是否存在
                {//如果图片路径不存在
                    Directory.CreateDirectory(strPhotoPath);//创建路径
                }
                string strNewPath = strPhotoPath + "1_" + sdnIdCard.CartNo + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".bmp";
                if (!string.IsNullOrWhiteSpace(sdnIdCard.Photo) && File.Exists(sdnIdCard.Photo))
                {
                    File.Copy(sdnIdCard.Photo, strNewPath);
                }
                cardMsg.PHOTOPATH = strNewPath;
                cardMsg.SERIALNUM = strSerial;
                cardMsg.SEX = sdnIdCard.Sex;
                cardMsg.SIGN = sdnIdCard.Institution;
                cardMsg.STARTTIME = sdnIdCard.Begin_validity;
                cardMsg.STATE = "0";
                new QueueSys.BLL.T_CARD_MSG().Add(cardMsg); //上传数据到数据库
            }
            catch (Exception ex)
            {
                Common.SysLog.WriteLog(ex, AppDomain.CurrentDomain.BaseDirectory);
            }
        }

        #endregion

        #region 与六合一平台接口通信

        #region 开机获取取票机备案信息
        private void iniBAinfo()
        {
            OperQueueData2DB opDb = new OperQueueData2DB(strBBDM);
            string strMsg = "";
            dtServerInfo = opDb.getBAInfo(strBBDM, "", "", "", strServerIp, out strMsg);
            if (dtServerInfo == null)
            {
                MessageBox.Show(strMsg); //
                //throw new Exception(strMsg);//抛出错误信息
            }
        }
        #endregion

        #region 补传排队取票信息

        #endregion

        #region 写入评价信息

        #endregion


        #endregion

        #region 得到系统记录的全局信息
        /// <summary>
        /// 根据窗口电脑IP得到备案字典信息窗口信息
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private string getWinNum(string strIP)
        {
            //return dicServerInfo[str];
            // return dtServerInfo.Where()
            if (dtServerInfo != null)
            {
                DataRow[] arrDR = dtServerInfo.Select("jsjip='" + strIP + "'");
                string strWinNum = arrDR[0]["ckbh"].ToString();
                return Convert.ToInt32(strWinNum) + "";
            }
            return "1";
        }
        /// <summary>
        /// 从备案信息中得到10位设备计算机控制编码，作为全局唯一编号与生产24位排队序列号
        /// </summary>
        /// <returns></returns>
        private string getMachineSerialNum()
        {
            if (dtServerInfo != null)
            {
                return dtServerInfo.Rows[0]["sbkzjsjbh"].ToString();

            }
            else
            {
                MessageBox.Show("必须先从六合一获取备案信息！！！");
                return null;
            }
        }

        /// <summary>
        /// 根据排队号码生产22位排队序列号
        /// </summary>
        /// <param name="strQueNum"></param>
        /// <returns></returns>
        private string produceSerialNum(string strQueNum)
        {
            string dateNum = DateTime.Now.ToString("yyMMdd");
            string macNum = getMachineSerialNum();
            if (string.IsNullOrWhiteSpace(macNum)) //10位机器码不可为空
            {
                return null;
            }
            else
            {
                string strSerialNum = dateNum + macNum + "00" + strQueNum; //strQueum 为4位数 如A001
                return strSerialNum;
            }

        }

        //   public string 


        #endregion

        #region 补传取票信息到六合一系统

        private void AddBCqueue(string strWinIp, string strGLBM)
        {
            try
            {
                QueueItem sdnItem = new QueueItem();
                sdnItem.msgState = 0;
                sdnItem.windowIp = strWinIp;//窗口IP
                sdnItem.windowNum = getWinNum(strWinIp); //窗口号
                sdnItem.bmdm = strGLBM; //部门代码
                // if(queuelist_BC)
                queuelist_BC.Add(strWinIp, sdnItem); //用IP当作关键字
            }
            catch { }
        }

        /// <summary>
        /// 自动上传（补充）取票信息到六合一系统
        /// </summary>
        private void uploadQueueSelf()
        {
            QueueItem sdnItem = queuelist_BC.GetTopOutQueue();//得到未使用队列
            if (sdnItem != null) //如果不为null 即补传队列有数据
            {
                //  QueueItem queItem = GetQueueItem(sdnItem.windowIp, sdnItem.windowNum);
                if (httpserver != null)
                {
                    string strReqData = string.Format("{{\"opType\":\"TMRI_CALLOUT\",\"reqdata\":{ \"ywckjsjip\": \"{0}\", \"glbm\": \"{1}\" }, \"charset\": \"utf-8\" }}", sdnItem.windowIp, sdnItem.bmdm);
                    //调用六合一补传信息接口
                    httpserver.cmdStartRec(null, strReqData);//调用叫号指令

                    //插入六合一数据
                    //  new OperQueueData2DB().writeBCQH(sdnItem.XH,"","","",sdnItem.windowIp,strServerIp)
                }
            }
        }

        #endregion

        #region 暂停/恢复取票 暂停当前窗口的取票
        /// <summary>
        /// 
        /// </summary>
        /// <param name="iControl">控制码</param>
        /// <param name="strJSJIP">计算机IP</param>
        private void sdnControlCall(string iControl, string strJSJIP)
        {
            if (iControl == "0")//暂停
            {
                if (dicPause.ContainsKey(strJSJIP))
                {
                    dicPause[strJSJIP] = iControl;
                }
                else
                {
                    dicPause.Add(strJSJIP, iControl);
                }
            }
            else //恢复
            {
                if (dicPause.ContainsKey(strJSJIP))
                {
                    dicPause.Remove(strJSJIP);
                }
            }
        }
        /// <summary>
        /// 验证某计算机IP是否处于暂停状态
        /// </summary>
        /// <param name="strIp">请求计算机IP</param>
        /// <returns></returns>
        private bool isPause(string strIp)
        {
            if (dicPause.ContainsKey(strIp) && dicPause[strIp] == "0")
            {
                return true;
            }
            else
            {
                return false;
            }
        }



        #endregion

        #region 回滚业务
        /// <summary>
        /// 根据排队号 回滚业务
        /// </summary>
        /// <param name="strPDH">排队号</param>
        private void sdnGoBack(string strPDH)
        {
            string strXLH = DateTime.Now.ToString("yyMMdd") + getMachineSerialNum();//得到16位序列号
            UpdateQueueItemState(strXLH + "00" + strPDH, 0); //更新为未叫号状态
            iDealCount--;//已经处理减一
            iNoDealCount++;//未处理加一
        }

        #endregion

        #region 其他车管业务

        /// <summary>
        /// 其他业务  （用于js函数调用）
        /// </summary>
        /// <param name="sdnType">操作类型（读卡 1/手工输入 2）</param>
        /// <returns></returns>
        public string YQ_MsgQueue(string _strQueType)
        {

            //1 弹出读卡界面
            //2 读卡
            //3 获取读卡信息

            string strRes = "";
            string strMsg = "";
            string strQueueNo = "";
            string strSerialNum = "";//24位排队序列号
            IDCard sdnIdCard = new IDCard();//身份证信息类 
            IdentityCard identity = new IdentityCard();
            sdnIdCard = identity.GetBaseMsg(1, out strMsg);
            if (sdnIdCard != null && sdnIdCard.CartNo != "")
            {
                if (sdnQueList_YQ.List.Count > 0)
                {
                    QueueItem lastqueue = sdnQueList_YQ.List.Values[sdnQueList_YQ.List.Count - 1]; //上个队列数据
                    if (lastqueue.msgCardNo == sdnIdCard.CartNo)
                    {
                        strRes = string.Format("{{\"flag\":\"{0}\",\"cardno\":\"{1}\",\"msg\":\"{2}\"}}", "false", "err", "该证件取票过于频繁");
                        return strRes;
                    }
                }
                if (dicNoTimes.ContainsKey(sdnIdCard.CartNo) && dicNoTimes[sdnIdCard.CartNo] >= iMaxCount)
                {
                    strRes = string.Format("{{\"flag\":\"{0}\",\"cardno\":\"{1}\",\"msg\":\"{2}\"}}", "false", "err", "该证件取票过于频繁");
                    return strRes;
                }
                else if (dicNoTimes.ContainsKey(sdnIdCard.CartNo)) //包含相应的键值对
                {
                    dicNoTimes[sdnIdCard.CartNo]++;

                }
                else
                {
                    dicNoTimes.Add(sdnIdCard.CartNo, 1);
                }
                //4 添加队列
                QueueItem sdnQueueItem = new QueueItem();
                sdnQueueItem.msgCardNo = sdnIdCard.CartNo;//身份证号
                sdnQueueItem.msgName = sdnIdCard.Name;
                strQueueNo = GetNewQueNo(_strQueType);
                strSerialNum = produceSerialNum(strQueueNo); //得到24位编码序列号
                sdnQueueItem.msgQueueNo = strQueueNo;
                sdnQueueItem.serialNum = strSerialNum;
                sdnQueueItem.msgState = 0;//初始状态为0
                sdnQueueItem.XH = "";//序号
                sdnQueueItem.mWay = "1";//正常读卡
                sdnQueueItem.strqhsj = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                sdnQueList_YQ.Add(strSerialNum, sdnQueueItem);//添加排队数据到园区排队队列中
                //5 添加数据库
                switch (_strQueType)
                {
                    case "1":
                        sdnAddLocalDB(sdnIdCard, strQueueNo, _strQueType, iMaxA, strSerialNum); //插入数据到本地数据库
                        sdnAddCardMsg(sdnIdCard, _strQueType, strSerialNum);//插入身份证信息到本地库
                        break;
                    case "B":
                        sdnAddLocalDB(sdnIdCard, strQueueNo, _strQueType, iMaxB, strSerialNum); //插入数据到本地数据库
                        sdnAddCardMsg(sdnIdCard, _strQueType, strSerialNum);//插入身份证信息到本地库
                        break;
                    case "C":
                        sdnAddLocalDB(sdnIdCard, strQueueNo, _strQueType, iMaxC, strSerialNum); //插入数据到本地数据库
                        sdnAddCardMsg(sdnIdCard, _strQueType, strSerialNum);//插入身份证信息到本地库
                        break;
                    case "D":
                        sdnAddLocalDB(sdnIdCard, strQueueNo, _strQueType, iMaxD, strSerialNum); //插入数据到本地数据库
                        sdnAddCardMsg(sdnIdCard, _strQueType, strSerialNum);//插入身份证信息到本地库
                        break;
                    default:
                        sdnAddLocalDB(sdnIdCard, strQueueNo, _strQueType, iMaxA, strSerialNum); //插入数据到本地数据库
                        sdnAddCardMsg(sdnIdCard, _strQueType, strSerialNum);//插入身份证信息到本地库
                        break;
                }

                //6 插入远程数据库  远程数据库 什么时候插入根据web网页存放的位置有关
                if (isNet == 1)
                {
                    AddQueueData(sdnQueueItem, _strQueType, 1); //异步调用API插入远程数据库
                }
                sdn_CardNo = sdnIdCard.CartNo; //给程序赋值全局变量——身份证号
                // MessageBox.Show(strAdress);
                // strAdress = System.Web.HttpUtility.UrlEncode(strAdress,Encoding.UTF8);
                string strQueMsg = string.Format("[{{ \"nonum\":\"{0}\",\"cardno\":\"{1}\",\"name\":\"{2}\",\"queno\":\"{3}\",\"address\":\"{4}\"}}]", (iAllCount - iDealCount) + "", sdnIdCard.CartNo, sdnIdCard.Name, strQueueNo, strAdress);
                strRes = string.Format("{{\"flag\":\"{0}\",\"cardno\":\"{1}\",\"msg\":{2}}}", "true", sdnIdCard.CartNo, strQueMsg);
            }
            else
            {

                strRes = string.Format("{{\"flag\":\"{0}\",\"cardno\":\"{1}\",\"msg\":\"{2}\"}}", "false", "1", "读卡失败");
                return strRes;
            }
            iAllCount++; //排队总数增加
            //sdnQuePrintJS((iAllCount - iDealCount) + "", sdnIdCard.CartNo, sdnIdCard.Name, strQueueNo, strBBDM);
            return strRes;

        }

        #endregion


        public string getSystemid()
        {
            return i_showType.ToString();
        }

        #region 硬键盘取票功能



        #endregion


        #region redis 锁、订阅/发布函数
        /// <summary>
        /// 发布信息
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="msg"></param>
        private void pub_msg_event(string channel, string msg)
        {
            try
            {
                pub_msg(channel, msg);
            }
            catch { }
        }

        /// <summary>
        /// 向指定的频道发布信息
        /// </summary>
        /// <param name="channel">频道</param>
        /// <param name="msg">发布信息</param>
        private async void pub_msg(string channel, string msg)
        {
            await _redis.PublishAsync(channel, msg);
        }
        /// <summary>
        /// 订阅指定的频道，接收对应信息
        /// </summary>
        /// <param name="channel"></param>
        private async void sub_msg(string channel)
        {
            await _redis.SubscribeAsync(channel, (cha, message) =>
            {
                Console.WriteLine("接受到发布的内容为：" + message);
                //   MessageBox.Show("接受到发布的内容为：" + message);
            });
        }
        /// <summary>
        /// 取消所有订阅
        /// </summary>
        private void unSubAll()
        {
            _redis.UnsubscribeAll();
        }

        #endregion

    }
}
