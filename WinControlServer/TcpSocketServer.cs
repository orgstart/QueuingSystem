using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml;

namespace WinControlServer
{
    public class TcpSocketServer
    {
        #region 自定义变量

        private bool m_IsRun;
        private byte[] m_MSG = new byte[0x10000];
        private ManualResetEvent m_MySet = new ManualResetEvent(false);// ManualResetEvent 允许线程通过发信号互相通信
        private Socket m_Server;
        private Socket m_Client;
        private string m_SVRIP;
        private int m_SVRPort;
        /// <summary>
        /// 打开浏览器窗口
        /// </summary>
        /// <param name="strCardNo"></param>
        /// <param name="strBMDM"></param>
        public delegate void delOpenWin(string strCardNo, string strBMDM, string strQueSN);
        public event delOpenWin eventOpenWin;
        public delegate void delSetQueue(string queuenum, string cardnum, string xm,string count);
        public event delSetQueue eventSetQueue;


        #endregion

        /// <summary>
        /// TCPserver构造函数
        /// </summary>
        /// <param name="svrIP"></param>
        /// <param name="svrPort"></param>
        /// <param name="program_id"></param>
        public TcpSocketServer(string svrIP)
        {
            this.m_SVRIP = svrIP; //本机IP
            this.m_SVRPort = 5888;//本机服务端口
            this.m_IsRun = true;//
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
                    string strBMDM = "1";//窗口号
                    string cardNum = "";//身份证号码
                    string strQueSN = "";//24位取票序列号

                    switch (strType)
                    {
                        case "jh": //评价
                            string queuenum = dicXmlValues["queuenum"]; //cardnumber
                            string cardnum = dicXmlValues["cardnum"]; //身份证号
                            string xm = dicXmlValues["xm"];  //取票序列号
                            string count = dicXmlValues["count"];  //排队数
                            eventSetQueue(queuenum, cardnum, xm,count);//打开对应的窗口
                            break;
                        case "sdnpj": //评价
                            strBMDM = dicXmlValues["bmdm"]; //cardnumber
                            cardNum = dicXmlValues["cardnumber"]; //身份证号
                            strQueSN = dicXmlValues["serialnum"];  //取票序列号
                            eventOpenWin(cardNum, strBMDM, strQueSN);//打开对应的窗口
                            break;
                        case "sdnqm"://签名
                            strBMDM = dicXmlValues["bmdm"]; //部门代码
                            cardNum = dicXmlValues["cardnumber"];//身份证号
                            strQueSN = dicXmlValues["serialnum"];  //取票序列号
                            eventOpenWin(cardNum, strBMDM, strQueSN);//打开对应的窗口
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

        #region 处理收发数据

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

        #endregion

    }
}
