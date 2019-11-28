
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace WinControlServer
{
    class TcpSocket
    {
        public delegate void dlgtTcpSocketValue(string strCount, string queNo, string IdNum, string strName, string bmdm, string qpxxxlh);
        public event dlgtTcpSocketValue eventTcpSocketValue;

        Socket socket = null;
        bool isRun = false;
        public string calladdr { get; set; }
        public string strLocalIp { get; set; } //本机IP

        public void ConnServer(object localIP)
        {
            try
            {
                ReadIniFile read = new ReadIniFile(AppDomain.CurrentDomain.BaseDirectory + @"config\sdnsystem.ini");
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ipaddress = IPAddress.Parse(localIP.ToString());
                IPEndPoint port = new IPEndPoint(ipaddress, 0);
                this.socket.Bind(port);
                try
                {
                    this.socket.Connect(IPAddress.Parse(read.ReadValue("sdnServer", "Ip")), Convert.ToInt32(read.ReadValue("sdnServer", "Port")));
                    isRun = true;
                    new Thread(RecData).Start();
                }
                catch
                {
                    MessageBox.Show("叫号系统未运行，请在叫号系统运行后重启该程序", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch
            { }
        }
        /// <summary>
        /// 接收数据
        /// </summary>
        private void RecData()
        {
            try
            {
                byte[] arrRecData = new byte[1024 * 2];
                while (isRun && this.socket.Connected)
                {
                    int recCount = this.socket.Receive(arrRecData);
                    if (recCount > 0)
                    {
                        string strRecDataLength = Encoding.Default.GetString(arrRecData, 0, 8);
                        string strRecData = Encoding.Default.GetString(arrRecData, 8, recCount - 8);

                        // if (strRecData.Length == Convert.ToInt32(strRecDataLength.Trim()))
                        // {
                        //将接收到的信息转换成字典
                        Dictionary<string, string> dics = xmlRead(strRecData, "diagram", null, null, null);
                        //string strCount = dics["count"]; //总排队人数
                        //string strCardNo = dics["cardno"]; //编号 如 A001
                        //string strCardNumber = dics["cardnumber"]; //身份证号 如 3207241990.........
                        eventTcpSocketValue(dics["count"], dics["cardno"], dics["cardnumber"],dics["xm"],dics["bmdm"],dics["qpxxxlh"]);
                        //   }
                    }
                    Thread.Sleep(10);
                }
            }
            catch
            { }
        }
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="strType">指令类型</param>
        public void SendData(string strType, string strKey)
        {
            try
            {
                if (isRun)
                {
                    string strMsg = ToCmdXmlMsg(strType, strKey);
                    // byte aar = Convert.ToByte(8);
                    socket.Send(string2Bytes_jcx(strMsg));
                }
            }
            catch
            { }
        }
        /// <summary>
        /// 命令
        /// </summary>
        /// <param name="strType">命令类型 如叫号 重叫</param>
        /// <param name="strKey">编号 如 A001</param>
        /// <returns></returns>
        private string ToCmdXmlMsg(string strType, string strKey)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("<?xml version=\"1.0\" encoding=\"GB2312\"?>");
                sb.Append("<diagram type=\"" + strType + "\" calladdr=\"" + calladdr + "\" queueno=\"" + strKey + "\" winip=\"" + strLocalIp + "\" />");
                return sb.ToString();
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 将字符串转换成数组，前八个字节是信息长度
        /// </summary>
        /// <param name="strMsg"></param>
        /// <returns></returns>
        private byte[] string2Bytes_jcx(string strMsg)
        {
            byte[] arr_buff = Encoding.Default.GetBytes(strMsg);
            uint MSGLength = (uint)arr_buff.Length;
            byte[] array = new byte[MSGLength + 8];
            // byte[] buffer2 = BitConvert.uint2Bytes(MSGLength);
            byte[] buffer2 = Encoding.Default.GetBytes(MSGLength + "");
            int dstOffset = 0;
            buffer2.CopyTo(array, 0);
            dstOffset += 8;
            Buffer.BlockCopy(arr_buff, 0, array, dstOffset, (int)MSGLength);
            return array;
        }

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
    }
}
