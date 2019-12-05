using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Queue_Show_TV.httpServer
{
    public class HttpProcessor
    {
        public TcpClient socket;
        public HttpServer srv;
        private Stream inputStream;
        public StreamWriter outputStream;
        public String http_method;
        public String http_url;
        public String http_protocol_versionstring;
        public Hashtable httpHeaders = new Hashtable();
        private static int MAX_POST_SIZE = 10 * 1024 * 1024; // 10MB

        public HttpProcessor(TcpClient s, HttpServer srv)
        {
            this.socket = s;
            this.srv = srv;
        }

        /// <summary>
        /// 逐行读取http 输入流
        /// </summary>
        /// <param name="inputStream"></param>
        /// <returns></returns>
        private string streamReadLine(Stream inputStream)
        {
            int next_char;
            string data = "";
            while (true)
            {
                next_char = inputStream.ReadByte();
                if (next_char == '\n') { break; }
                if (next_char == '\r') { continue; }
                if (next_char == -1) { Thread.Sleep(1); continue; };
                data += Convert.ToChar(next_char);
            }
            return data;
        }
        public void process()
        {
            try
            {
                inputStream = new BufferedStream(socket.GetStream());
                // 这里不能使用StreamWriter 输出所有socket的流数据？？？
                outputStream = new StreamWriter(new BufferedStream(socket.GetStream()));
                try
                {
                    parseRequest(); //解析请求数据
                    readHeaders(); //解析请求头文件
                    if (http_method.Equals("GET"))
                    {
                        handleGETRequest(); //处理get时间
                    }
                    else if (http_method.Equals("POST"))
                    {
                        handlePOSTRequest(); //处理POST事件
                    }
                }
                catch (Exception e)
                {
                    //  Console.WriteLine("Exception: " + e.ToString());
                    writeFailure();
                }
                outputStream.Flush();
                // bs.Flush(); 
                inputStream = null; outputStream = null; // bs = null;            
                socket.Close();
            }
            catch { }
        }
        /// <summary>
        /// 解析请求参数
        /// </summary>
        public void parseRequest()
        {
            String request = streamReadLine(inputStream);
            string[] tokens = request.Split(' ');
            if (tokens.Length != 3)
            {
                throw new Exception("invalid http request line");
            }
            http_method = tokens[0].ToUpper(); //得到请求方法
            http_url = tokens[1]; //请求路径
            http_protocol_versionstring = tokens[2]; //版本信息

            // Console.WriteLine("starting: " + request);
        }

        /// <summary>
        /// 读取请求头信息
        /// </summary>
        public void readHeaders()
        {
            // Console.WriteLine("readHeaders()");
            String line;
            while ((line = streamReadLine(inputStream)) != null)
            {
                if (line.Equals(""))
                {
                    //得到 头信息
                    // Console.WriteLine("got headers");
                    return;
                }

                int separator = line.IndexOf(':');
                if (separator == -1)
                {
                    throw new Exception("invalid http header line: " + line);
                }
                String name = line.Substring(0, separator);
                int pos = separator + 1;
                while ((pos < line.Length) && (line[pos] == ' '))
                {
                    pos++; // 去除空格部分
                }

                string value = line.Substring(pos, line.Length - pos);
                //   Console.WriteLine("header: {0}:{1}", name, value);
                httpHeaders[name] = value; //得到头文件信息 并保存到 hashtable httpHeaders中
            }
        }
        /// <summary>
        /// 处理Get请求
        /// </summary>
        public void handleGETRequest()
        {
            srv.handleGETRequest(this); //继承基类的方法实现
        }

        private const int BUF_SIZE = 4096;
        /// <summary>
        /// 处理POST  请求
        /// </summary>
        public void handlePOSTRequest()
        {
            //POST请求数据处理只是将所有内容读入内存流中。这对小数据来说是好的，但是对于大数据我们应该将一个输入流交给请求处理器。但是，输入流
            //我们要让他在这个内容上看到“流的结尾”在内容长度，因为否则他将不知道什么时候结束
            //  Console.WriteLine("get post data start");
            int content_len = 0;
            MemoryStream ms = new MemoryStream();
            if (this.httpHeaders.ContainsKey("Content-Length")) //如果头文件有内容长度，即有内容
            {
                content_len = Convert.ToInt32(this.httpHeaders["Content-Length"]);
                if (content_len > MAX_POST_SIZE)
                {
                    throw new Exception(String.Format("POST Content-Length({0}) too big for this simple server", content_len));
                }
                byte[] buf = new byte[BUF_SIZE];
                int to_read = content_len;
                while (to_read > 0)
                {
                    // Console.WriteLine("starting Read, to_read={0}", to_read);

                    int numread = this.inputStream.Read(buf, 0, Math.Min(BUF_SIZE, to_read));
                    //   Console.WriteLine("read finished, numread={0}", numread);
                    if (numread == 0)
                    {
                        if (to_read == 0)
                        {
                            break;
                        }
                        else
                        {
                            throw new Exception("client disconnected during post");
                        }
                    }
                    to_read -= numread;
                    ms.Write(buf, 0, numread);
                }
                ms.Seek(0, SeekOrigin.Begin);
            }
            // Console.WriteLine("get post data end");
            srv.handlePOSTRequest(this, new StreamReader(ms));

        }
        /// <summary>
        /// 成功
        /// </summary>
        public void writeSuccess()
        {
            outputStream.WriteLine("HTTP/1.0 200 OK");
            outputStream.WriteLine("Content-Type: text/html");
            outputStream.WriteLine("Connection: close");
            outputStream.WriteLine("");
            outputStream.WriteLine("ok");
        }
        /// <summary>
        /// 失败
        /// </summary>
        public void writeFailure()
        {
            outputStream.WriteLine("HTTP/1.0 404 File not found");
            outputStream.WriteLine("Connection: close");
            outputStream.WriteLine("");
            outputStream.WriteLine("fail");
        }
    }

}
