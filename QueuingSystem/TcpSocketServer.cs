using Common;
using QueuingSystem.OperQueue;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace QueuingSystem
{
    public class TcpSocketServer
    {
        #region 定义变量

        private bool m_IsRun;
        private byte[] m_MSG = new byte[0x10000];
        private ManualResetEvent m_MySet = new ManualResetEvent(false);// ManualResetEvent 允许线程通过发信号互相通信
        private Socket m_Server;
        private Socket m_Client;
        private string m_SVRIP;
        private int m_SVRPort;
        private string com;//COM口
        private string bps;//波特率
        private string cardip1;//波特率
        private string cardip2;//波特率
        private string ZHPServerIp;//综合屏控制卡IP
                                   /// <summary>
                                   /// 获取队列中的第一项
                                   /// </summary>
                                   /// <returns></returns>  
        //  public delegate QueueItem dlgtGetQueueItem();
        //   public event dlgtGetQueueItem eventGetQueueItem;

        /// <summary>
        /// 获取队列中的第一项,并更新当前使用此号的窗口IP和窗口号
        /// </summary>
        /// <returns></returns>  
        public delegate QueueItem dlgtGetQueueItem(string strWinIp, string strWinNum);
        public event dlgtGetQueueItem eventGetQueueItem;
        /// <summary>
        /// 获取园区特殊屏队列中的第一项
        /// </summary>
        /// <returns></returns>
        public delegate QueueItem dlgtGetQueueItemwuli(string winnum);
        public event dlgtGetQueueItemwuli eventGetQueueItemwuli;
        /// <summary>
        /// 获取园区特殊屏队列中的第一项
        /// </summary>
        /// <returns></returns>
        public delegate QueueItem dlgtGetQueueItemwuliRe(string winnum);
        public event dlgtGetQueueItemwuli eventGetQueueItemwuliRe;
        /// <summary>
        /// 获取总排队人数
        /// </summary>
        /// <returns></returns>
        public delegate string dlgtGetQueueCount();
        public event dlgtGetQueueCount eventGetQueueCount;
        /// <summary>
        /// 委托  更新队列状态
        /// </summary>
        /// <param name="strKey"></param>
        /// <param name="iState"></param>
        public delegate void dlgUpdateQueue(string strKey, int iState);
        public event dlgUpdateQueue eventUpdateQueue;
        /// <summary>
        /// 根据关键字从队列中移除该项
        /// </summary>
        /// <param name="strKey"></param>
        public delegate void dlgtRemoveQueueItem(string strKey);
        public event dlgtRemoveQueueItem eventRemoveQueueItem;

        /// <summary>
        /// 根据业务窗口IP得到对应的取票队列
        /// </summary>
        /// <param name="strIp"></param>
        /// <returns></returns>
        public delegate QueueItem dlgFindQueueByIP(string strIp);
        public event dlgFindQueueByIP eventFindQueueByIP;

        /// <summary>
        /// 设置取号窗口的取号功能是否可用
        /// </summary>
        /// <param name="strCmd">命令 pause-暂停 不可用 restart-恢复 可用</param>
        public delegate void dlgtIsPause(string strCmd);
        public event dlgtIsPause eventIsPause;
        /// <summary>
        /// 获取机器码
        /// </summary>
        /// <returns></returns>
        public delegate string dlgGetMachineNum();
        public event dlgGetMachineNum eventGetMachineNum;

        private System.IO.Ports.SerialPort serialPortCaller;

        hardware.CallNumberAudio audio = new hardware.CallNumberAudio("wavFiles"); //播放语音实体类
        public string windownum { get; set; } //窗口号
        public string strBBDM { get; set; } //部门代码
        public string strQHXLH { get; set; }//16位全局唯一取号序列号
        /// <summary>
        /// 叫号信息
        /// </summary> 
        List<string> callinfos = new List<string>(); //叫号音频信息存放
        List<string> callinfoss = new List<string>();//吴江盛泽
        List<string> callinfowuli = new List<string>();//园区物理叫号
        List<string> list_yz_led = new List<string>();//扬州综合屏
        List<string> list_Done = new List<string>();//叫号信息（正在或者已经叫号)
        bool bLEDShow = false;//条屏
        bool bZHPShow = false;//综合屏
        bool bLED2 = false;//综合屏
        private int i_ShowType = 0;//显示种类

        #endregion

        public TcpSocketServer(string svrIP, int svrPort, int program_id)
        {
            this.m_SVRIP = svrIP;
            this.m_SVRPort = svrPort;
            this.m_IsRun = true;
            if (program_id == 8)
            {
                new Thread(call_audiowuli).Start(); //单独线程进行叫号语音播放
            }
            new Thread(call_audio).Start(); //单独线程进行叫号语音播放
            i_ShowType = program_id;

            operConfig.ReadIniFile rnd = new operConfig.ReadIniFile(AppDomain.CurrentDomain.BaseDirectory + "configs\\Tiaopinconfig.ini");
            com = rnd.ReadValue("comname", "value"); //串口名称
            bps = rnd.ReadValue("comspeed", "value"); //波特率
            cardip1 = rnd.ReadValue("cardip1", "value"); //一号卡IP
            cardip2 = rnd.ReadValue("cardip2", "value"); //二号卡IP
            ZHPServerIp = rnd.ReadValue("ZHPServerIp", "value"); //二号卡IP
        }

        #region 接收连接、接收信息
        public void AcceptCallBack(IAsyncResult ia)
        {
            this.m_MySet.Set();
            try
            {
                //通过IAsyncResult.AsyncState获得传入的套接字(EndAccept与beginAccept对应防止网络阻塞)
                Socket soc = ((Socket)ia.AsyncState).EndAccept(ia);
                byte[] msg_temp = new byte[0x10000];
                m_Client = soc;
                int msgCount = soc.Receive(msg_temp);//从绑定的 Socket 套接字接收数据，将数据存入接收缓冲区
                this.m_MSG = msg_temp;
                if (msgCount > 0)
                {
                    //========================================================第一次接收数据时，要先解析得到的数据，并做出相应的回应===START
                    try
                    {
                        // string recStr = Encoding.Default.GetString(m_MSG, 0, msgCount); //查看接收到的数据
                        // DealRecAppMsg(this.m_MSG, recvCount, client);
                        DealRecAppMsg(m_MSG, msgCount, soc);
                    }
                    catch (Exception ex)
                    {
                        this.CloseSocket(soc);
                    }
                    //========================================================第一次接收数据时，要先解析得到的数据，并做出相应的回应 ==========END==
                    object[] arrObj = new object[] { soc, msgCount };
                    ThreadPool.UnsafeQueueUserWorkItem(new WaitCallback(this.Recv), arrObj); //将Recv给线程池委托，并把这个委托排队到线程池
                    // new Thread(Recv).Start(arrObj);//异步开启接收线程
                }
                else
                {
                    this.CloseSocket(soc);
                }
            }
            catch (SocketException exception)
            {
            }
        }
        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="soc"></param>
        public void Recv(object objparm)
        {
            try
            {
                object[] arrParm = (object[])objparm;
                Socket client = (Socket)arrParm[0];
                // int iLength = (int)arrParm[1];
                while (this.m_IsRun && client.Connected)
                {
                    try
                    {
                        byte[] m_MSG_local = new byte[0x10000];
                        int recvCount = client.Receive(m_MSG_local); //从绑定的 Socket 套接字接收数据，将数据存入接收缓冲区
                        if (recvCount > 0)
                        {
                            // string recStr = Encoding.Default.GetString(m_MSG_local, 0, recvCount); //查看接收到的数据
                            DealRecAppMsg(m_MSG_local, recvCount, client);
                        }
                        else
                        {
                            this.CloseSocket(client);
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            catch (SocketException ex)
            {
            }
        }
        /// <summary>
        /// 处理接收到客户端发送过来的信息
        /// </summary>
        private void DealRecAppMsg(byte[] bs, int recvCount, Socket soc)
        {
            try
            {
                string strMsg; //处理xml文档的返回结果
                // string strMsgLenght = Encoding.Default.GetString(bs, 0, 8);//前八个字节是信息长度
                string strMsgContent = Encoding.Default.GetString(bs, 8, recvCount - 8);//获取所有的数据
                if (string.IsNullOrEmpty(strMsgContent))
                {
                    return;
                }
                cmdStartRec(strMsgContent, soc, out strMsg);
            }
            catch (Exception ex)
            {
                //  SysLog.WriteLog(ex, Application.StartupPath);//记录异常日志
            }
        }
        #endregion

        #region 根据命令处理
        /// <summary>
        /// 开始检测命令处理
        /// </summary>
        /// <param name="strXml"></param>
        /// <param name="strMsg"></param>
        /// <returns></returns>
        private void cmdStartRec(string strXml, Socket soc, out string strMsg)
        {
            try
            {
                strMsg = "成功";
                Dictionary<string, string> dicXmlValues = xmlRead(strXml, "diagram", null, null, null);//得到所有属性的值，存放到字典中
                if (dicXmlValues != null && dicXmlValues.Count > 0)
                {
                    string strType = dicXmlValues["type"];
                    string strCall_addr = "1";//窗口号
                    string res = string.Empty;
                    try
                    {
                        strCall_addr = dicXmlValues["calladdr"];
                        res = (256 + Convert.ToInt32(strCall_addr)).ToString();
                    }
                    catch { }
                    string strWindowsIp = "127.0.0.1";
                    try
                    {
                        if (dicXmlValues.ContainsKey("winip"))
                        {
                            strWindowsIp = dicXmlValues["winip"];
                        }

                    }
                    catch { }
                    Common.SysLog.WriteOptDisk("TCPSocket服务：" + strType + strCall_addr, AppDomain.CurrentDomain.BaseDirectory); //记录日志
                    switch (strType)
                    {

                        case "call": //叫号
                            int iCall = 0;//多次发送到LED
                            QueueItem p = eventGetQueueItem(strWindowsIp, strCall_addr);  //获取队列最新项  //获取队列最新项
                            string count = eventGetQueueCount(); //得到当前排队总数
                            if (p == null) //当前无人
                            {
                                //SendMsg2JCX(ToCmdXmlMsg("", "", "0"), soc); //返回前台数据
                                SendMsg2JCX(ToCmdXmlMsg("", "", count, "", strBBDM, ""), soc); //返回前台数据
                                break;
                            }

                            //   SendMsg2JCX(ToCmdXmlMsg(p.msgQueueNo, p.msgCardNo, count), soc); //返回前台数据
                            SendMsg2JCX(ToCmdXmlMsg(p.msgQueueNo, p.msgCardNo, count, p.msgName, p.bmdm, p.serialNum), soc);
                            //  eventRemoveQueueItem(p.msgQueueNo);  //测试 叫号时移除该项
                            //3.调用音频
                            callinfos.Add(p.msgQueueNo + "," + strCall_addr); //语音叫号用
                            callinfoss.Insert(0, ZHPServerIp);
                            callinfoss.Insert(1, p.msgQueueNo + "," + strCall_addr);
                            list_Done.Insert(0, $"{{ \"que_no\":\"{p.msgQueueNo}\",\"win_no\":\"{strCall_addr}\"}}");
                            string str_yz_led_json = "{{\"ip1\":\"{0}\",\"ip2\":\"{1}\",\"queno\":\"{2}\",\"winno\":\"{3}\"}}";
                            str_yz_led_json = string.Format(str_yz_led_json, cardip1, cardip2, p.msgQueueNo, strCall_addr);
                            list_yz_led.Add(str_yz_led_json);
                            try
                            {

                                //系统种类 0：无显示 1:双屏  2：电视盒子  3：综合屏  4：综合屏+LED条屏1  5：综合屏+LED条屏2  6：LED条屏1  7：LED条屏2
                                switch (i_ShowType)
                                {
                                    case 3://只有综合屏
                                        new Thread(ZongHeShowMsg.ZongheShow.CLEDSender.sendData2LEDYXC1).Start(callinfoss);
                                        break;
                                    case 4:
                                        new Thread(ShowMsg.LEDshow.sendData2LEDYXC).Start(new string[] { com, bps, "请" + p.msgQueueNo + "到" + strCall_addr + "窗口", strCall_addr });
                                        new Thread(ZongHeShowMsg.ZongheShow.CLEDSender.sendData2LEDYXC).Start(callinfoss);
                                        break;
                                    case 5:
                                        new Thread(ShowMsg.LEDshow.sendData2LED).Start(new string[] { com, bps, res, "请" + p.msgQueueNo + "到" + strCall_addr + "窗口" });
                                        new Thread(ZongHeShowMsg.ZongheShow.CLEDSender.sendData2LEDYXC1).Start(callinfoss);
                                        break;
                                    case 6:
                                        //while (iCall<3) //发送三次到LED条屏
                                        //{
                                        new Thread(ShowMsg.LEDshow.sendData2LEDYXC).Start(new string[] { com, bps, "请" + p.msgQueueNo + "到" + strCall_addr + "窗口", strCall_addr });
                                        //    iCall++;
                                        //}
                                        try
                                        {
                                            Common.SysLog.WriteOptDisk("综合屏显示：", AppDomain.CurrentDomain.BaseDirectory); //记录日志
                                            new Thread(ZongHeShowMsg.ZongheShow.CLEDSender.sdnSendData2ZHP_yz).Start(list_yz_led);
                                        }
                                        catch (Exception ex)
                                        {
                                            Common.SysLog.WriteOptDisk(ex.Message, AppDomain.CurrentDomain.BaseDirectory);
                                        }
                                        break;
                                    case 7:
                                        new Thread(ShowMsg.LEDshow.sendData2LED).Start(new string[] { com, bps, res, "请" + p.msgQueueNo + "到" + strCall_addr + "窗口" });
                                        new Thread(ZongHeShowMsg.ZongheShow.CLEDSender.sendData2LEDYXC_NEW).Start(callinfoss);
                                        break;
                                    case 8:// 园区特殊屏幕
                                        //new Thread(ZongHeShowMsg.ZongheShow.CLEDSender.updateData2LEDYQCGS).Start(strCall_addr + "," + p.msgQueueNo);
                                        break;
                                    case 10://吴江车管所新车大厅综合屏
                                        string str_wait_count = eventGetQueueCount(); //得到当前排队总数
                                        int iMax_rows = list_Done.Count >= 6 ? 6 : list_Done.Count;
                                        string str_done_temp = "";
                                        for (int i = 0; i < iMax_rows; i++) //取前六条数据
                                        {
                                            str_done_temp += list_Done[i] + ",";
                                        }
                                        if (str_done_temp.Length > 1)
                                        {
                                            str_done_temp = str_done_temp.Substring(0, str_done_temp.Length - 1);
                                        }
                                        string str_queueNO_temp = p.msgQueueNo.Substring(1); //得到排队号的后三位
                                        string str_wait_temp = "";//等待队列数
                                        int iMax_wait_no = list_Done.Count >= 12 ? 12 : list_Done.Count;
                                        for (int i = 1; i <= iMax_rows; i++)
                                        {
                                            str_wait_temp += "1" + (Convert.ToInt32(str_queueNO_temp) + i) + ",";  //拼接成 1 001 这种叫号格式
                                        }
                                        if (str_wait_temp.Length > 1)
                                        {
                                            str_wait_temp = str_wait_temp.Substring(0, str_wait_temp.Length - 1);
                                        }
                                        string led_content = $"{{\"count\":{str_wait_count},\"done\":[{str_done_temp}],\"wait\":\"{str_wait_temp}\"}}";
                                        LED_Util.zhonghe.showMsg_zh.sendMsg2Screen(led_content); //发送信息到综合屏

                                        break;

                                    default: //默认
                                        break;

                                }
                                new Thread(DealQueue).Start(new string[] { p.serialNum, "1", strCall_addr }); //更新本地数据库中的数据
                                new Thread(WriteOptDisk).Start(new string[] { strCall_addr, res, com, bps, p.msgQueueNo });//记录日志

                               // new Thread(sdnAddCardMsg2Queue).Start(new string[] { strWindowsIp, p.serialNum });//上传身份证信息到业务数据库
                               // new Thread(sdnStartRecVideo).Start(new string[] { p.msgCardNo, strWindowsIp, p.strqhsj });//开始录像
                            }
                            catch (Exception ex)
                            {
                                Common.SysLog.WriteOptDisk("TCPSocket服务异常：" + ex.Message, AppDomain.CurrentDomain.BaseDirectory); //记录日志
                            }
                            break;
                        case "recall": //重叫
                            try
                            {
                                int iRecall = 0;//多次发送信息到LED条屏
                                string strKey = dicXmlValues["queueno"];
                                callinfos.Add(strKey + "," + strCall_addr);
                                //系统种类 0：无显示 1:双屏  2：电视盒子  3：综合屏  4：综合屏+LED条屏1  5：综合屏+LED条屏2  6：LED条屏1  7：LED条屏2
                                switch (i_ShowType)
                                {
                                    case 3://只有综合屏
                                        new Thread(ZongHeShowMsg.ZongheShow.CLEDSender.sendData2LEDYXC1).Start(callinfoss);
                                        break;
                                    case 4:
                                        new Thread(ShowMsg.LEDshow.sendData2LEDYXC).Start(new string[] { com, bps, "请" + strKey + "到" + strCall_addr + "窗口", strCall_addr });
                                        new Thread(ZongHeShowMsg.ZongheShow.CLEDSender.sendData2LEDYXC1).Start(callinfoss);
                                        break;
                                    case 5:
                                        new Thread(ShowMsg.LEDshow.sendData2LED).Start(new string[] { com, bps, res, "请" + strKey + "到" + strCall_addr + "窗口" });
                                        new Thread(ZongHeShowMsg.ZongheShow.CLEDSender.sendData2LEDYXC1).Start(callinfoss);
                                        break;
                                    case 6:
                                        while (iRecall < 3)
                                        {
                                            new Thread(ShowMsg.LEDshow.sendData2LEDYXC).Start(new string[] { com, bps, "请" + strKey + "到" + strCall_addr + "窗口", strCall_addr });
                                            iRecall++;
                                        }

                                        break;
                                    case 7:
                                        new Thread(ShowMsg.LEDshow.sendData2LED).Start(new string[] { com, bps, res, "请" + strKey + "到" + strCall_addr + "窗口" });
                                        break;
                                    case 8:// 园区特殊屏幕
                                        //new Thread(ZongHeShowMsg.ZongheShow.CLEDSender.updateData2LEDYQCGS).Start(strCall_addr + "," + strKey);
                                        break;
                                    default: //默认
                                        break;

                                }
                            }
                            catch { }
                            break;
                        case "here": //人员到达
                            try
                            {  //更新状态为5（人员到达，但不知道完成结果）
                                string strKey1 = dicXmlValues["queueno"];
                                strQHXLH = eventGetMachineNum();
                                eventUpdateQueue(DateTime.Now.ToString("yyMMdd") + strQHXLH + strKey1, 5);//更新队列
                                new Thread(DealQueue).Start(new string[] { DateTime.Now.ToString("yyMMdd") + strQHXLH + strKey1, "5", strCall_addr }); //正在办理2
                                //new Thread(WriteOptDisk).Start(new string[] { strCall_addr, res, com, bps, strKey1 });
                            }
                            catch { }
                            break;
                        case "jump": //跳号
                            try
                            {
                                string strKey2 = dicXmlValues["queueno"];
                                strQHXLH = eventGetMachineNum();
                                string strKey1 = "A001";
                                QueueItem sdnTemp = eventFindQueueByIP(strWindowsIp);
                                try
                                {
                                    strCall_addr = sdnTemp.windowNum; //
                                    //strKey1 = sdnTemp.msgQueueNo;//得到排队号码
                                    strKey1 = sdnTemp.serialNum;
                                    //  res = (256 + Convert.ToInt32(strCall_addr)).ToString();
                                }
                                catch { }
                                eventUpdateQueue(DateTime.Now.ToString("yyMMdd") + strQHXLH + strKey2, 4);//更新队列
                                new Thread(DealQueue).Start(new string[] { DateTime.Now.ToString("yyMMdd") + strQHXLH + strKey2, "4", strCall_addr });//终止办理4
                               // new Thread(sdnEndRecVideo).Start(new string[] { sdnTemp.msgCardNo, sdnTemp.strqhsj });//结束录像
                                //new Thread(WriteOptDisk).Start(new string[] { strCall_addr, res, com, bps, strKey2 });
                            }
                            catch { }
                            break;
                        case "dook": //完成
                            try
                            {
                                string strKey3 = dicXmlValues["queueno"];
                                strQHXLH = eventGetMachineNum();
                                eventUpdateQueue(DateTime.Now.ToString("yyMMdd") + strQHXLH + strKey3, 2);//更新队列

                                string strKey1 = "A001";
                                QueueItem sdnTemp = eventFindQueueByIP(strWindowsIp);
                                try
                                {
                                    strCall_addr = sdnTemp.windowNum; //
                                    //strKey1 = sdnTemp.msgQueueNo;//得到排队号码
                                    strKey1 = sdnTemp.serialNum;
                                    //  res = (256 + Convert.ToInt32(strCall_addr)).ToString();
                                }
                                catch { }

                                new Thread(DealQueue).Start(new string[] { DateTime.Now.ToString("yyMMdd") + strQHXLH + strKey3, "2", strCall_addr });//完成办理3
                              //  new Thread(sdnEndRecVideo).Start(new string[] { sdnTemp.msgCardNo, sdnTemp.strqhsj });//结束录像
                                //new Thread(WriteOptDisk).Start(new string[] { strCall_addr, res, com, bps, strKey3 });
                            }
                            catch { }
                            break;
                        case "pause": //暂停取票
                            try
                            {
                                eventIsPause("pause");//读取身份证功能不可用 手动输入证件号取票也不可用
                                //系统种类 0：无显示 1:双屏  2：电视盒子  3：综合屏  4：综合屏+LED条屏1  5：综合屏+LED条屏2  6：LED条屏1  7：LED条屏2
                                //switch (i_ShowType)
                                //{
                                //    case 3://只有综合屏
                                //        new Thread(ZongHeShowMsg.ZongheShow.CLEDSender.sendData2LEDYXC1).Start(callinfoss);
                                //        break;
                                //    case 4:
                                //        new Thread(ShowMsg.LEDshow.sendData2LEDYXC).Start(new string[] { com, bps, "暂 停 服 务", strCall_addr });
                                //     //   new Thread(ZongHeShowMsg.ZongheShow.CLEDSender.sendData2LEDYXC1).Start(callinfoss);
                                //        break;
                                //    case 5:
                                //        new Thread(ShowMsg.LEDshow.sendData2LED).Start(new string[] { com, bps, res, "暂 停 服 务" });
                                //       // new Thread(ZongHeShowMsg.ZongheShow.CLEDSender.sendData2LEDYXC1).Start(callinfoss);
                                //        break;
                                //    case 6:
                                //        new Thread(ShowMsg.LEDshow.sendData2LEDYXC).Start(new string[] { com, bps, "暂 停 服 务", strCall_addr });
                                //        break;
                                //    case 7:
                                //        new Thread(ShowMsg.LEDshow.sendData2LED).Start(new string[] { com, bps, res, "暂 停 服 务" });
                                //        break;
                                //    default: //默认
                                //        break;

                                //}

                                //new Thread(WriteOptDisk).Start(new string[] { strCall_addr, res, com, bps, "暂停服务" });
                            }
                            catch { }
                            break;
                        case "restart": //恢复取票
                            try
                            {
                                eventIsPause("restart"); //恢复暂停的功能
                                //switch (i_ShowType)
                                //{
                                //    case 3://只有综合屏
                                //        new Thread(ZongHeShowMsg.ZongheShow.CLEDSender.sendData2LEDYXC1).Start(callinfoss);
                                //        break;
                                //    case 4:
                                //        new Thread(ShowMsg.LEDshow.sendData2LEDYXC).Start(new string[] { com, bps, "开 始 服 务", strCall_addr });
                                //        //   new Thread(ZongHeShowMsg.ZongheShow.CLEDSender.sendData2LEDYXC1).Start(callinfoss);
                                //        break;
                                //    case 5:
                                //        new Thread(ShowMsg.LEDshow.sendData2LED).Start(new string[] { com, bps, res, "开 始 服 务" });
                                //        // new Thread(ZongHeShowMsg.ZongheShow.CLEDSender.sendData2LEDYXC1).Start(callinfoss);
                                //        break;
                                //    case 6:
                                //        new Thread(ShowMsg.LEDshow.sendData2LEDYXC).Start(new string[] { com, bps, "开 始 服 务", strCall_addr });
                                //        break;
                                //    case 7:
                                //        new Thread(ShowMsg.LEDshow.sendData2LED).Start(new string[] { com, bps, res, "开 始 服 务" });
                                //        break;
                                //    default: //默认
                                //        break;
                                //}
                            }
                            catch { }
                            break;
                        case "sdnpj"://窗口评价控制端发送回来的数据，写入到六合一
                            //解析数据并发送到六合一
                            break;
                        case "count":
                            string count0 = eventGetQueueCount(); //得到当前排队总数
                            SendMsg2JCX(ToCmdXmlMsg("-1", "", count0, "", strBBDM, ""), soc); //返回前台数据
                            break;

                        default:
                            break;
                    }
                }
                else
                {
                    strMsg = "没有读取到值";
                }
            }
            catch (Exception ex)
            {
                strMsg = ex.Message;
            }
        }

        public void cmdRec(string strType, string calladdr, out string strMsg)
        {
            try
            {
                strMsg = "成功";
                string strCall_addr = "1";
                try
                {
                    strCall_addr = calladdr;
                }
                catch { }
                switch (strType)
                {
                    case "call": //叫号
                        string strCallAddr = calladdr;
                        QueueItem p = eventGetQueueItemwuli("wuli" + strCallAddr);
                        if (p != null)
                        {
                            //3.调用音频
                            callinfowuli.Add(p.msgQueueNo + "," + strCallAddr); //语音叫号用
                            //4，异步调用API 更新当前取票数据根据部门代码+时间+票号取出唯一对应的数据
                            try
                            {
                                //new Thread(ZongHeShowMsg.ZongheShow.CLEDSender.updateData2LEDYQCGS).Start(strCallAddr + "," + p.msgQueueNo);
                            }
                            catch { }
                            new Thread(DealQueuewuli).Start(new string[] { strCallAddr, p.msgQueueNo, "1" });//叫号完成后，修改数据库中的状态为已完成
                            new Thread(WriteOptDisk).Start(new string[] { "物理叫号" + strCallAddr, "", com, bps, p.msgQueueNo });//记录日志

                        }
                        break;
                    case "recall": //重叫
                        string strCallAddr1 = calladdr;
                        QueueItem p1 = eventGetQueueItemwuliRe("wuli" + strCallAddr1);
                        if (p1 != null)
                        {
                            //调用音频
                            callinfowuli.Add(p1.msgQueueNo + "," + strCallAddr1);
                            try
                            {
                                //new Thread(ZongHeShowMsg.ZongheShow.CLEDSender.updateData2LEDYQCGS).Start(strCallAddr1 + "," + p1.msgQueueNo);
                            }
                            catch { }
                        }
                        break;
                    case "jump": //跳号
                        try
                        {
                            string strKey2 = calladdr;
                            //eventUpdateQueue("wuli" + calladdr, 4);//更新队列
                            //new Thread(DealQueuewuli).Start(new string[] { strKey2, "", "4" });//跳号

                            QueueItem p2 = eventGetQueueItemwuli("wuli" + calladdr);
                            if (p2 != null)
                            {
                                callinfowuli.Add(p2.msgQueueNo + "," + strKey2); //语音叫号用
                                //4，异步调用API 更新当前取票数据根据部门代码+时间+票号取出唯一对应的数据
                                try
                                {
                                    // new Thread(ZongHeShowMsg.ZongheShow.CLEDSender.updateData2LEDYQCGS).Start(strKey2 + "," + p2.msgQueueNo);
                                }
                                catch { }

                                new Thread(DealQueuewuli).Start(new string[] { strKey2, p2.msgQueueNo, "2" });//叫号完成后，修改数据库中的状态为已完成
                                new Thread(WriteOptDisk).Start(new string[] { "物理叫号" + strKey2, "", com, bps, p2.msgQueueNo });//记录日志
                            }
                        }
                        catch { }
                        break;
                    case "here": //人员到达
                        string strKey1 = calladdr;
                        //eventUpdateQueue("wuli" + strKey1, 2);//更新队列
                        //new Thread(DealQueuewuli).Start(new string[] { strKey1, "", "2" });//完成办理3
                        break;
                    case "pause": //暂停取票
                        eventIsPause("pause");
                        break;
                    case "restart": //恢复取票
                        eventIsPause("restart");
                        break;
                    case "dook": //完成
                        try
                        {
                            string strKey3 = calladdr;
                            //eventUpdateQueue("wuli" + calladdr, 2);//更新队列
                            //new Thread(DealQueuewuli).Start(new string[] { strKey3, "", "2" });//完成办理3
                        }
                        catch { }

                        break;
                    default:
                        break;
                }

            }
            catch (Exception ex)
            {
                strMsg = ex.Message;
            }
        }

        /// <summary>
        /// 处理数据  异步
        /// </summary>
        /// <param name="obj"></param>
        private void DealQueue(object obj)
        {
            try
            {
                string[] arrStr = (string[])obj;
                OperQueueData2DB operQueue2DB = new OperQueueData2DB(strBBDM);
                if (arrStr[1] == "1") //如果为1 及为call指令
                {
                    operQueue2DB.UpdateDB_call(arrStr[0], 1, arrStr[2]); //把当前窗口为1的标记为0，即，当前窗口正在办理状态的标记为可再次叫号（下次叫号被叫到）
                }
                operQueue2DB.UpdateDBbySql(arrStr[0], Convert.ToInt32(arrStr[1]), arrStr[2]); //叫号后标记为1 
                //    operQueue2DB.UpdateDBbyAPI(arrStr[0], Convert.ToInt32(arrStr[1])); //更新中间库数据
            }
            catch { }
        }

        private void DealQueuewuli(object obj)
        {
            try
            {
                string[] arrStr = (string[])obj;
                OperQueueData2DB operQueue2DB = new OperQueueData2DB(strBBDM);
                if (arrStr[2] == "1") //如果为1 及为call指令
                {
                    operQueue2DB.UpdateCallNumXSL(arrStr[0], arrStr[1]); //把当前窗口为1的标记为0，即，当前窗口正在办理状态的标记为可再次叫号（下次叫号被叫到）
                }
                else
                {
                    operQueue2DB.UpdateStateXSL(arrStr[0], Convert.ToInt32(arrStr[2])); //叫号后标记为1 
                    //    operQueue2DB.UpdateDBbyAPI(arrStr[0], Convert.ToInt32(arrStr[1])); //更新中间库数据
                }

            }
            catch { }
        }
        /// <summary>
        /// 处理数据  异步
        /// </summary>
        /// <param name="obj"></param>
        private void WriteOptDisk(object obj)
        {
            try
            {
                string[] arrStr = (string[])obj;
                Log.WriteOptDisk(arrStr[0], arrStr[1], arrStr[2], arrStr[3], arrStr[4]);
            }
            catch { }
        }

        /// <summary>
        /// 添加身份证信息到191 PDJH_QUEUE_DATA表
        /// </summary>
        /// <param name="obj"></param>
        private void sdnAddCardMsg2Queue(object obj)
        {
            try
            {
                string[] arrStr = (string[])obj;
                new OperQueueData2DB(strBBDM).addCardMsg2QueueData(arrStr[0], arrStr[1]);
            }
            catch { }
        }
        /// <summary>
        /// 异步开始录像
        /// </summary>
        /// 身份证号码 窗口IP  取票时间
        /// <param name="obj"></param>
        private void sdnStartRecVideo(object obj)
        {
            try
            {
                string[] arrStr = (string[])obj;
                new OperQueueData2DB(strBBDM).sdnStartRecVideo(arrStr[0], arrStr[1], arrStr[2]);
            }
            catch { }
        }
        /// <summary>
        /// 异步 结束录像
        /// </summary>
        /// 身份证号码  取票时间
        /// <param name="obj"></param>
        private void sdnEndRecVideo(object obj)
        {
            try
            {
                string[] arrStr = (string[])obj;
                new OperQueueData2DB(strBBDM).sdnEndRecVideo(arrStr[0], arrStr[1]);
            }
            catch { }
        }


        /// <summary>
        /// 语音播放
        /// </summary>
        /// <param name="obj"></param>
        private void call_audioCall(object obj)
        {
            try
            {
                string[] strParameter = obj.ToString().Split(',');
                audio.Call(strParameter[0], strParameter[1]);
            }
            catch
            { }
        }
        #endregion

        #region 向客户端发送信息
        /// <summary>
        /// 处理发送的信息
        /// </summary>
        /// <param name="strSendMsg"></param>
        public void SendMsg2JCX(string strSendMsg, Socket soc)
        {
            try
            {
                byte[] sdnSendByte = string2Bytes_jcx(strSendMsg);
                // SendMsg2Client(sdnSendByte);//向客户端发送信息
                SendMSG(sdnSendByte, soc); //发送信息到指定的客户端
            }
            catch (Exception ex)
            {
            }
        }
        /// <summary>
        ///向app客户端发送信息
        /// </summary>
        /// <param name="bs"></param>
        public void SendMsg2Client(byte[] bs)
        {
            try
            {
                if (m_Client.Send(bs) == 0)
                {
                    this.CloseSocket(m_Client);
                }
            }
            catch
            {
                this.CloseSocket(m_Client);
            }
        }

        //根据传递的数据和套接字，把数据发送到套接字上
        public void SendMSG(byte[] bs, Socket soc)
        {
            try
            {
                if (soc.Send(bs) == 0)
                {
                    this.CloseSocket(soc);
                }
            }
            catch (SocketException)
            {
                this.CloseSocket(soc);
            }
        }
        /// <summary>
        /// 命令
        /// </summary>
        /// <param name="strHPHM"></param>
        /// <param name="strHPZL"></param>
        /// <param name="strDetail"></param>
        /// <returns></returns>
        private string GetCmdXmlMsg(string strType)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("<?xml version=\"1.0\" encoding=\"GB2312\"?>");
                sb.Append("<ret type=\"" + strType + "\" />");
                return sb.ToString();
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region 服务控制

        /// <summary>
        /// 开启服务
        /// </summary>
        public void Start()
        {
            Thread thread = new Thread(new ThreadStart(this.StratThead));
            thread.IsBackground = true;
            thread.Start();
        }
        /// <summary>
        /// 关闭服务
        /// </summary>
        public void Stop()
        {
            try
            {
                try
                {
                    this.m_Server.Shutdown(SocketShutdown.Both);
                }
                catch
                {
                }
            }
            finally
            {
                this.m_Server.Close();
            }
        }

        //开始线程
        public void StratThead()
        {
            try
            {   //定义服务器线程，绑定IP,Port 并设置监听
                this.m_Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint localEP = new IPEndPoint(IPAddress.Parse(this.m_SVRIP), this.m_SVRPort);
                this.m_Server.Bind(localEP);
                this.m_Server.Listen(50);
                while (this.m_IsRun)
                {
                    this.m_MySet.Reset();
                    //把负责监听的socket和负责通信的socket关联起来
                    this.m_Server.BeginAccept(new AsyncCallback(this.AcceptCallBack), this.m_Server);
                    this.m_MySet.WaitOne();
                }
            }
            catch
            {
                this.Stop();
            }
        }
        /// <summary>
        /// 关闭套接字
        /// </summary>
        /// <param name="client"></param>
        public void CloseSocket(Socket client)
        {
            try
            {
                try
                {
                    client.Shutdown(SocketShutdown.Both);
                }
                catch
                {
                }
            }
            finally
            {
                client.Close();
            }
        }
        /// <summary>
        /// 得到xml类型的返回值
        /// </summary>
        /// <param name="strType"></param>
        /// <param name="strstate"></param>
        /// <param name="strDetail"></param>
        /// <returns></returns>
        private string GetXmlMsg(string strType, string strstate, string strDetail)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("<?xml version=\"1.0\" encoding=\"GB2312\"?>");
                sb.Append("<ret type=\"" + strType + "\" state=\"" + strstate + "\" detail=\"" + strDetail + "\"/>");
                return sb.ToString();
            }
            catch
            {
                return null;
            }
        }
        #endregion


        #region 物理叫号 串口

        /// <summary>
        /// 初始化串口
        /// </summary>
        public void InitComPort()
        {
            //打开串口
            try
            {
                var components = new System.ComponentModel.Container();
                this.serialPortCaller = new System.IO.Ports.SerialPort(components);
                ReadIniFile readRoadexam = new Common.ReadIniFile(AppDomain.CurrentDomain.BaseDirectory + "configs\\sdnSystem.ini");
                string strComName = readRoadexam.ReadValue("comname", "value"); //串口名称
                string strSpeed = readRoadexam.ReadValue("comspeed", "value");//串口波特率
                string stopbit = readRoadexam.ReadValue("stopbit", "value");//串口停止位
                if (serialPortCaller.IsOpen)
                {
                    serialPortCaller.Close();
                }
                else
                {
                    serialPortCaller.PortName = strComName;//com1
                    serialPortCaller.BaudRate = int.Parse(strSpeed);//波特率
                    serialPortCaller.StopBits = (StopBits)Enum.Parse(typeof(Parity), stopbit);
                    serialPortCaller.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(comPort_DataReceived);
                    serialPortCaller.Open();//打开串口
                    //打开串口成功，前台页面要不要显示？？

                    Log.WriteOptDisk("串口初始化成功");
                }
            }
            catch (Exception ex)
            {
                Log.WriteOptDisk("串口初始化异常【error】" + ex.Message + ex.StackTrace);
            }
        }
        /// <summary>
        /// 串口接收到
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            try
            {
                string msg = "";
                Thread.Sleep(1500);
                int iByteCount = serialPortCaller.BytesToRead;
                if (iByteCount <= 1)
                    return;
                byte[] arrReadBuffer = new byte[iByteCount];
                serialPortCaller.Read(arrReadBuffer, 0, iByteCount); //得到串口的所有字节数
                string strValue = "";
                foreach (byte by in arrReadBuffer)
                {
                    strValue += by.ToString("X2");
                }
                string calladdr = strValue.Substring(11, 1);
                string strType = strValue.Substring(15, 1);
                if (strType == "C")
                {
                    strType = "call";
                }
                else if (strType == "D")
                {
                    strType = "recall";
                }
                else if (strType == "F")
                {
                    strType = "dook";
                }
                else if (strType == "A")
                {
                    strType = "here";
                }
                cmdRec(strType, calladdr, out msg);
                string strRec = arrReadBuffer[1].ToString("X2");//得到相应的字符串数据,并转换成16进制
                Log.WriteOptDisk(strType);
            }
            catch (Exception ex)
            {
                Log.WriteOptDisk("串口接收信息异常【error】" + ex.Message + ex.StackTrace);
            }
        }


        #endregion

        public static Dictionary<string, string> xmlRead(string strXml, string node, string attribute_dis, string value_dis, string attribute)
        {
            Dictionary<string, string> dic_attr_value = new Dictionary<string, string>();
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(strXml);
                XmlNodeList xns = doc.SelectNodes(node);
                if (string.IsNullOrEmpty(attribute_dis))
                    foreach (XmlNode xn in xns)
                    {
                        if (string.IsNullOrEmpty(attribute))
                        {
                            XmlAttributeCollection attrs = xn.Attributes;
                            foreach (XmlAttribute at in attrs)
                            {
                                dic_attr_value.Add(at.Name, at.Value);
                            }
                        }
                        else
                        {
                            dic_attr_value.Add(attribute, xn.Attributes[attribute].Value);
                        }
                    }
                else
                    foreach (XmlNode xn in xns)
                    {
                        if (((XmlElement)xn).GetAttribute(attribute_dis) == value_dis)
                        {
                            if (string.IsNullOrEmpty(attribute))
                            {
                                XmlAttributeCollection attrs = xn.Attributes;
                                foreach (XmlAttribute at in attrs)
                                {
                                    dic_attr_value.Add(at.Name, at.Value);
                                }
                            }
                            else
                            {
                                dic_attr_value.Add(attribute, xn.Attributes[attribute].Value);
                            }
                        }
                    }

            }
            catch { }
            return dic_attr_value;
        }

        /// <summary>
        /// 将字符串转换成数组，前八个字节是信息长度
        /// </summary>
        /// <param name="strMsg"></param>
        /// <returns></returns>
        public static byte[] string2Bytes_jcx(string strMsg)
        {
            byte[] arr_buff = Encoding.Default.GetBytes(strMsg);
            uint MSGLength = (uint)arr_buff.Length;
            byte[] array = new byte[MSGLength + 8];
            byte[] buffer2 = Encoding.Default.GetBytes(MSGLength + "");
            int dstOffset = 0;
            buffer2.CopyTo(array, 0);
            dstOffset += 8;
            Buffer.BlockCopy(arr_buff, 0, array, dstOffset, (int)MSGLength);
            return array;
        }

        private string ToCmdXmlMsg(string strCardNo, string strCardNumber, string strCount)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("<?xml version=\"1.0\" encoding=\"GB2312\"?>");
                sb.Append("<diagram cardno=\"" + strCardNo + "\" cardnumber=\"" + strCardNumber + "\" count=\"" + strCount + "\" />");
                return sb.ToString();
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 扬州排队取票
        /// </summary>
        /// <param name="strCardNo"></param>
        /// <param name="strCardNumber"></param>
        /// <param name="strCount"></param>
        /// <param name="xm"></param>
        /// <param name="bmdm"></param>
        /// <param name="qpxxxlh"></param>
        /// <returns></returns>
        private string ToCmdXmlMsg(string strCardNo, string strCardNumber, string strCount, string xm, string bmdm, string qpxxxlh)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("<?xml version=\"1.0\" encoding=\"GB2312\"?>");
                sb.Append("<diagram cardno=\"" + strCardNo + "\" cardnumber=\"" + strCardNumber + "\" count=\"" + strCount + "\" xm=\"" + xm + "\" bmdm=\"" + bmdm + "\" qpxxxlh=\"" + qpxxxlh + "\"/>");
                return sb.ToString();
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 单独一个线程进行叫号语音播放
        /// </summary>
        private void call_audio()
        {
            try
            {
                while (true)
                {
                    if (callinfos != null && callinfos.Count > 0)
                    {
                        string str = callinfos.ToArray()[0];
                        audio.Call(str.Split(',')[0], str.Split(',')[1]);
                        callinfos.Remove(str);
                        Thread.Sleep(800);
                    }
                    else
                    {
                        Thread.Sleep(1000);
                    }
                }
            }
            catch
            { }
        }


        /// <summary>
        /// 单独一个线程进行叫号语音播放
        /// </summary>
        private void call_audiowuli()
        {
            try
            {
                while (true)
                {
                    if (callinfowuli != null && callinfowuli.Count > 0)
                    {
                        string str = callinfowuli.ToArray()[0];
                        audio.Call(str.Split(',')[0], str.Split(',')[1]);
                        callinfowuli.Remove(str);
                        //a.Abort();

                        Thread.Sleep(800);
                    }
                    else
                    {
                        Thread.Sleep(1000);
                    }
                }
            }
            catch
            { }
        }
    }
}