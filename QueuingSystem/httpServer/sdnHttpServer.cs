using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QueuingSystem.OperQueue;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QueuingSystem.httpServer
{
    public class sdnHttpServer : HttpServer
    {
        #region 定义变量
        private bool m_IsRun;
        private byte[] m_MSG = new byte[0x10000];
        private ManualResetEvent m_MySet = new ManualResetEvent(false);// ManualResetEvent 允许线程通过发信号互相通信
        private string m_SVRIP;
        private int m_SVRPort;
        private string com;//COM口
        private string bps;//波特率
        private string cardip1;//综合屏1
        private string cardip2;//综合屏2
        private string ZHPServerIp;//综合屏控制卡IP
        private HttpClient.sdnHttpWebRequest sdnHttpClient;//http客户端
        /// <summary>
        /// 根据关键字得到相应的QueueItem
        /// </summary>
        /// <param name="strKey"></param>
        /// <returns></returns>
        public delegate QueueItem dlgGetItemByKey(string strKey);
        public event dlgGetItemByKey eventGetItemByKey;

        /// <summary>
        /// 获取队列中的第一项,并更新当前使用此号的窗口IP和窗口号
        /// </summary>
        /// <returns></returns>  
        public delegate QueueItem dlgtGetQueueItem(string strWinIp, string strWinNum);
        public event dlgtGetQueueItem eventGetQueueItem;
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
        /// 添加补传信息到队列
        /// </summary>
        /// <param name="strWin"></param>
        /// <param name="strGLBM"></param>
        public delegate void dlgAddBCqueue(string strWin, string strGLBM);
        public event dlgAddBCqueue eventAddBCqueue;
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
        /// 根据业务窗口IP得到相应的业务窗口的窗口号
        /// </summary>
        /// <param name="strIp"></param>
        /// <returns></returns>
        public delegate string dlgGetWin_Num(string strIp);
        public event dlgGetWin_Num eventGetWinNum;
        /// <summary>
        /// 根据IP 检测暂停状态
        /// </summary>
        /// <param name="strIp"></param>
        public delegate bool dlgCheckPauseState(string strIp);
        public event dlgCheckPauseState eventCheckPauseState;
        /// <summary>
        /// 回滚
        /// </summary>
        /// <param name="strPDH"></param>
        public delegate void dlgGoBack(string strPDH);
        public event dlgGoBack eventGoBack;
        /// <summary>
        /// 发布信息委托
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="msg"></param>
        public delegate void dlgPub_MSG(string channel, string msg);
        public event dlgPub_MSG event_pub_msg;

        /// <summary>
        /// 控制暂停
        /// </summary>
        /// <param name="iControl"></param>
        /// <param name="strJSJIP"></param>
        public delegate void dlgControlCall(string iControl, string strJSJIP);
        public event dlgControlCall eventControlCall;

        hardware.CallNumberAudio audio = new hardware.CallNumberAudio("wavFiles"); //播放语音实体类
        public string windownum { get; set; } //窗口号
        public string strBBDM { get; set; } //部门代码
        public string strJH { get; set; } //经办人警号，需要在配置参数中配置
        /// <summary>
        /// 叫号信息
        /// </summary> 
        List<string> callinfos = new List<string>(); //叫号音频信息存放
        List<string> callinfoss = new List<string>();//吴江盛泽
        List<string> list_Done = new List<string>();//叫号信息（正在或者已经叫号)
        List<string> list_yz_led = new List<string>();//扬州综合屏
        Dictionary<string, string> dic_tv_ip = new Dictionary<string, string>();
        Dictionary<string, DateTime> dic_call_time = new Dictionary<string, DateTime>();//记录叫号时间,叫号窗口IP-时间，间隔5s内第二次不处理
                                                                                        //  Dictionary<string, DateTime> dic_skip_time = new Dictionary<string, DateTime>();//记录跳号时间，当前序列号-时间，间隔5s内第二次不处理
        bool bLEDShow = false;//条屏
        bool bZHPShow = false;//综合屏
        bool bLED2 = false;//综合屏
        private int i_ShowType = 0;//显示种类
        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="port"></param>
        public sdnHttpServer(string svrIP, int svrPort, int program_id)
            : base(svrPort)
        {
            this.m_SVRIP = svrIP;
            this.m_SVRPort = svrPort;
            this.m_IsRun = true;
            new Thread(call_audio).Start(); //单独线程进行叫号语音播放
            i_ShowType = program_id;

            operConfig.ReadIniFile rnd = new operConfig.ReadIniFile(AppDomain.CurrentDomain.BaseDirectory + "configs\\Tiaopinconfig.ini");
            com = rnd.ReadValue("comname", "value"); //串口名称
            bps = rnd.ReadValue("comspeed", "value"); //波特率
            cardip1 = rnd.ReadValue("cardip1", "value"); //一号卡IP
            cardip2 = rnd.ReadValue("cardip2", "value"); //二号卡IP
            ZHPServerIp = rnd.ReadValue("ZHPServerIp", "value"); //二号卡IP
            try
            {
                for (int i = 1; i <= 7; i++)
                {
                    dic_tv_ip.Add("" + i, rnd.ReadValue("androidtv", "tv" + i));
                }
            }
            catch { }
            sdnHttpClient = new HttpClient.sdnHttpWebRequest(); //实例化http客户端
        }

        #region 响应HTTP请求

        /// <summary>
        /// 处理GET请求
        /// </summary>
        /// <param name="p"></param>
        public override void handleGETRequest(HttpProcessor p)
        {
            p.writeSuccess(); //返回成功
        }
        /// <summary>
        /// 处理POST请求
        /// </summary>
        /// <param name="p"></param>
        /// <param name="inputData"></param>
        public override void handlePOSTRequest(HttpProcessor p, StreamReader inputData)
        {
            string data = inputData.ReadToEnd(); //获取传入的数据

            p.outputStream.WriteLine("HTTP/1.0 200 OK");
            p.outputStream.WriteLine("Content-Type: text/html");
            p.outputStream.WriteLine("Connection: close");
            p.outputStream.WriteLine("");
            string strMsg = "";
            cmdStartRec(p, data);
        }
        /// <summary>
        /// 向客户端写数据
        /// </summary>
        private void write2Client(HttpProcessor p, string strJson)
        {
            //   byte[] bytes = Encoding.Default.GetBytes(strJson);
            //   string base64Json=Convert.ToBase64String(bytes);
            string strResJson = string.Format("{{\"respCode\":\"{0}\" ,\"respMsg\":\"{1}\",\"respData\":{2}}}", "200", "成功", strJson);
            Common.SysLog.WriteOptDisk(strResJson, AppDomain.CurrentDomain.BaseDirectory);
            p.outputStream.WriteLine(strResJson);
        }

        #endregion

        #region 根据命令处理
        /// <summary>
        /// 开始检测命令处理
        /// </summary>
        /// <param name="strXml"></param>
        /// <param name="strMsg"></param>
        /// <returns></returns>
        public void cmdStartRec(HttpProcessor httpPro, string reqData)
        {
            try
            {


                Common.SysLog.WriteOptDisk("开始处理命令：：" + reqData, AppDomain.CurrentDomain.BaseDirectory); //记录日志

                if (!string.IsNullOrWhiteSpace(reqData))
                {
                    JObject jo = (JObject)JsonConvert.DeserializeObject(reqData);
                    string strType = jo.GetValue("opType").ToString(); //取票类型
                    string jsonReqdata = jo.GetValue("reqdata").ToString();
                    string charset = jo.GetValue("charset").ToString();
                    Common.SysLog.WriteOptDisk("开始处理命令,解析json：：" + strType + "::::" + jsonReqdata, AppDomain.CurrentDomain.BaseDirectory); //记录日志
                    //  string strType = dicXmlValues["type"];
                    string strCall_addr = "1";//窗口号
                    string res = string.Empty;
                    //  Dictionary<string, string> dicReqData = parseReqData(jsonReqdata);

                    //  JToken jt = jo.GetValue("reqdata");
                    // string ts = jo["reqdata"]["ywckjsjip"].ToString();
                    // Common.SysLog.WriteOptDisk("解析到正确的IP22：：" + ts, AppDomain.CurrentDomain.BaseDirectory); //记录日志
                    jsonReqdata = jsonReqdata.Replace(@"\n", "").Replace(@"\r", ""); //去除字符串中的换行
                    JObject joReqData = (JObject)JsonConvert.DeserializeObject(jsonReqdata);

                    switch (strType)
                    {

                        case "TMRI_CALLOUT": //叫号
                            //  string strWindowsIp = dicReqData["ywckjsjip"];//得到业务窗口IP
                            string strWindowsIp = joReqData.GetValue("ywckjsjip").ToString();
                            //   Common.SysLog.WriteOptDisk("解析到正确的IP：：" + strWindowsIp , AppDomain.CurrentDomain.BaseDirectory); //记录日志
                            if (eventCheckPauseState(strWindowsIp)) //当前计算机是否暂停业务
                            {
                                string strJsonTemp = string.Format("{{\"sbkzjsjip\":\"{0}\",\"qhxxxlh\":\"{1}\",\"pdh\":\"{2}\",\"ywlb\":\"04\",\"sfzmhm\":\"{3}\",\"dlrsfzmhm\":\"\",\"qhrxm\":\"\",\"qhsj\":\"{4}\",\"rylb\":\"1\"}}", this.m_SVRIP, "", "", "", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                write2Client(httpPro, strJsonTemp); //给客户端返回值
                                break;
                            }

                            try
                            {
                                strBBDM = joReqData.GetValue("glbm").ToString();
                                strCall_addr = eventGetWinNum(strWindowsIp);
                                res = (256 + Convert.ToInt32(strCall_addr)).ToString();
                            }
                            catch
                            {
                                res = "1";
                            }
                            int iCall = 0;//多次发送到LED
                            QueueItem p = eventGetQueueItem(strWindowsIp, strCall_addr);  //获取队列最新项

                            string strJson = "{}";

                            if (p == null) //如果没有号，返回空
                            {//如果没有排队信息，则记录当前叫号信息并返回空给综合平台
                                eventAddBCqueue(strWindowsIp, strBBDM);
                                strJson = string.Format("{{\"sbkzjsjip\":\"{0}\",\"qhxxxlh\":\"{1}\",\"pdh\":\"{2}\",\"ywlb\":\"04\",\"sfzmhm\":\"{3}\",\"dlrsfzmhm\":\"\",\"qhrxm\":\"\",\"qhsj\":\"{4}\",\"rylb\":\"1\"}}", this.m_SVRIP, "", "", "", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                write2Client(httpPro, strJson); //给客户端返回值
                                break;
                            }
                            else
                            {
                                string count = eventGetQueueCount(); //得到当前排队总数
                                //  SendMsg2JCX(ToCmdXmlMsg(p.msgQueueNo, p.msgCardNo, count), soc); //返回前台数据
                                strJson = string.Format("{{\"sbkzjsjip\":\"{0}\",\"qhxxxlh\":\"{1}\",\"pdh\":\"{2}\",\"ywlb\":\"04\",\"sfzmhm\":\"{3}\",\"dlrsfzmhm\":\"\",\"qhrxm\":\"\",\"qhsj\":\"{4}\",\"rylb\":\"1\"}}", this.m_SVRIP, p.serialNum, "00" + p.msgQueueNo, p.msgCardNo, p.strqhsj);
                            }


                            if (httpPro == null) //如果为空，则没有通过叫号，直接补传信息
                            {
                                try
                                {
                                    strJson = string.Format("{{\"sbkzjsjip\":\"{0}\",\"qhxxxlh\":\"{1}\",\"pdh\":\"{2}\",\"ywlb\":\"04\",\"sfzmhm\":\"{3}\",\"dlrsfzmhm\":\"\",\"qhrxm\":\"\",\"qhsj\":\"{4}\",\"rylb\":\"1\"}}", this.m_SVRIP, "", "", "", DateTime.Now.ToString());
                                    // strJson = "{}";
                                    //插入补传信息到综合平台
                                    new OperQueueData2DB(strBBDM).writeBCQH(strBBDM, "", "", "", strWindowsIp, m_SVRIP, p.serialNum, "00" + p.msgQueueNo, "04", p.msgCardNo, "", p.msgName, p.strqhsj, "1", strJH);
                                }
                                catch { }
                            }
                            else
                            {
                                try
                                {
                                    write2Client(httpPro, strJson); //给客户端返回值
                                }
                                catch
                                { }
                            }

                            //  eventRemoveQueueItem(p.msgQueueNo);  //测试 叫号时移除该项
                            //3.调用音频
                            callinfos.Add(p.msgQueueNo + "," + strCall_addr); //语音叫号用
                            callinfoss.Insert(0, ZHPServerIp);
                            callinfoss.Insert(1, p.msgQueueNo + "," + strCall_addr);
                            //{ "que_no":"1005","win_no":"1"}
                            list_Done.Insert(0, $"{{ \"que_no\":\"{p.msgQueueNo}\",\"win_no\":\"{strCall_addr}\"}}");
                            string str_yz_led_json = "{{\"ip1\":\"{0}\",\"ip2\":\"{1}\",\"queno\":\"{2}\",\"winno\":\"{3}\"}}";
                            str_yz_led_json = string.Format(str_yz_led_json, cardip1, cardip2, p.msgQueueNo, strCall_addr);
                            list_yz_led.Add(str_yz_led_json);
                            event_pub_msg("sdnsound", $"{{\"queue\":\"{p.msgQueueNo}\",\"winnum\":\"{strCall_addr}\"}}");
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
                                        new Thread(ZongHeShowMsg.ZongheShow.CLEDSender.sendData2LEDYXC_NEW).Start(callinfoss);
                                        break;
                                    case 5:
                                        new Thread(ShowMsg.LEDshow.sendData2LED).Start(new string[] { com, bps, res, "请" + p.msgQueueNo + "到" + strCall_addr + "窗口" });
                                        new Thread(ZongHeShowMsg.ZongheShow.CLEDSender.sendData2LEDYXC_NEW).Start(callinfoss);
                                        break;
                                    case 6:
                                        //while (iCall < 3) //发送三次到LED条屏
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
                                        break;
                                    case 8:// 园区特殊屏幕
                                        //new Thread(ZongHeShowMsg.ZongheShow.CLEDSender.updateData2LEDYQCGS).Start(strCall_addr + "," + p.msgQueueNo);
                                        break;
                                    case 9://双综合屏（扬州） cardip1 //cardip2
                                        new Thread(ZongHeShowMsg.ZongheShow.CLEDSender.sdnSendData2ZHP_yz).Start(list_yz_led);
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
                                        try
                                        {
                                            string sdn_tv_url = $"http://{dic_tv_ip[strCall_addr]}:8888/queue?queue={p.msgQueueNo}";
                                            sdnHttpClient.DoGet(sdn_tv_url);
                                        }
                                        catch { }
                                        break;
                                    default: //默认
                                        break;

                                }
                                new Thread(DealQueue).Start(new string[] { p.serialNum, "1", strCall_addr }); //更新本地数据库中的数据, 更新排队状态为1
                                new Thread(WriteOptDisk).Start(new string[] { strCall_addr, res, com, bps, p.msgQueueNo });//记录日志
                              //  new Thread(sdnAddCardMsg2Queue).Start(new string[] { strWindowsIp, p.serialNum });//上传身份证信息到业务数据库
                              //  new Thread(sdnStartRecVideo).Start(new string[] { p.msgCardNo, strWindowsIp, p.strqhsj });//开始录像
                            }
                            catch { }
                            break;
                        case "TMRI_RECALL": //重叫
                            try
                            {
                                //string qhxxxlh = dicReqData["qhxxxlh"];//得到22位取票序列号
                                string qhxxxlh = joReqData.GetValue("qhxxxlh").ToString();
                                string strWindowsIp_recall = joReqData.GetValue("jsjip").ToString();
                                strCall_addr = eventGetWinNum(strWindowsIp_recall);
                                //   Common.SysLog.WriteOptDisk("解析到正确的取票信息：" + qhxxxlh, AppDomain.CurrentDomain.BaseDirectory); //记录日志
                                //  QueueItem sdnTemp = 
                                //  string res = string.Empty;
                                string strKey = "";
                                QueueItem sdnTemp = eventGetItemByKey(qhxxxlh);
                                try
                                {
                                    //  strCall_addr = sdnTemp.windowNum; //
                                    strKey = sdnTemp.msgQueueNo;//得到排队号码
                                    res = (256 + Convert.ToInt32(strCall_addr)).ToString();
                                }
                                catch { }

                                int iRecall = 0;//多次发送信息到LED条屏
                                //  string strKey = dicXmlValues["queueno"];
                                string strJson1 = string.Format("{{\"code\":\"{0}\",\"message\":\"{1}\",\"pdh\":\"{2}\"}}", "1", "成功", "00" + sdnTemp.msgQueueNo);
                                write2Client(httpPro, strJson1); //给客户端返回值
                                if (!string.IsNullOrWhiteSpace(strKey)) //如果 strKey不为空 即有号码
                                {
                                    callinfos.Add(strKey + "," + strCall_addr);
                                    event_pub_msg("sdnsound", $"{{\"queue\":\"{strKey}\",\"winnum\":\"{strCall_addr}\"}}");
                                }
                                else //如果没有号 直接返回 六合一数据后 跳出
                                {
                                    break;
                                }

                                //系统种类 0：无显示 1:双屏  2：电视盒子  3：综合屏  4：综合屏+LED条屏1  5：综合屏+LED条屏2  6：LED条屏1  7：LED条屏2
                                Common.SysLog.WriteOptDisk("条屏显示：：" + com + "-" + bps + "-" + strCall_addr, AppDomain.CurrentDomain.BaseDirectory); //记录日志
                                switch (i_ShowType)
                                {
                                    case 3://只有综合屏
                                        new Thread(ZongHeShowMsg.ZongheShow.CLEDSender.sendData2LEDYXC1).Start(callinfoss);
                                        break;
                                    case 4:
                                        new Thread(ShowMsg.LEDshow.sendData2LEDYXC).Start(new string[] { com, bps, "请" + strKey + "到" + strCall_addr + "窗口", strCall_addr });
                                        new Thread(ZongHeShowMsg.ZongheShow.CLEDSender.sendData2LEDYXC_NEW).Start(callinfoss);
                                        break;
                                    case 5:
                                        new Thread(ShowMsg.LEDshow.sendData2LED).Start(new string[] { com, bps, res, "请" + strKey + "到" + strCall_addr + "窗口" });
                                        new Thread(ZongHeShowMsg.ZongheShow.CLEDSender.sendData2LEDYXC_NEW).Start(callinfoss);
                                        break;
                                    case 6:
                                        //while (iRecall < 3)
                                        //{
                                        new Thread(ShowMsg.LEDshow.sendData2LEDYXC).Start(new string[] { com, bps, "请" + strKey + "到" + strCall_addr + "窗口", strCall_addr });
                                        //    iRecall++;
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
                                        new Thread(ShowMsg.LEDshow.sendData2LED).Start(new string[] { com, bps, res, "请" + strKey + "到" + strCall_addr + "窗口" });
                                        break;
                                    case 8:// 园区特殊屏幕
                                        //new Thread(ZongHeShowMsg.ZongheShow.CLEDSender.updateData2LEDYQCGS).Start(strCall_addr + "," + strKey);
                                        break;
                                    case 9://双综合屏（扬州） cardip1 //cardip2
                                        try
                                        {
                                            new Thread(ShowMsg.LEDshow.sendData2LEDYXC).Start(new string[] { com, bps, "请" + strKey + "到" + strCall_addr + "窗口", strCall_addr });
                                        }
                                        catch { }
                                        try
                                        {
                                            new Thread(ZongHeShowMsg.ZongheShow.CLEDSender.sdnSendData2ZHP_yz).Start(list_yz_led);
                                        }
                                        catch { }
                                        break;
                                    default: //默认
                                        break;

                                }
                            }
                            catch { }
                            break;
                        case "TMRI_COMPLETE": //完成
                            try
                            {  //更新状态为5（人员到达，但不知道完成结果）
                                //  string qhxxxlh = joReqData.GetValue("qhxxxlh").ToString();
                                string strYWckip = joReqData.GetValue("ywckjsjip").ToString();//得到控制窗口IP
                                Common.SysLog.WriteOptDisk("解析到正确的取票信息：" + strYWckip, AppDomain.CurrentDomain.BaseDirectory); //记录日志
                                //  QueueItem sdnTemp = 
                                //  string res = string.Empty;
                                string strKey1 = "A001";
                                QueueItem sdnTemp = eventFindQueueByIP(strYWckip);
                                try
                                {
                                    strCall_addr = sdnTemp.windowNum; //
                                    //strKey1 = sdnTemp.msgQueueNo;//得到排队号码
                                    strKey1 = sdnTemp.serialNum;
                                    //  res = (256 + Convert.ToInt32(strCall_addr)).ToString();
                                }
                                catch { }
                                string strJson1 = string.Format("{{\"code\":\"{0}\",\"message\":\"{1}\"}}", 1, "成功");
                                write2Client(httpPro, strJson1); //给客户端返回值
                                //string strKey1 = dicXmlValues["queueno"];
                                eventUpdateQueue(strKey1, 2);//更新队列
                                new Thread(DealQueue).Start(new string[] { strKey1, "2", strCall_addr }); //正在办理2
                             //   new Thread(sdnEndRecVideo).Start(new string[] { sdnTemp.msgCardNo, sdnTemp.strqhsj });//结束录像
                                //new Thread(WriteOptDisk).Start(new string[] { strCall_addr, res, com, bps, strKey1 });
                                try
                                {//发送信息到签名功能
                                    //string strSendMsg = ToCmdXmlMsg(strBBDM, sdnTemp.serialNum, sdnTemp.msgCardNo, "sdnqm");
                                    //new sdnTcpClient(sdnTemp.windowIp, 5888).send2Server(strSendMsg);
                                }
                                catch { }

                            }
                            catch { }
                            break;
                        case "TMRI_SKIP": //跳号
                            try
                            {
                                string qhxxxlh = joReqData.GetValue("qhxxxlh").ToString();
                                string strWindowsIp_skip = joReqData.GetValue("jsjip").ToString();
                                strCall_addr = eventGetWinNum(strWindowsIp_skip);
                                Common.SysLog.WriteOptDisk("解析到正确的取票信息：" + qhxxxlh, AppDomain.CurrentDomain.BaseDirectory); //记录日志
                                //  QueueItem sdnTemp = 
                                //  string res = string.Empty;
                                string strKey2 = "";
                                QueueItem sdnTemp = eventGetItemByKey(qhxxxlh);

                                try
                                {
                                    // strCall_addr = sdnTemp.windowNum; //
                                    //  strKey2 = sdnTemp.msgQueueNo;//得到排队号码
                                    strKey2 = sdnTemp.serialNum;
                                    //  res = (256 + Convert.ToInt32(strCall_addr)).ToString();
                                    eventUpdateQueue(strKey2, 4);//更新队列
                                    new Thread(DealQueue).Start(new string[] { strKey2, "4", strCall_addr });//终止办理4
                                    //new Thread(sdnEndRecVideo).Start(new string[] { sdnTemp.msgCardNo, sdnTemp.strqhsj });//结束录像
                                    ////**********************以上为跳号处理，*********以下为跳号后重新叫号***********************
                                }
                                catch
                                {
                                    string strJson_sdn = string.Format("{{\"sbkzjsjip\":\"{0}\",\"qhxxxlh\":\"{1}\",\"pdh\":\"{2}\",\"ywlb\":\"04\",\"sfzmhm\":\"{3}\",\"dlrsfzmhm\":\"\",\"qhrxm\":\"\",\"qhsj\":\"{4}\",\"rylb\":\"1\"}}", this.m_SVRIP, "", "", "", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                    write2Client(httpPro, strJson_sdn); //给客户端返回值
                                    break;
                                }

                                string strWindowsIp1 = sdnTemp.windowIp; //得到窗口ip
                                //   Common.SysLog.WriteOptDisk("解析到正确的IP：：" + strWindowsIp , AppDomain.CurrentDomain.BaseDirectory); //记录日志
                                try
                                {
                                    strBBDM = sdnTemp.bmdm; //管理部门
                                                            //   strCall_addr = eventGetWinNum(strWindowsIp1);
                                    res = (256 + Convert.ToInt32(strCall_addr)).ToString();
                                }
                                catch
                                {
                                    res = "1";
                                }
                                int iCall1 = 0;//多次发送到LED
                                QueueItem p1 = eventGetQueueItem(strWindowsIp1, strCall_addr);  //获取队列最新项
                                string strJson1 = "{}";
                                if (p1 == null) //如果没有号，不给返回值
                                {//如果没有排队信息，则记录当前叫号信息即可
                                    eventAddBCqueue(strWindowsIp1, strBBDM);
                                    strJson1 = string.Format("{{\"sbkzjsjip\":\"{0}\",\"qhxxxlh\":\"{1}\",\"pdh\":\"{2}\",\"ywlb\":\"04\",\"sfzmhm\":\"{3}\",\"dlrsfzmhm\":\"\",\"qhrxm\":\"\",\"qhsj\":\"{4}\",\"rylb\":\"1\"}}", this.m_SVRIP, "", "", "", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                    write2Client(httpPro, strJson1); //给客户端返回值
                                    break;
                                }

                                //  SendMsg2JCX(ToCmdXmlMsg(p.msgQueueNo, p.msgCardNo, count), soc); //返回前台数据
                                strJson1 = string.Format("{{\"sbkzjsjip\":\"{0}\",\"qhxxxlh\":\"{1}\",\"pdh\":\"{2}\",\"ywlb\":\"04\",\"sfzmhm\":\"{3}\",\"dlrsfzmhm\":\"\",\"qhrxm\":\"\",\"qhsj\":\"{4}\",\"rylb\":\"1\"}}", this.m_SVRIP, p1.serialNum, "00" + p1.msgQueueNo, p1.msgCardNo, p1.strqhsj);
                                if (httpPro == null) //如果为空，则没有通过叫号，直接补传信息
                                {
                                    try
                                    {
                                        strJson1 = string.Format("{{\"sbkzjsjip\":\"{0}\",\"qhxxxlh\":\"{1}\",\"pdh\":\"{2}\",\"ywlb\":\"04\",\"sfzmhm\":\"{3}\",\"dlrsfzmhm\":\"\",\"qhrxm\":\"\",\"qhsj\":\"{4}\",\"rylb\":\"1\"}}", this.m_SVRIP, "", "", "", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                        // strJson1 = "{}";
                                        //插入补传信息到综合平台
                                        new OperQueueData2DB(strBBDM).writeBCQH(strBBDM, "", "", "", strWindowsIp1, m_SVRIP, p1.serialNum, "00" + p1.msgQueueNo, "04", p1.msgCardNo, "", p1.msgName, p1.strqhsj, "1", strJH);
                                    }
                                    catch { }
                                }
                                try
                                {
                                    write2Client(httpPro, strJson1); //给客户端返回值
                                }
                                catch
                                { }
                                //  eventRemoveQueueItem(p.msgQueueNo);  //测试 叫号时移除该项
                                //3.调用音频
                                callinfos.Add(p1.msgQueueNo + "," + strCall_addr); //语音叫号用
                                callinfoss.Insert(0, ZHPServerIp);
                                callinfoss.Insert(1, p1.msgQueueNo + "," + strCall_addr);
                                list_Done.Insert(0, $"{{ \"que_no\":\"{p1.msgQueueNo}\",\"win_no\":\"{strCall_addr}\"}}");
                                string str_yz_led_json_1 = "{{\"ip1\":\"{0}\",\"ip2\":\"{1}\",\"queno\":\"{2}\",\"winno\":\"{3}\"}}";
                                str_yz_led_json_1 = string.Format(str_yz_led_json_1, cardip1, cardip2, p1.msgQueueNo, strCall_addr);
                                list_yz_led.Add(str_yz_led_json_1);
                                event_pub_msg("sdnsound", $"{{\"queue\":\"{p1.msgQueueNo}\",\"winnum\":\"{strCall_addr}\"}}");
                                try
                                {
                                    //系统种类 0：无显示 1:双屏  2：电视盒子  3：综合屏  4：综合屏+LED条屏1  5：综合屏+LED条屏2  6：LED条屏1  7：LED条屏2
                                    switch (i_ShowType)
                                    {
                                        case 3://只有综合屏
                                            new Thread(ZongHeShowMsg.ZongheShow.CLEDSender.sendData2LEDYXC1).Start(callinfoss);
                                            break;
                                        case 4:
                                            new Thread(ShowMsg.LEDshow.sendData2LEDWJSZ).Start(new string[] { com, bps, "请" + p1.msgQueueNo + "到" + strCall_addr + "窗口", strCall_addr });
                                            new Thread(ZongHeShowMsg.ZongheShow.CLEDSender.sendData2LEDYXC_NEW).Start(callinfoss);
                                            break;
                                        case 5:
                                            new Thread(ShowMsg.LEDshow.sendData2LEDWJSZ).Start(new string[] { com, bps, res, "请" + p1.msgQueueNo + "到" + strCall_addr + "窗口" });
                                            new Thread(ZongHeShowMsg.ZongheShow.CLEDSender.sendData2LEDYXC_NEW).Start(callinfoss);
                                            break;
                                        case 6:
                                            while (iCall1 < 3) //发送三次到LED条屏
                                            {
                                                new Thread(ShowMsg.LEDshow.sendData2LEDYXC).Start(new string[] { com, bps, "请" + p1.msgQueueNo + "到" + strCall_addr + "窗口", strCall_addr });
                                                iCall1++;
                                            }
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
                                            new Thread(ShowMsg.LEDshow.sendData2LED).Start(new string[] { com, bps, res, "请" + p1.msgQueueNo + "到" + strCall_addr + "窗口" });
                                            break;
                                        case 8:// 园区特殊屏幕
                                            //new Thread(ZongHeShowMsg.ZongheShow.CLEDSender.updateData2LEDYQCGS).Start(strCall_addr + "," + p1.msgQueueNo);
                                            break;
                                        case 9://双综合屏（扬州） cardip1 //cardip2
                                            new Thread(ZongHeShowMsg.ZongheShow.CLEDSender.sdnSendData2ZHP_yz).Start(list_yz_led);
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
                                            string str_queueNO_temp = p1.msgQueueNo.Substring(1); //得到排队号的后三位
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
                                            try
                                            {
                                                string sdn_tv_url = $"http://{dic_tv_ip[strCall_addr]}:8888/queue?queue={p1.msgQueueNo}";
                                                sdnHttpClient.DoGet(sdn_tv_url);
                                            }
                                            catch { }
                                            break;
                                        default: //默认
                                            break;

                                    }
                                    new Thread(DealQueue).Start(new string[] { p1.serialNum, "1", strCall_addr }); //更新本地数据库中的数据
                                    new Thread(WriteOptDisk).Start(new string[] { strCall_addr, res, com, bps, p1.msgQueueNo });//记录日志
                                   // new Thread(sdnAddCardMsg2Queue).Start(new string[] { strWindowsIp1, p1.serialNum });//上传身份证信息到业务数据库
                                  //  new Thread(sdnStartRecVideo).Start(new string[] { p1.msgCardNo, strWindowsIp1, p1.strqhsj });//开始录像
                                }
                                catch { }

                                //string strJson2 = string.Format("{{\"ywckjsjip\":\"{5}\",\"sbkzjsjip\":\"{0}\",\"qhxxxlh\":\"{1}\",\"pdh\":\"{2}\",\"ywlb\":\"04\",\"sfzmhm\":\"{3}\",\"dlrsfzmhm\":\"\",\"qhrxm\":\"\",\"qhsj\":\"{4}\",\"rylb\":\"1\"}}", this.m_SVRIP, sdnTemp.serialNum, "00" + sdnTemp.msgQueueNo, sdnTemp.msgCardNo, sdnTemp.strqhsj, sdnTemp.windowIp);
                                //write2Client(httpPro, strJson2); //给客户端返回值
                                //string strKey2 = dicXmlValues["queueno"];

                                //new Thread(WriteOptDisk).Start(new string[] { strCall_addr, res, com, bps, strKey2 });


                            }
                            catch { }
                            break;
                        case "TMRI_EVALUATION": //提请评价
                            try
                            {
                                string qhxxxlh = joReqData.GetValue("qhxxxlh").ToString();
                                Common.SysLog.WriteOptDisk("解析到正确的取票信息：" + qhxxxlh, AppDomain.CurrentDomain.BaseDirectory); //记录日志
                                //  QueueItem sdnTemp = 
                                //  string res = string.Empty;
                                string strKey3 = "A001";
                                QueueItem sdnTemp = eventGetItemByKey(qhxxxlh);
                                try
                                {
                                    strCall_addr = sdnTemp.windowNum; //
                                    // strKey3 = sdnTemp.msgQueueNo;//得到排队号码
                                    strKey3 = sdnTemp.serialNum;
                                    //  res = (256 + Convert.ToInt32(strCall_addr)).ToString();
                                }
                                catch { }
                                string strJson1 = string.Format("{{\"code\":\"{0}\",\"message\":\"{1}\"}}", 1, "成功");
                                write2Client(httpPro, strJson1); //给客户端返回值
                                // string strKey3 = dicXmlValues["queueno"];
                                eventUpdateQueue(strKey3, 5);//更新队列
                                new Thread(DealQueue).Start(new string[] { strKey3, "5", strCall_addr });//完成办理3
                                //new Thread(WriteOptDisk).Start(new string[] { strCall_addr, res, com, bps, strKey3 });
                                //
                                //    new Thread(new OperQueueData2DB(strBBDM).SetPJXX(strBBDM, "", "", "", qhxxxlh, "2", "1")).Start();//默认非常好评

                                //如果有评价器，这里与评价器控制端通信，向其发送开启评价指令
                                //使用TCP socket 向评价器控制服务发送指令，使其操作评价器, 取票系统当作服务端，评价器当作客户端，保持长连接
                                try
                                {
                                    var task1 = new Task(() =>
                                    {
                                        new OperQueueData2DB(strBBDM).SetPJXX(strBBDM, "", "", "", qhxxxlh, "5", "1");
                                    });
                                    task1.Start();//开启任务


                                }
                                catch
                                { }

                                //没有评价器直接写入系统默认值

                            }
                            catch { }
                            break;
                        case "TMRI_SUSPEND": //暂停取票
                            try
                            {
                                string strYWckip = joReqData.GetValue("ywckjsjip").ToString();//得到控制窗口IP
                                // eventIsPause("pause");//读取身份证功能不可用 手动输入证件号取票也不可用
                                //系统种类 0：无显示 1:双屏  2：电视盒子  3：综合屏  4：综合屏+LED条屏1  5：综合屏+LED条屏2  6：LED条屏1  7：LED条屏2
                                switch (i_ShowType)
                                {
                                    case 3://只有综合屏
                                        new Thread(ZongHeShowMsg.ZongheShow.CLEDSender.sendData2LEDYXC1).Start(callinfoss);
                                        break;
                                    case 4:
                                        new Thread(ShowMsg.LEDshow.sendData2LEDYXC).Start(new string[] { com, bps, "暂 停 服 务", strCall_addr });
                                        //   new Thread(ZongHeShowMsg.ZongheShow.CLEDSender.sendData2LEDYXC1).Start(callinfoss);
                                        break;
                                    case 5:
                                        new Thread(ShowMsg.LEDshow.sendData2LED).Start(new string[] { com, bps, res, "暂 停 服 务" });
                                        // new Thread(ZongHeShowMsg.ZongheShow.CLEDSender.sendData2LEDYXC1).Start(callinfoss);
                                        break;
                                    case 6:
                                        new Thread(ShowMsg.LEDshow.sendData2LEDYXC).Start(new string[] { com, bps, "暂 停 服 务", strCall_addr });
                                        break;
                                    case 7:
                                        new Thread(ShowMsg.LEDshow.sendData2LED).Start(new string[] { com, bps, res, "暂 停 服 务" });
                                        break;
                                    default: //默认
                                        break;

                                }

                                //new Thread(WriteOptDisk).Start(new string[] { strCall_addr, res, com, bps, "暂停服务" });
                                eventControlCall("0", strYWckip);
                                string strJson1 = string.Format("{{\"code\":\"{0}\",\"message\":\"{1}\"}}", 1, "成功");
                                write2Client(httpPro, strJson1); //给客户端返回值
                            }
                            catch { }
                            break;
                        case "TMRI_RECOVER": //恢复取票
                            try
                            {
                                string strYWckip = joReqData.GetValue("ywckjsjip").ToString();//得到控制窗口IP
                                //eventIsPause("restart"); //恢复暂停的功能
                                switch (i_ShowType)
                                {
                                    case 3://只有综合屏
                                        new Thread(ZongHeShowMsg.ZongheShow.CLEDSender.sendData2LEDYXC1).Start(callinfoss);
                                        break;
                                    case 4:
                                        new Thread(ShowMsg.LEDshow.sendData2LEDYXC).Start(new string[] { com, bps, "开 始 服 务", strCall_addr });
                                        //   new Thread(ZongHeShowMsg.ZongheShow.CLEDSender.sendData2LEDYXC1).Start(callinfoss);
                                        break;
                                    case 5:
                                        new Thread(ShowMsg.LEDshow.sendData2LED).Start(new string[] { com, bps, res, "开 始 服 务" });
                                        // new Thread(ZongHeShowMsg.ZongheShow.CLEDSender.sendData2LEDYXC1).Start(callinfoss);
                                        break;
                                    case 6:
                                        new Thread(ShowMsg.LEDshow.sendData2LEDYXC).Start(new string[] { com, bps, "开 始 服 务", strCall_addr });
                                        break;
                                    case 7:
                                        new Thread(ShowMsg.LEDshow.sendData2LED).Start(new string[] { com, bps, res, "开 始 服 务" });
                                        break;
                                    default: //默认
                                        break;
                                }
                                eventControlCall("1", strYWckip);
                                string strJson1 = string.Format("{{\"code\":\"{0}\",\"message\":\"{1}\"}}", 1, "成功");
                                write2Client(httpPro, strJson1); //给客户端返回值
                            }
                            catch { }
                            break;

                        case "TMRI_RECEIVE": //待领取牌证信息写入
                            break;
                        case "SDN_PAUSE"://暂停取票
                            eventIsPause("pause");//读取身份证功能不可用 手动输入证件号取票也不可用
                            string strJson_pause = string.Format("{{\"code\":\"{0}\",\"message\":\"{1}\"}}", 1, "成功");
                            write2Client(httpPro, strJson_pause); //给客户端返回值
                            break;
                        case "SDN_RECOVERY": //恢复取票
                            eventIsPause("restart"); //恢复暂停的功能
                            string strJson_restart = string.Format("{{\"code\":\"{0}\",\"message\":\"{1}\"}}", 1, "成功");
                            write2Client(httpPro, strJson_restart); //给客户端返回值
                            break;
                        case "SDN_BACK": //回滚
                            string strQueuNo = joReqData.GetValue("pdh").ToString();//得到控制窗口IP
                            eventGoBack(strQueuNo);
                            string strJson_back = string.Format("{{\"code\":\"{0}\",\"message\":\"{1}\"}}", 1, "成功");
                            write2Client(httpPro, strJson_back); //给客户端返回值
                            break;

                        default:
                            break;
                    }
                }
                else
                {
                    //  strMsg = "没有读取到值";
                    Common.SysLog.WriteOptDisk("开始处理命令：：没有读取到值", AppDomain.CurrentDomain.BaseDirectory); //记录日志
                }
            }
            catch (Exception ex)
            {
                // strMsg = ex.Message;
                Common.SysLog.WriteLog(ex, AppDomain.CurrentDomain.BaseDirectory);
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
        /// 解析http传输过来的数据
        /// </summary>
        /// <param name="reqdata"></param>
        /// <returns></returns>
        private Dictionary<string, string> parseReqData(string jsonStr)
        {
            string jsonBase64 = "";
            if (string.IsNullOrEmpty(jsonStr))
            {
                return new Dictionary<string, string>();
            }
            //string[] arrTemp = jsonStr.Split('=');//用等号分割
            //if (arrTemp != null && arrTemp.Length > 1)
            //{
            //    jsonBase64 = arrTemp[1]; //得到base64编码的json数据
            //}
            //byte[] tempJson = Convert.FromBase64String(jsonBase64);
            //string strJosn = Encoding.Default.GetString(tempJson);

            Dictionary<string, string> jsonDict = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonStr);

            return jsonDict;
        }

        #endregion

        #region 录像开始与结束

        #endregion

        #region 格式化传输数据
        /// <summary>
        /// 格式化传输数据格式
        /// </summary>
        /// <param name="strserialnum">24位排队序列号</param>
        /// <param name="strCardNumber">身份证号码</param>
        /// <param name="strtype">类型</param>
        /// <returns></returns>
        private string ToCmdXmlMsg(string strbmdm, string strserialnum, string strCardNumber, string strtype)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("<?xml version=\"1.0\" encoding=\"GB2312\"?>");
                sb.Append("<diagram bmdm=\"" + strbmdm + "\" serialnum=\"" + strserialnum + "\" cardnumber=\"" + strCardNumber + "\" type=\"" + strtype + "\" />");
                return sb.ToString();
            }
            catch
            {
                return null;
            }
        }
        #endregion

        #region 语言报号
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
        #endregion
    }
}
