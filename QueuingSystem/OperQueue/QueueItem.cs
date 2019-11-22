using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueuingSystem.OperQueue
{
    public class QueueItem
    {
        private uint m_msgState; //当前信息状态
        /// <summary>
        /// 当前信息状态
        /// </summary>
        public uint msgState
        {
            get { return this.m_msgState; }
            set { this.m_msgState = value; }
        }
        private string m_msgQueueNo;//排队号码，例如A001
        //排队号码，例如A001
        public string msgQueueNo
        {
            get { return this.m_msgQueueNo; }
            set { this.m_msgQueueNo = value; }
        }
        //六合一 24位取票序列号
        private string m_serialNum;
        public string serialNum
        {
            get { return this.m_serialNum; }
            set { this.m_serialNum = value; }
        }
        private string m_windowNum;
        //窗口号
        public string windowNum
        {
            get { return this.m_windowNum; }
            set { this.m_windowNum = value; }
        }

        private string m_windowIp;
        /// <summary>
        /// 叫号窗口IP
        /// </summary>
        public string windowIp
        {
            get { return this.m_windowIp; }
            set { this.m_windowIp = value; }
        }

        private string m_msgCardNo;//身份证号码
        //身份证号码
        public string msgCardNo
        {
            get { return this.m_msgCardNo; }
            set { this.m_msgCardNo = value; }
        }
        private string m_msgName;//人员姓名
        //人员姓名
        public string msgName
        {
            get { return this.m_msgName; }
            set { this.m_msgName = value; }
        }
        private string m_xh;//预约序号
        /// <summary>
        /// 预约序号
        /// </summary>
        public string XH
        {
            get { return this.m_xh; }
            set { this.m_xh = value; }
        }
        /// <summary>
        /// 部门代码
        /// </summary>
        private string m_BMDM;
        public string bmdm
        {
            get { return this.m_BMDM; }
            set { this.m_BMDM = value; }
        }
        private string m_Way;//取票途径 1：正常读取身份证 2：手工输入证件号 3：外籍人员证件输入4：预约取号
        /// <summary>
        /// 取票途径 1：正常读取身份证 2：手工输入证件号 3：外籍人员证件输入4：预约取号
        /// </summary>
        public string mWay
        {
            get { return this.m_Way; }
            set { this.m_Way = value; }
        }
        /// <summary>
        /// 取票时间
        /// </summary>
        public string m_strqhsj;
        public string strqhsj
        {
            get { return this.m_strqhsj; }
            set { this.m_strqhsj = value; }
        }
    }
}

