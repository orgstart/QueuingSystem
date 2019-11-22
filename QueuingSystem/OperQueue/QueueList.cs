using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueuingSystem.OperQueue
{
    public class QueueList : BaseSortedList<string, QueueItem>
    {
        /// <summary>
        /// 根据关键字搜索队列
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public QueueItem Find(string key)
        {
            if (base.m_List.ContainsKey(key))
            {
                return base.m_List[key];
            }
            return null;
        }
        /// <summary>
        /// 根据IP得到队列
        /// </summary>
        /// <param name="strIP"></param>
        /// <returns></returns>
        public QueueItem FindByIp(string strIP)
        {
            for (int i = 0; i < base.m_List.Count; i++)
            {
                QueueItem item = base.m_List.Values[i];
                if (item.msgState == 1 && item.windowIp == strIP)
                {
                    return item;
                }
            }
            return null;
        }


        /// <summary>
        /// 根据物理叫号窗口获取队列
        /// </summary>
        /// <param name="strIP"></param>
        /// <returns></returns>
        public QueueItem FindByWinnum(string winnum)
        {
            for (int i = 0; i < base.m_List.Count; i++)
            {
                QueueItem item = base.m_List.Values[i];
                if (item.msgState == 1 && item.windowNum == winnum)
                {
                    return item;
                }
            }
            return null;
        }

        /// <summary>
        /// 最上面的出队列
        /// 标记状态为正在被叫号
        /// </summary>
        /// <returns></returns>
        public QueueItem GetTopOutQueue()
        {
            for (int i = 0; i < base.m_List.Count; i++)
            {
                QueueItem item = base.m_List.Values[i];
                if (item.msgState == 0)
                {
                    lock (this)
                    {
                        item.msgState = 1;
                    }
                    return item;
                }
            }
            return null;
        }

        public QueueItem GetTopOutQueue(string strWinNum)
        {
            for (int i = 0; i < base.m_List.Count; i++)
            {
                QueueItem item = base.m_List.Values[i];
                if (item.msgState == 0)
                {
                    lock (this)
                    {
                        item.msgState = 1;
                        item.windowNum = strWinNum; //更新窗口号信息
                    }
                    return item;
                }
                else if (item.msgState == 1 && item.windowNum == strWinNum)//物理叫号，将之前已经叫过号的状态修改为2，已完成
                {
                    lock (this)
                    {
                        item.msgState = 2;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 最上面的出队列
        /// 标记状态为正在被叫号
        /// 并更新窗口号 和窗口IP信息
        /// </summary>
        /// <returns></returns>
        public QueueItem GetTopOutQueue(string strWinIp, string strWinNum)
        {
            for (int i = 0; i < base.m_List.Count; i++)
            {
                QueueItem item = base.m_List.Values[i];
                if (item.msgState == 0)
                {
                    lock (this)
                    {
                        item.msgState = 1;
                        item.windowNum = strWinNum; //更新窗口号信息
                        item.windowIp = strWinIp;//更新控制窗口IP信息
                    }
                    return item;
                }
            }
            return null;
        }
        /// <summary>
        /// 更新某列的数据
        /// </summary>
        /// <param name="strKey">关键字</param>
        /// <param name="iState">状态 0：未被叫号 1：正在叫号 2：完成 3：跳号</param>
        /// <returns>更新是否成功</returns>
        public bool UpdateQueue(string strKey, int iState)
        {
            if (base.m_List.ContainsKey(strKey))
            {
                lock (this)
                {
                    base.m_List[strKey].msgState = (uint)iState;
                }
                return true;
            }
            return false;
        }
        /// <summary>
        /// 获取队列中的排队数目
        /// </summary>
        /// <returns>以int数组的形式返回 quenum排队总数,donum已处理数,nonum未处理总</returns>
        public int[] GetDealQueue()
        {
            int quenum = 0, donum = 0, nonum = 0;
            for (int i = 0; i < base.m_List.Count; i++)
            {
                QueueItem item = base.m_List.Values[i];
                if (item.msgState == 2) //已经处理
                {
                    donum++;
                }
                else if (item.msgState == 0)
                {
                    nonum++;
                }
                quenum++;
            }
            return new int[] { quenum, donum, nonum };
        }
        /// <summary>
        /// 清除队列数据
        /// </summary>
        public void ClearAllQue()
        {
            for (int i = 0; i < base.m_List.Count; i++)
            {
                //   QueueItem item = base.m_List.Values[i];
                base.Remove(base.m_List.Keys[i]);
            }
        }
    }
}
