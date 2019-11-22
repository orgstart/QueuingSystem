using QueuingSystem.OperQueue;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace QueuingSystem
{
    public partial class sdnMainForm : Form
    {
        LED_Util.zhonghe.showMsg_zh zh_show = new LED_Util.zhonghe.showMsg_zh();

        #region 全局变量

        private string strExePath = ""; //exe 程序所在路径
        private string strHtmlPath = ""; //html页面路径
        private QueueList sdnQueList = null;//队列 list 正常排队队列
        private QueueList sdnQueList_YY = null;//网上预约排队队列

        private QueueList sdnQueList_YQ = null;//园区 其他业务排队
        private Dictionary<string, int> dicNoTimes = null;//证件号与取票次数字典
        private int iQueNo = 0;//排队初始号码  数字格式，排队总人数
        private string strQueNo = "001";//排队初始化排队号码  标准格式
        private string sdn_CardNo = "";//身份证号码
        private string strQueType = "A"; //取票种类
        private int iMaxA = 0, iMaxB = 0, iMaxC = 0; //分别以A B C 开头的队列最大值
        private int iMaxD = 0;//园区特殊屏  

        private string isWebShow = "0";//是否显示web网页
        private int i_showType = 7; //排队信息展示类型 默认为LED1 显示
        private string strDS = "0";//电视信息是否显示（一机双屏）默认不显示

        private int iAllCount = 0;//总取票人数
        private int iNoDealCount = 0;//等待叫号人数
        private int iDealCount = 0;//处理完成人数

        private int iMaxCount = 2;//当天最大取票次数
        private string strAdress = "交通违法处理点"; //违法处理地点
        private string strBBDM = "320506005300";//违法处理部门网点
        private string strMachine = "001000001";//唯一机器码，前三位机器种类后六位唯一编号
        private string strJH = "";//配置警号

        private string strUrl = "";//远程数据库地址

        private int isNet = 0;//是否联网 0否 1是

        private int sdn_iDay = DateTime.Now.DayOfYear;//一年中的第几天

        private string strServerIp = "127.0.0.1";//本机Ip

        //   private Dictionary<string, string> dicServerInfo = new Dictionary<string, string>(); //服务器备案信息

        DataTable dtServerInfo = null;//服务器备案信息

        private QueueList queuelist_BC;//补传取票记录
        httpServer.sdnHttpServer httpserver = null;//全局http服务

        private Dictionary<string, string> dicPause = new Dictionary<string, string>(); //暂停字典表 IP与窗口号


        #endregion

        public sdnMainForm()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();
            sdnQueList = new QueueList(); //实例化排队队列
            sdnQueList_YY = new QueueList();//网上预约排队队列
            queuelist_BC = new QueueList();//补传取票信息队列表
            sdnQueList_YQ = new QueueList();//园区其他排队
            dicNoTimes = new Dictionary<string, int>();//证件号与取票次数字典
            strExePath = AppDomain.CurrentDomain.BaseDirectory; //得到exe程序所在位置
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
            zh_show.sendMsg2Screen("测试测试测试");

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
        private void _ErrLog(string err, Exception ex)
        {
            LogHelper.WriteLog(err, ex);
        }
    }
}
