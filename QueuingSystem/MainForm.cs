using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QueuingSystem
{
    public partial class MainForm : Form
    {
        LED_Util.zhonghe.showMsg_zh zh_show = new LED_Util.zhonghe.showMsg_zh();
        public MainForm()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 测试打印
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTest_Click(object sender, EventArgs e)
        {
            zh_show.Event_Log += _InfoLog;
            //Common.Util.RawPrint.SendStringToPrinter("", "ssss");
            // new Common.Util.ClsPrintLPT().PrintDataSet_test();
            zh_show.sendMsg2Screen("sssssssssssssssss");

        }
        /// <summary>
        /// 普通日志
        /// </summary>
        /// <param name="info"></param>
        private void _InfoLog(string info)
        {
            LogHelper.WriteLog(info);
        }
        /// <summary>
        /// 错误日志
        /// </summary>
        /// <param name="err"></param>
        private void _ErrLog(string err,Exception ex)
        {
            LogHelper.WriteLog(err, ex);
        }
    }
}
