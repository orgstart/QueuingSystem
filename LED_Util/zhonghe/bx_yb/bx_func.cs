using Aspose.Cells;
using Aspose.Cells.Rendering;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace LED_Util.zhonghe
{
    /// <summary>
    /// 此类是对BX06SDK常用方法的逻辑代码
    /// 初始化动态库InitSdk--->设置屏幕相关参数program_setScreenParams_G56----->创建节目program_addProgram----->创建区域 program_AddArea----->添加区域内容program_picturesAreaAddTxt---->合成节目program_IntegrateProgramFile---->开始写文件cmd_ofsStartFileTransf----->写文件cmd_ofsWriteFile----->结束写文件cmd_ofsEndFileTransf----->清除节目缓存program_deleteProgram----->释放动态库ReleaseSd
    /// </summary>
    public class bx_func : IDisposable
    {
        #region 日志记录事件
        /// <summary>
        /// 日志记录委托
        /// </summary>
        /// <param name="info"></param>
        public delegate void del_log(string info);
        /// <summary>
        /// 日志记录委托
        /// </summary>
        public event del_log Event_Log;
        #endregion

        #region 方法

        int err = 0;

        /// <summary>
        /// 初始化sdk
        /// </summary>
        public void initSDK()
        {
            err = bx_sdk_dual.InitSdk();
            if (Event_Log != null)
            {
                Event_Log($"初始化BX SDK，代码:{err}");
            }
        }
        /// <summary>
        /// 网络中设备搜索
        /// </summary>
        public void Net_search()
        {
            byte[] arrPointer = new byte[Marshal.SizeOf(typeof(bx_sdk_dual.Ping_data))];
            bx_sdk_dual.Ping_data data;
            int err = bx_sdk_dual.cmd_udpPing(arrPointer);
            IntPtr dec = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(bx_sdk_dual.Ping_data)));
            Marshal.Copy(arrPointer, Marshal.SizeOf(typeof(bx_sdk_dual.Ping_data)) * 0, dec, Marshal.SizeOf(typeof(bx_sdk_dual.Ping_data)));
            data = (bx_sdk_dual.Ping_data)Marshal.PtrToStructure(dec, typeof(bx_sdk_dual.Ping_data));
            Marshal.FreeHGlobal(dec);
            if (Event_Log != null)
            {
                Event_Log($"控制卡类型为：{data.ControllerType.ToString("X2")},firmware版本为：{System.Text.Encoding.Default.GetString(data.FirmwareVersion)}");
                Event_Log($"搜索到的控制卡IP为：{System.Text.Encoding.Default.GetString(data.ipAdder)}");
            }
        }
        /// <summary>
        /// 设置屏幕参数
        /// </summary>
        public void setScreenParams_G56()
        {
            err = bx_sdk_dual.program_setScreenParams_G56(bx_sdk_dual.E_ScreenColor_G56.eSCREEN_COLOR_DOUBLE, 0x374, bx_sdk_dual.E_DoubleColorPixel_G56.eDOUBLE_COLOR_PIXTYPE_1);
            if (Event_Log != null)
            {
                Event_Log($"设置屏幕参数，代码:{err}");
            }
        }
        /// <summary>
        /// 把给定的json数据解析到对应的excle中
        /// </summary>
        /// <param name="strContent">数据格式</param>
        /// {"count":12,"done":[{"que_no":"1005","win_no":"1"},{"que_no":"1004","win_no":"1"},{"que_no":"1003","win_no":"1"},{"que_no":"1002","win_no":"1"},{"que_no":"1001","win_no":"1"}],"wait":"1006,1007,1008,1009"}
        public void writeXLS(string strContent)
        {
            try
            {
                //   string Template_File_Path = @".\wj_led.xlsx";
                string Template_File_Path = @".\Template\wj_led.xlsx";

                //  打开 Excel 模板
                Workbook CurrentWorkbook = File.Exists(Template_File_Path) ? new Workbook(Template_File_Path) : new Workbook();

                //  打开第一个sheet
                Worksheet DetailSheet = CurrentWorkbook.Worksheets[0];
                //得到对应的json数据，定义json格式如下
                //{"count":12,"done":[{"que_no":"1005","win_no":"1"},{"que_no":"1004","win_no":"1"},{"que_no":"1003","win_no":"1"},{"que_no":"1002","win_no":"1"},{"que_no":"1001","win_no":"1"}],"wait":"1006,1007,1008,1009"}
                JObject jobj = (JObject)Newtonsoft.Json.JsonConvert.DeserializeObject(strContent);
                int istart = 5;
                foreach (JObject jo in jobj["done"])
                {
                    DetailSheet.Cells[$"B{istart}"].PutValue($"请{jo["que_no"]}号到{jo["win_no"]}号窗口办理业务");
                    istart++;
                }
                string[] arrWait = jobj["wait"].ToString().Split(','); //得到等待队列
                for (int i = 0; i < arrWait.Length; i++)
                {
                    int iIndex = i / 2 + 5;
                    if (i % 2 == 0) //双数  F
                    {
                        DetailSheet.Cells[$"E{iIndex}"].PutValue(arrWait[i]);
                    }
                    else //单数  E
                    {
                        DetailSheet.Cells[$"F{iIndex}"].PutValue(arrWait[i]);
                    }
                }
                DetailSheet.Cells["E4"].PutValue($"({jobj["count"]}位等待)");

                PageSetup pageSetup = DetailSheet.PageSetup;
                pageSetup.Orientation = PageOrientationType.Portrait;
                pageSetup.LeftMargin = 0.3;
                pageSetup.RightMargin = 0.5;
                pageSetup.BottomMargin = 0.5;
                // pageSetup.TopMargin=1;
                pageSetup.PaperSize = PaperSizeType.Custom;
                pageSetup.PrintArea = "A1:H13";

                //Apply different Image / Print options.
                Aspose.Cells.Rendering.ImageOrPrintOptions options = new Aspose.Cells.Rendering.ImageOrPrintOptions();
                options.OnlyArea = true;
                options.ImageFormat = System.Drawing.Imaging.ImageFormat.Png;
                //Set the Printing page property
                options.PrintingPage = PrintingPageType.IgnoreStyle;
                options.PrintWithStatusDialog = false;
                //Render the worksheet
                SheetRender sr = new SheetRender(DetailSheet, options);

                //System.Drawing.Printing.PrinterSettings printSettings = new System.Drawing.Printing.PrinterSettings();
                //string strPrinterName = printSettings.PrinterName;
                //if (!Directory.Exists(@".\Excel"))
                //    Directory.CreateDirectory(@".\Excel");

                ////  设置执行公式计算 - 如果代码中用到公式，需要设置计算公式，导出的报表中，公式才会自动计算
                //CurrentWorkbook.CalculateFormula(true);

                ////  生成的文件名称
                //string ReportFileName = string.Format("Excel_{0}.xlsx", DateTime.Now.ToString("yyyy-MM-dd"));

                ////  保存文件
                //CurrentWorkbook.Save(@".\Excel\" + ReportFileName, SaveFormat.Xlsx);
                //send to printer
                sr.ToImage(0, "sstest.png");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        /// <summary>
        /// 创建节目
        /// </summary>
        public void create_Program()
        {
            bx_sdk_dual.EQprogramHeader_G6 header;
            header.FileType = 0x00;
            header.ProgramID = 0;
            header.ProgramStyle = 0x00;
            header.ProgramPriority = 0x00;
            header.ProgramPlayTimes = 1;
            header.ProgramTimeSpan = 0;
            header.SpecialFlag = 0;
            header.CommExtendParaLen = 0x00;
            header.ScheduNum = 0;
            header.LoopValue = 0;
            header.Intergrate = 0x00;
            header.TimeAttributeNum = 0x00;
            header.TimeAttribute0Offset = 0x0000;
            header.ProgramWeek = 0xff;
            header.ProgramLifeSpan_sy = 0xffff;
            header.ProgramLifeSpan_sm = 0x03;
            header.ProgramLifeSpan_sd = 0x14;
            header.ProgramLifeSpan_ey = 0xffff;
            header.ProgramLifeSpan_em = 0x03;
            header.ProgramLifeSpan_ed = 0x14;
            header.PlayPeriodGrpNum = 0;
            IntPtr aa = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(bx_sdk_dual.EQprogramHeader_G6)));
            Marshal.StructureToPtr(header, aa, false);
            err = bx_sdk_dual.program_addProgram_G6(aa);
            if (Event_Log != null)
            {
                Event_Log($"创建节目，代码:{err}");
            }
        }
        /// <summary>
        /// 创建区域
        /// </summary>
        /// <param name="AreaType">区域类型</param>
        /// <param name="x">左上角x坐标</param>
        /// <param name="y">左上角y坐标</param>
        /// <param name="w">宽度</param>
        /// <param name="h">高度</param>
        /// <param name="areaID">区域ID</param>
        public void Creat_Area(byte AreaType, ushort x, ushort y, ushort w, ushort h, ushort areaID)
        {
            bx_sdk_dual.EQareaHeader_G6 aheader;
            aheader.AreaType = AreaType;
            aheader.AreaX = x;
            aheader.AreaY = y;
            aheader.AreaWidth = w;
            aheader.AreaHeight = h;
            aheader.BackGroundFlag = 0x00;
            aheader.Transparency = 101;
            aheader.AreaEqual = 0x00;
            bx_sdk_dual.EQSound_6G stSoundData = new bx_sdk_dual.EQSound_6G();
            stSoundData.SoundFlag = 0;
            stSoundData.SoundVolum = 0;
            stSoundData.SoundSpeed = 0;
            stSoundData.SoundDataMode = 0;
            stSoundData.SoundReplayTimes = 0;
            stSoundData.SoundReplayDelay = 0;
            stSoundData.SoundReservedParaLen = 0;
            stSoundData.Soundnumdeal = 0;
            stSoundData.Soundlanguages = 0;
            stSoundData.Soundwordstyle = 0;
            stSoundData.SoundDataLen = 0;
            byte[] t = new byte[1];
            t[0] = 0;
            stSoundData.SoundData = t;
            aheader.stSoundData = stSoundData;
            IntPtr bb = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(bx_sdk_dual.EQareaHeader_G6)));
            Marshal.StructureToPtr(aheader, bb, false);
            err = bx_sdk_dual.program_addArea_G6(areaID, bb);  //添加图文区域
            if (Event_Log != null)
            {
                Event_Log($"创建图文区域，代码:{err}");
            }
        }
        /// <summary>
        ///添加边框
        /// </summary>
        /// <param name="areaID"></param>
        public void AreaAddFrame(ushort areaID)
        {
            bx_sdk_dual.EQareaframeHeader afheader;
            afheader.AreaFFlag = 0x01;
            afheader.AreaFDispStyle = 0x01;
            afheader.AreaFDispSpeed = 0x08;
            afheader.AreaFMoveStep = 0x01;
            afheader.AreaFWidth = 3;
            afheader.AreaFBackup = 0;
            byte[] img = Encoding.Default.GetBytes("黄10.png");
            bx_sdk_dual.program_picturesAreaAddFrame(areaID, ref afheader, img);
        }
        /// <summary>
        /// 添加内容
        /// </summary>
        /// <param name="areaID">区域ID</param>
        /// <param name="content">显示内容</param>
        public void Creat_AddStr(ushort areaID, string content)
        {
            byte[] str = Encoding.Default.GetBytes(content);
            byte[] font = Encoding.Default.GetBytes("宋体");
            //string str = "Hello,LED789";
            bx_sdk_dual.EQpageHeader_G6 pheader;
            pheader.PageStyle = 0x01;
            pheader.DisplayMode = 0x03;
            pheader.ClearMode = 0x00;
            pheader.Speed = 15;
            pheader.StayTime = 0;
            pheader.RepeatTime = 1;
            pheader.ValidLen = 0;
            pheader.CartoonFrameRate = 0x00;
            pheader.BackNotValidFlag = 0x00;
            pheader.arrMode = bx_sdk_dual.E_arrMode.eMULTILINE;
            pheader.fontSize = 16;
            pheader.color = (uint)0x01;
            pheader.fontBold = 0;
            pheader.fontItalic = 0;
            pheader.tdirection = bx_sdk_dual.E_txtDirection.pNORMAL;
            pheader.txtSpace = 0;
            pheader.Valign = 1;
            pheader.Halign = 1;
            IntPtr cc = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(bx_sdk_dual.EQpageHeader_G6)));
            Marshal.StructureToPtr(pheader, cc, false);
            err = bx_sdk_dual.program_picturesAreaAddTxt_G6(areaID, str, font, cc);
            if (Event_Log != null)
            {
                Event_Log($"添加内容，代码:{err}");
            }
        }

        public void Creat_Addimg(ushort areaID)
        {
            byte[] str = Encoding.Default.GetBytes("Hello,123");
            byte[] font = Encoding.Default.GetBytes("宋体");
            //string str = "Hello,LED789";
            bx_sdk_dual.EQpageHeader_G6 pheader;
            pheader.PageStyle = 0x00;
            pheader.DisplayMode = 0x01;
            pheader.ClearMode = 0x00;
            pheader.Speed = 15;
            pheader.StayTime = 0;
            pheader.RepeatTime = 1;
            pheader.ValidLen = 0;
            pheader.CartoonFrameRate = 0x20;
            pheader.BackNotValidFlag = 0x00;
            pheader.arrMode = bx_sdk_dual.E_arrMode.eSINGLELINE;
            pheader.fontSize = 15;
            pheader.color = (uint)0x01;
            pheader.fontBold = 0;
            pheader.fontItalic = 0;
            pheader.tdirection = bx_sdk_dual.E_txtDirection.pNORMAL;
            pheader.txtSpace = 0;
            pheader.Valign = 2;
            pheader.Halign = 2;
            byte[] img = Encoding.Default.GetBytes("sstest.png");
            IntPtr cc = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(bx_sdk_dual.EQpageHeader_G6)));
            Marshal.StructureToPtr(pheader, cc, false);
            err = bx_sdk_dual.program_pictureAreaAddPic_G6(areaID, 0, cc, img);
            // int err = bx_sdk_dual.program_pictureAreaAddPic_G6(areaID, 0, cc, arrr_img);
            // Console.WriteLine("program_pictureAreaAddPic_G6:" + err);
            if (Event_Log != null)
            {
                Event_Log($"添加t图片内容，代码:{err}");
            }
        }

        /// <summary>
        /// 发送节目
        /// </summary>
        /// <param name="ipAdder"></param>
        public void Net_SendProgram(string _ipAddr)
        {
            if (string.IsNullOrEmpty(_ipAddr))
            {
                if (Event_Log != null)
                {
                    Event_Log($"发送节目，控制卡ip不能为空");
                }
                return;
            }
            byte[] ipAddr = Encoding.GetEncoding("GBK").GetBytes(_ipAddr);
            byte[] arrProgram = new byte[100];//[Marshal.SizeOf(typeof(bx_sdk_dual.EQprogram))];
            bx_sdk_dual.EQprogram_G6 program;
            err = bx_sdk_dual.program_IntegrateProgramFile_G6(arrProgram);
            // Console.WriteLine("program_IntegrateProgramFile_G6:" + err);
            if (Event_Log != null)
            {
                Event_Log($"发送节目，program_IntegrateProgramFile_G6,代码:{err}");
            }
            IntPtr dec = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(bx_sdk_dual.EQprogram_G6)));
            Marshal.Copy(arrProgram, Marshal.SizeOf(typeof(bx_sdk_dual.EQprogram_G6)) * 0, dec, Marshal.SizeOf(typeof(bx_sdk_dual.EQprogram_G6)));
            program = (bx_sdk_dual.EQprogram_G6)Marshal.PtrToStructure(dec, typeof(bx_sdk_dual.EQprogram_G6));
            Marshal.FreeHGlobal(dec);

            err = bx_sdk_dual.cmd_ofsStartFileTransf(ipAddr, 5005);
            //  Console.WriteLine("cmd_ofsStartFileTransf:" + err);
            if (Event_Log != null)
            {
                Event_Log($"发送节目，cmd_ofsStartFileTransf,代码:{err}");
            }

            err = bx_sdk_dual.cmd_ofsWriteFile(ipAddr, 5005, program.dfileName, program.dfileType, program.dfileLen, 1, program.dfileAddre);
            // Console.WriteLine("cmd_ofsWriteFile:" + err);
            if (Event_Log != null)
            {
                Event_Log($"发送节目，cmd_ofsWriteFile,代码:{err}");
            }
            err = bx_sdk_dual.cmd_ofsWriteFile(ipAddr, 5005, program.fileName, program.fileType, program.fileLen, 1, program.fileAddre);
            // Console.WriteLine("cmd_ofsWriteFile:" + err);
            if (Event_Log != null)
            {
                Event_Log($"发送节目，cmd_ofsWriteFile,代码:{err}");
            }
            err = bx_sdk_dual.cmd_ofsEndFileTransf(ipAddr, 5005);
            // Console.WriteLine("cmd_ofsEndFileTransf:" + err);
            if (Event_Log != null)
            {
                Event_Log($"发送节目，cmd_ofsEndFileTransf,代码:{err}");
            }
            err = bx_sdk_dual.program_deleteProgram_G6();
            if (Event_Log != null)
            {
                Event_Log($"发送节目，cmd_ofsEndFileTransf,代码:{err}");
            }

        }
        /// <summary>
        /// 释放sdk
        /// </summary>
        public void release_sdk()
        {
            bx_sdk_dual.ReleaseSdk();
        }


        #endregion
        /// <summary>
        /// 析构函数
        /// </summary>
        public void Dispose()
        {
            // throw new NotImplementedException();
            try
            {
                GC.Collect();
            }
            catch
            {

            }
        }



    }
}
