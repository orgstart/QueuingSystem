using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace QueuingSystem
{
    public class ftp_download
    {
        /// <summary>  
        /// 下载方法KO  
        /// </summary>  
        /// <param name="ftpads">FTP路径</param>  
        /// <param name="name">需要下载文件路径</param>  
        /// <param name="Myads">保存的本地路径</param>
        public void downftp(string strUri, string strLocalPath) //(string ftpads, string name, string Myads)
        {
            try
            {
                string downloadDir = strLocalPath;
                if (!Directory.Exists(downloadDir))
                {
                    Directory.CreateDirectory(downloadDir);
                }
                string[] fullname = ftp(strUri, WebRequestMethods.Ftp.ListDirectoryDetails);
                string[] onlyname = ftp(strUri, WebRequestMethods.Ftp.ListDirectory);
                if (!Directory.Exists(downloadDir))
                {
                    Directory.CreateDirectory(downloadDir);
                }
                foreach (string names in fullname)
                {
                    //判断是否具有文件夹标识<DIR>  
                    if (names.Contains("<DIR>"))
                    {
                        string olname = names.Split(new string[] { "<DIR>" },
                        StringSplitOptions.None)[1].Trim();
                        downftp(strUri + "/" + olname, downloadDir + @"\" + olname);
                    }
                    else
                    {
                        foreach (string onlynames in onlyname)
                        {
                            if (onlynames == "" || onlynames == " " || names == "")
                            {
                                break;
                            }
                            else
                            {
                                if (names.Contains(" " + onlynames))
                                {
                                    download(downloadDir + @"\" + onlynames, strUri + "/" +
                                    onlynames);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //throw ex;
            }
        }
        /// <summary>  
        /// 单个文件下载方法  
        /// </summary>  
        /// <param name="adss">保存文件的本地路径</param>  
        /// <param name="ftpadss">下载文件的FTP路径</param>  
        public void download(string adss, string ftpadss)
        {
            try
            {
                //FileMode常数确定如何打开或创建文件,指定操作系统应创建新文件。  
                //FileMode.Create如果文件已存在，它将被改写  
                FileStream outputStream = new FileStream(adss, FileMode.Create);
                FtpWebRequest downRequest = (FtpWebRequest)WebRequest.Create(new Uri(ftpadss));
                //设置要发送到 FTP 服务器的命令  
                downRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                FtpWebResponse response = (FtpWebResponse)downRequest.GetResponse();
                Stream ftpStream = response.GetResponseStream();
                long cl = response.ContentLength;
                int bufferSize = 2048;
                int readCount;
                byte[] buffer = new byte[bufferSize];
                readCount = ftpStream.Read(buffer, 0, bufferSize);
                while (readCount > 0)
                {
                    outputStream.Write(buffer, 0, readCount);
                    readCount = ftpStream.Read(buffer, 0, bufferSize);
                }
                ftpStream.Close();
                outputStream.Close();
                response.Close();
            }
            catch (Exception ex)
            {
                //  throw ex;
            }
        }
        /// </summary>  
        /// <param name="strUri">ftp完全路径</param>
        /// <returns></returns>  
        public string[] ftp(string strUri, string type)
        {
            try
            {
                WebResponse webresp = null;
                StreamReader ftpFileListReader = null;
                FtpWebRequest ftpRequest = null;
                try
                {
                    ftpRequest = (FtpWebRequest)WebRequest.Create(new Uri(strUri));
                    ftpRequest.Method = type;
                    webresp = ftpRequest.GetResponse();
                    ftpFileListReader = new StreamReader(webresp.GetResponseStream(), Encoding.Default);
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }
                StringBuilder str = new StringBuilder();
                string line = ftpFileListReader.ReadLine();
                while (line != null)
                {
                    str.Append(line);
                    str.Append("|");
                    line = ftpFileListReader.ReadLine();
                }
                string[] fen = str.ToString().Split('|');
                return fen;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
