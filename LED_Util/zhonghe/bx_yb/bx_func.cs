using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LED_Util.zhonghe
{
    /// <summary>
    /// 此类是对BX06SDK常用方法的逻辑代码
    /// 初始化动态库InitSdk--->设置屏幕相关参数program_setScreenParams_G56----->创建节目program_addProgram----->创建区域 program_AddArea----->添加区域内容program_picturesAreaAddTxt---->合成节目program_IntegrateProgramFile---->开始写文件cmd_ofsStartFileTransf----->写文件cmd_ofsWriteFile----->结束写文件cmd_ofsEndFileTransf----->清除节目缓存program_deleteProgram----->释放动态库ReleaseSd
    /// </summary>
    public class bx_func:IDisposable
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
        /// 创建节目
        /// </summary>
        public void create_Program()
        {
            bx_sdk_dual.EQprogramHeader header;
            header.FileType = 0x00;
            header.ProgramID = 0;
            header.ProgramStyle = 0x00;
            header.ProgramPriority = 0x00;
            header.ProgramPlayTimes = 1;
            header.ProgramTimeSpan = 0;
            header.ProgramWeek = 0xff;
            header.ProgramLifeSpan_sy = 0xffff;
            header.ProgramLifeSpan_sm = 0x03;
            header.ProgramLifeSpan_sd = 0x05;
            header.ProgramLifeSpan_ey = 0xffff;
            header.ProgramLifeSpan_em = 0x04;
            header.ProgramLifeSpan_ed = 0x12;
            header.PlayPeriodGrpNum = 0;
            IntPtr aa = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(bx_sdk_dual.EQprogramHeader)));
            Marshal.StructureToPtr(header, aa, false);
            err = bx_sdk_dual.program_addProgram(aa);
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
            bx_sdk_dual.EQareaHeader aheader;
            aheader.AreaType = AreaType;
            aheader.AreaX = x;
            aheader.AreaY = y;
            aheader.AreaWidth = w;
            aheader.AreaHeight = h;
            IntPtr bb = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(bx_sdk_dual.EQareaHeader)));
            Marshal.StructureToPtr(aheader, bb, false);
            err = bx_sdk_dual.program_AddArea(areaID, bb);  //添加图文区域
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
            bx_sdk_dual.EQpageHeader pheader;
            pheader.PageStyle = 0x00;
            pheader.DisplayMode = 0x03;
            pheader.ClearMode = 0x01;
            pheader.Speed = 30;
            pheader.StayTime = 0;
            pheader.RepeatTime = 1;
            pheader.ValidLen = 0;
            pheader.arrMode = bx_sdk_dual.E_arrMode.eSINGLELINE;
            pheader.fontSize = 12;
            pheader.color = (uint)0x01;
            pheader.fontBold = false;
            pheader.fontItalic = false;
            pheader.tdirection = bx_sdk_dual.E_txtDirection.pNORMAL;
            pheader.txtSpace = 0;
            pheader.Valign = 2;
            pheader.Halign = 2;
            IntPtr cc = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(bx_sdk_dual.EQpageHeader)));
            Marshal.StructureToPtr(pheader, cc, false);
            err = bx_sdk_dual.program_picturesAreaAddTxt(areaID, str, font, cc);
            if (Event_Log != null)
            {
                Event_Log($"添加内容，代码:{err}");
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
            err = bx_sdk_dual.cmd_ofsStartFileTransf(ipAddr, 5005);
            if (Event_Log != null)
            {
                Event_Log($"发送节目，cmd_ofsStartFileTransf,代码:{err}");
            }
            byte[] arrProgram = new byte[100];//[Marshal.SizeOf(typeof(bx_sdk_dual.EQprogram))];
            bx_sdk_dual.EQprogram program;
            err = bx_sdk_dual.program_IntegrateProgramFile(arrProgram);
            if (Event_Log != null)
            {
                Event_Log($"发送节目，program_IntegrateProgramFile,代码:{err}");
            }
            IntPtr dec = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(bx_sdk_dual.EQprogram)));
            Marshal.Copy(arrProgram, Marshal.SizeOf(typeof(bx_sdk_dual.EQprogram)) * 0, dec, Marshal.SizeOf(typeof(bx_sdk_dual.EQprogram)));
            program = (bx_sdk_dual.EQprogram)Marshal.PtrToStructure(dec, typeof(bx_sdk_dual.EQprogram));
            Marshal.FreeHGlobal(dec);

            err = bx_sdk_dual.cmd_ofsWriteFile(ipAddr, 5005, program.fileName, program.fileType, program.fileLen, 1, program.fileAddre);
            if (Event_Log != null)
            {
                Event_Log($"发送节目，cmd_ofsWriteFile,代码:{err}");
            }
            err = bx_sdk_dual.cmd_ofsEndFileTransf(ipAddr, 5005);
            if (Event_Log != null)
            {
                Event_Log($"发送节目，cmd_ofsEndFileTransf,代码:{err}");
            }
            err = bx_sdk_dual.program_deleteProgram();
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
