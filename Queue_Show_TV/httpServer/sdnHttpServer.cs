using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Queue_Show_TV.httpServer
{
    public class sdnHttpServer : HttpServer
    {
        private bool m_IsRun;
        private byte[] m_MSG = new byte[0x10000];
        private ManualResetEvent m_MySet = new ManualResetEvent(false);// ManualResetEvent 允许线程通过发信号互相通信
        private string m_SVRIP;
        private int m_SVRPort;

        public delegate void del_update_show_queue(string strjson);
        public event del_update_show_queue event_up_tv_queue;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="port"></param>
        public sdnHttpServer(string svrIP, int svrPort)
            : base(svrPort)
        {
            this.m_SVRIP = svrIP;
            this.m_SVRPort = svrPort;
            this.m_IsRun = true;
        }

        /// <summary>
        ///处理get请求
        /// </summary>
        /// <param name="p"></param>
        public override void handleGETRequest(HttpProcessor p)
        {
            try
            {
                p.writeSuccess();//直接返回成功
            }
            catch
            {

            }
          
        }

        /// <summary>
        /// 处理post请求
        /// </summary>
        /// <param name="p"></param>
        /// <param name="inputData"></param>
        public override void handlePOSTRequest(HttpProcessor p, StreamReader inputData)
        {
            try
            {
                string data = inputData.ReadToEnd(); //获取传入的数据
                                                     //数据格式如下
                                                     //{"count":12,"done":[{"que_no":"1005","win_no":"1"},{"que_no":"1004","win_no":"1"},{"que_no":"1003","win_no":"1"},{"que_no":"1002","win_no":"1"},{"que_no":"1001","win_no":"1"}],"wait":"1006,1007,1008,1009"}
                switch (p.http_url)
                {
                    case "/update": //叫号 更新
                        new Task(() => {
                            string strFun = "update_queue('" + data + "')";
                            parsReqJson(strFun);
                        }).Start();
                        break;
                    case "/add": //新增
                        new Task(() => {
                            string strFun = "add_queue('" + data + "')";
                            parsReqJson(strFun);
                        }).Start();
                        break;
                    default:
                        break;
                }
                response2Client(p);
            }
            catch 
            {

            }
           
        }

        /// <summary>
        /// 向客户端写数据
        /// </summary>
        private void response2Client(HttpProcessor p)
        {
            p.outputStream.WriteLine("HTTP/1.0 200 OK");
            p.outputStream.WriteLine("Content-Type: text/html");
            p.outputStream.WriteLine("Date:"+DateTime.UtcNow.ToString());
            p.outputStream.WriteLine("Connection: close");
            p.outputStream.WriteLine(""); 
            string strResJson = string.Format("{{\"respCode\":\"{0}\" ,\"respMsg\":\"{1}\",\"respData\":\"{2}\"}}", "200", "成功", "ok");
            p.outputStream.WriteLine(strResJson);
        }

        /// <summary>
        /// 解析处理请求json为固定格式
        /// </summary>
        /// <param name="strJson"></param>
        private void parsReqJson(string strJson)
        {
            try
            {
                event_up_tv_queue(strJson);
            }
            catch { }
        }
    }
}
