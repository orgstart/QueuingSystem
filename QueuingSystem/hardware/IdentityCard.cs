using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace QueuingSystem.hardware
{
    /// <summary>
    /// 身份证识别器dll封装
    /// 单氐楠 
    /// 2014-12-02
    /// </summary>
    public class IdentityCard
    {
        #region API声明封装
        /// <summary>
        /// 打开端口  
        /// </summary>
        /// <param name="iPort">端口号</param>
        /// <returns></returns>
        [DllImport("sdtapi.dll", CallingConvention = CallingConvention.StdCall)]
        static extern int SDT_OpenPort(int iPort);
        /// <summary>
        /// 关闭端口  
        /// </summary>
        /// <param name="iPort">端口号</param>
        /// <returns></returns>
        [DllImport("sdtapi.dll", CallingConvention = CallingConvention.StdCall)]
        static extern int SDT_ClosePort(int iPort);
        /// <summary>
        /// 获取com口波特率 
        /// </summary>
        /// <param name="iPort"></param>
        /// <param name="puiBaudRate"></param>
        /// <returns></returns>
        [DllImport("sdtapi.dll", CallingConvention = CallingConvention.StdCall)]
        static extern int SDT_GetCOMBaud(int iPort, int[] puiBaudRate);

        /// <summary>
        /// 寻找身份证
        /// </summary>
        /// <param name="iPort">usb端口号</param>
        /// <param name="pucManaInfo">无符号字符指针，证/卡芯片管理号，4个字节</param>
        /// <param name="iIfOpen">是否打开0否 1是</param>
        /// <returns>0x9f 找卡成功 0x80 找卡失败</returns>
        [DllImport("sdtapi.dll", CallingConvention = CallingConvention.StdCall)]
        static extern int SDT_StartFindIDCard(int iPort, byte[] pucManaInfo, int iIfOpen);
        /// <summary>
        /// 选择身份证
        /// </summary>
        /// <param name="iPort">usb端口号</param>
        /// <param name="pucManaMsg">无符号字符指针</param>
        /// <param name="iIfOpen">是否打开0否 1是</param>
        /// <returns>0x90 选卡成功 0x81 选卡失败</returns>
        [DllImport("sdtapi.dll", CallingConvention = CallingConvention.StdCall)]
        static extern int SDT_SelectIDCard(int iPort, byte[] pucManaMsg, int iIfOpen);
        /// <summary>
        /// 读取身份证基本信息
        /// </summary>
        /// <param name="iPort">usb端口号</param>
        /// <param name="pucCHMsg">读取到的文字信息</param>
        /// <param name="puiCHMsgLen">文字信息长度</param>
        /// <param name="pucPHMsg">读取到的图片信息</param>
        /// <param name="puiPHMsgLen">图片信息的长度</param>
        /// <param name="iIfOpen">是否打开</param>
        /// <returns>0x90 读固定信息成功 其他 失败</returns>
        [DllImport("sdtapi.dll", CallingConvention = CallingConvention.StdCall)]
        static extern int SDT_ReadBaseMsg(int iPort, byte[] pucCHMsg, ref UInt32 puiCHMsgLen, byte[] pucPHMsg, ref UInt32 puiPHMsgLen, int iIfOpen);

        /// <summary>
        /// wlt图片转码函数
        /// </summary>
        /// <param name="pucManaInfo">wlt文件</param>
        /// <param name="intf">1 RS-232C 2 USB</param>
        /// <returns></returns>
        [DllImport("WltRS.dll", CallingConvention = CallingConvention.StdCall)]
        static extern int GetBmp(string pucManaInfo, int intf);
        #endregion

        #region 声明变量
        //变量声明
        byte[] CardPUCIIN = new byte[255];
        byte[] pucManaMsg = new byte[255];
        byte[] pucCHMsg = new byte[255];
        byte[] pucPHMsg = new byte[3024];
        UInt32 puiCHMsgLen = 0;
        UInt32 puiPHMsgLen = 0;
        int st = 0;
        public int i = 0;

        IDCard sdnidCard = new IDCard(); //实例化身份证信息
        string strCardType = "1";//二代身份证读卡器品牌种类（1：华旭，2：华视）


        #endregion

        #region 系统函数封装

        public IDCard GetBaseMsg(int itemp, out string strMsg)
        {
            // string[] arrRes = new string[2];
            i = itemp;
            sdnGetReadCardType();//获取二代身份证读卡器种类

            try
            {
                switch (strCardType)
                {
                    case "1": //华旭二代身份证读卡器
                        #region 读卡操作
                        int iPort = 1001;
                        //读卡操作
                        st = SDT_OpenPort(iPort);
                        if (st != 0x90)
                        {
                            for (int iLoop = 1002; iLoop <= 1016; iLoop++)
                            {
                                st = SDT_OpenPort(iLoop);
                                if (st == 0x90)
                                {
                                    iPort = iLoop;//把当前usb号传递给iport
                                    break;//跳出循环
                                }
                            }
                        }
                        st = SDT_StartFindIDCard(iPort, CardPUCIIN, 1);
                        if (st != 0x9f)
                        {
                            strMsg = "未找到身份证";
                            return null;
                        }
                        st = SDT_SelectIDCard(iPort, pucManaMsg, 1);
                        if (st != 0x90)
                        {
                            strMsg = "身份证识别错误";
                            return null;
                        }
                        st = SDT_ReadBaseMsg(iPort, pucCHMsg, ref puiCHMsgLen, pucPHMsg, ref puiPHMsgLen, 1); //pucCHMs 读到的文件信息，pucPHMsg 图片信息字节
                        if (st != 0x90)
                        {
                            strMsg = "读取身份证信息错误";
                            return null;
                        }
                        else
                        {
                            #region 处理身份证信息
                            GetName();
                            GetSex();
                            GetNationality();
                            GetBirthday();
                            GetAddress();
                            GetCartNo();
                            GetInstitution();
                            GetBegin_validity();
                            GetEnd_validity();

                            Get_Txzhm();
                            Get_qfcs();
                            Get_zjlxbs();

                            sdnidCard.Photo = getPhotos(pucPHMsg, puiPHMsgLen);
                            #endregion
                        }
                        strMsg = "获取信息成功";
                        return sdnidCard;
                    #endregion

                    case "2"://华视读卡器
                        #region 华视读卡器
                        try
                        {
                            AxIDCARDREADERLib.AxIDCardReader sd = new AxIDCARDREADERLib.AxIDCardReader();
                            sd.BeginInit();//初始化控件
                            sd.CreateControl();
                            string strRes = sd.ReadCard();
                            if (strRes == "0")
                            {
                                sdnidCard.Address = sd.Address;
                                sdnidCard.Begin_validity = sd.EffectedDate;
                                if (sd.Born.Contains("年"))
                                {
                                    sdnidCard.Birthday = sd.Born.Substring(0, 4) + "-" + sd.Born.Substring(5, 2) + "-" + sd.Born.Substring(8, 2);
                                }
                                else
                                {
                                    sdnidCard.Birthday = sd.Born;
                                }
                                sdnidCard.CartNo = sd.CardNo;
                                sdnidCard.End_validity = sd.ExpiredDate;
                                sdnidCard.Institution = sd.IssuedAt;
                                sdnidCard.Name = sd.CtlName;
                                sdnidCard.Nationality = sd.Nation;
                                if (sd.Pic.Contains("\\\\"))
                                    sdnidCard.Photo = sd.Pic.Replace("\\\\", "\\");
                                else
                                    sdnidCard.Photo = sd.Pic;
                                sdnidCard.Sex = sd.Sex;
                                strMsg = "成功";
                                return sdnidCard;
                            }
                            else
                            {
                                strMsg = "无卡、卡片无效或重复读取信息";
                                return null;
                            }

                        }
                        catch (Exception ex)
                        {
                            strMsg = ex.Message;
                            return null;
                        }

                        #endregion
                        break;
                    default:
                        strMsg = "二代身份证读卡器种类错误";
                        return null;
                }

            }
            catch (Exception ex)
            {
                strMsg = ex.Message;
                return null;
            }

        }



        #endregion

        #region 处理得到的身份证信息
        //取姓名
        private void GetName()
        {
            sdnidCard.Name = GetText(0, 15).Trim();
        }
        //取性别
        private void GetSex()
        {
            string result = "";
            string text = GetText(15, 1);
            if ("9".Equals(text))
                result = "未说明";
            if ("1".Equals(text))
                result = "男";
            if ("2".Equals(text))
                result = "女";
            if ("0".Equals(text))
                result = "未知";
            sdnidCard.Sex = result;
            // return result.Trim();
        }
        //取民族
        private void GetNationality()
        {
            //try
            //{
            //    string[] nationality ={"汉","蒙古","回","藏","维吾尔","苗","彝","壮","布依",
            //                      "朝鲜","满","侗","瑶","白","土家","哈尼","哈萨克","傣","黎","傈僳","佤","畲","高山","拉祜",
            //                      "水","东乡","纳西","景颇","柯尔克孜","土","达斡尔","仫佬","羌","布朗","撒拉","毛南","仡佬",
            //                      "锡伯","阿昌","普米","塔吉克","怒","乌孜别克","俄罗斯","鄂温克","德昂","保安","裕固","京",
            //                      "塔塔尔","独龙","鄂伦春","赫哲","门巴","珞巴","基诺","其他"};
            //    int index = int.Parse(GetText(16, 2));
            //    if (index == 0)
            //    {
            //        sdnidCard.Nationality = "未知";
            //    }
            //    else if (index < 57)
            //    {
            //        sdnidCard.Nationality = nationality[index - 1];
            //    }
            //    else
            //    {
            //        sdnidCard.Nationality = "其他";
            //    }
            //}
            //catch
            //{
            //    sdnidCard.Nationality = "未知";
            //}
            try
            {
                string[] nationality ={"汉","蒙古","回","藏","维吾尔","苗","彝","壮","布依",
                                  "朝鲜","满","侗","瑶","白","土家","哈尼","哈萨克","傣","黎","傈僳","佤","畲","高山","拉祜",
                                  "水","东乡","纳西","景颇","柯尔克孜","土","达斡尔","仫佬","羌","布朗","撒拉","毛南","仡佬",
                                  "锡伯","阿昌","普米","塔吉克","怒","乌孜别克","俄罗斯","鄂温克","德昂","保安","裕固","京",
                                  "塔塔尔","独龙","鄂伦春","赫哲","门巴","珞巴","基诺","其他","革家","穿青"};
                int index = int.Parse(GetText(16, 2));
                if (index == 0)
                {
                    sdnidCard.Nationality = "未知";
                }
                else if (index < 60)
                {
                    sdnidCard.Nationality = nationality[index - 1];
                }
                else
                {
                    sdnidCard.Nationality = "其他";
                }
            }
            catch
            {
                sdnidCard.Nationality = "未知";
            }

        }
        //取生日
        private void GetBirthday()
        {
            string text = GetText(18, 8);
            string result = text.Substring(0, 4) + '-' + text.Substring(4, 2) + '-' + text.Substring(6, 2);
            sdnidCard.Birthday = result;
        }
        //取家庭住址
        private void GetAddress()
        {
            sdnidCard.Address = GetText(26, 35).Trim();
        }
        //取身份证号
        private void GetCartNo()
        {
            sdnidCard.CartNo = GetText(61, 18).Trim();
        }
        //取发证机关
        private void GetInstitution()
        {
            sdnidCard.Institution = GetText(79, 15).Trim();
        }
        // 有效期开始
        private void GetBegin_validity()
        {
            string text = GetText(94, 8);
            string result = text.Substring(0, 4) + '-' + text.Substring(4, 2) + '-' + text.Substring(6, 2);
            sdnidCard.Begin_validity = result;
        }
        //有效期结束
        private void GetEnd_validity()
        {
            string text = GetText(102, 8);
            string result = "";
            if (!"长期".Equals(text))
            {
                result = text.Substring(0, 4) + '-' + text.Substring(4, 2) + '-' + text.Substring(6, 2);
            }
            else if (!text.Contains("长期"))
            {
                result = text.Substring(0, 4) + '-' + text.Substring(4, 2) + '-' + text.Substring(6, 2);
            }
            sdnidCard.End_validity = result;
        }
        /// <summary>
        /// 获取通信证号码
        /// </summary>
        private void Get_Txzhm()
        {
            try
            {
                string text = GetText(110, 9);
                sdnidCard.txzhm = text;
            }
            catch
            {
                sdnidCard.txzhm = "";
            }
        }
        /// <summary>
        /// 得到签发次数
        /// </summary>
        private void Get_qfcs()
        {
            try
            {
                string strQFCS = GetText(119, 2);
                sdnidCard.qfcs = strQFCS;
            }
            catch
            {
                sdnidCard.qfcs = "";
            }
        }
        /// <summary>
        /// 证件类型标识
        /// </summary>
        private void Get_zjlxbs()
        {
            try
            {
                string strZJLXBS = GetText(124, 1);
                sdnidCard.zjlxbs = strZJLXBS;
            }
            catch
            {
                sdnidCard.zjlxbs = "";
            }
        }

        private string GetText(int startIndex, int length)
        {
            string strContent = ASCIIEncoding.Unicode.GetString(pucCHMsg);
            return strContent.Substring(startIndex, length);
        }
        #endregion

        #region 处理之前的图片信息
        public string getPhotos(byte[] wltBytes, uint byetLength)
        {
            try
            {
                // int i = 1;
                string path = AppDomain.CurrentDomain.BaseDirectory + @"Photos";
                //  string filename = DateTime.Now.ToString("yyyyMMddHHmmss");
                if (!File.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string newpath = "";
                if (i % 2 == 0)
                {
                    newpath = path + @"\sdnIDcard.wlt";
                }
                else
                {
                    newpath = path + @"\sdnIDcard~.wlt";
                }
                //   i++;
                //   string bmpPath = path + @"\sdnIDcard.bmp";
                //if (File.Exists(bmpPath))
                //{
                //    File.Delete(bmpPath);
                //}
                if (!File.Exists(newpath))
                {
                    File.CreateText(newpath).Close();
                }
                FileStream fs = new FileStream(newpath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite, 2048);
                fs.Write(wltBytes, 0, (int)byetLength);
                if (fs != null)
                {
                    fs.Close();
                }
                int fl = GetBmp(newpath, 2); //得到转码后的bmp图片路径
                if (fl != 1)
                {
                    switch (fl)
                    {
                        case 0:
                            return "调取sdtapi.dll 失败";
                        case -1:
                            return "相片解码错误";
                        case -2:
                            return "wlt文件后缀错误";
                        case -3:
                            return "wlt文件打开错误";
                        case -4:
                            return "wlt文件格式错误";
                        case -5:
                            return "文件未授权";
                        case -6:
                            return "连接设备错误";

                    }
                }
                string bmpPath = newpath.Substring(0, newpath.Length - 3) + "bmp";
                return bmpPath;
            }
            catch (Exception ex)
            {
                return ex.Message;
                //  MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region 获取身份证读卡器品牌

        private void sdnGetReadCardType()
        {
            try
            {
                string strPath = AppDomain.CurrentDomain.BaseDirectory + @"config\config.ini";
                if (!string.IsNullOrEmpty(strPath))
                {
                    operConfig.ReadIniFile sdnReadIni = new operConfig.ReadIniFile(strPath);

                    strCardType = sdnReadIni.ReadValue("CardType", "typenum");//得到读卡器的类型
                    strCardType = "1";
                }

            }
            catch
            { }
        }

        #endregion
    }
    #region 身份证基本信息类
    /// <summary>
    /// 身份证基本信息类
    /// </summary>
    public class IDCard
    {
        public string Name;//姓名
        public string Sex;//性别
        public string CartNo; //身份证号
        public string Nationality; //民族
        public string Birthday; //生日
        public string Address;//家庭住址
        public string Institution;//发证机关
        public string Begin_validity;// 有效期开始
        public string End_validity;//有效期结束
        public string Photo;//照片;
        // public string NewAddress;//最新住址
        public string txzhm;//通行证号码
        public string qfcs;//签发次数
        public string zjlxbs;//证件类型标识 （台胞证 大写J）
    }
    #endregion

}
