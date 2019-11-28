using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace WinControlServer
{
    public class TcpClient
    {
        string _strIp = ""; //ip地址
        int _iPort = 5888;//端口
        Socket clientSocket = null;//全局
        /// <summary>
        /// 重构函数
        /// </summary>
        /// <param name="strIp"></param>
        /// <param name="iPort"></param>
        public TcpClient(string strIp, int iPort)
        {
            _strIp = strIp;
            _iPort = iPort;
        }
        public void send2Server(string strMsg)
        {
            try
            {
               // string host = "127.0.0.1";//服务器端ip地址
                IPAddress ip = IPAddress.Parse(_strIp);
                IPEndPoint ipe = new IPEndPoint(ip, _iPort);
                //if (clientSocket != null&&clientSocket.)
                //{
                //    CloseSocket(clientSocket);
                //}
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Connect(ipe);
                
                byte[] sdnSendByte = string2Bytes_jcx(strMsg);
                // SendMsg2Client(sdnSendByte);//向客户端发送信息
                SendMSG(sdnSendByte, clientSocket); //发送信息到指定的客户端
            }
            catch (Exception ex)
            {
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
                }else
                {
                    this.CloseSocket(soc);//关闭套接字
                }
            }
            catch (SocketException)
            {
                this.CloseSocket(soc);
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

    }
}
