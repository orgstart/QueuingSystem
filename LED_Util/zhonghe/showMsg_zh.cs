using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LED_Util.zhonghe
{
    /// <summary>
    /// 综合屏显示信息
    /// </summary>
    public class showMsg_zh
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
        public void sendMsg2Screen(string content)
        {
            using(bx_func bxFun = new bx_func())
            {
                bxFun.Event_Log += sdn_log;
                bxFun.initSDK();
                bxFun.setScreenParams_G56();
                bxFun.create_Program();
                bxFun.Creat_Area(0, 1, 1, 200, 200, 0);
                bxFun.AreaAddFrame(0);
                bxFun.Creat_AddStr(0, content);
                bxFun.Net_SendProgram("50.79.98.180");
                bxFun.release_sdk();
            }
        }
        /// <summary>
        /// 日志记录
        /// </summary>
        /// <param name="_info"></param>
        private  void sdn_log(string _info)
        {
            if (Event_Log != null)
            {
                Event_Log(_info);
            }
        }
    }
}
