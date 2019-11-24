using Common.Redis;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QueuingSystem_DCS
{
    public partial class sdnMainForm : Form
    {
        /// <summary>
        /// redis帮助类
        /// </summary>
        RedisStackExchangeHelper _redis = new RedisStackExchangeHelper(); //实例化redis帮助类
        public sdnMainForm()
        {
            InitializeComponent();
        }


        #region redis 锁、订阅/发布函数
        /// <summary>
        /// 向指定的频道发布信息
        /// </summary>
        /// <param name="channel">频道</param>
        /// <param name="msg">发布信息</param>
        private async void pub_msg(string channel, string msg)
        {
            await _redis.PublishAsync(channel, msg);
        }
        /// <summary>
        /// 订阅指定的频道，接收对应信息
        /// </summary>
        /// <param name="channel"></param>
        private async void sub_msg(string channel)
        {
            await _redis.SubscribeAsync(channel, (cha, message) =>
            {
                Console.WriteLine("接受到发布的内容为：" + message);
                MessageBox.Show("接受到发布的内容为：" + message);
            });
        }
        /// <summary>
        /// 取消所有订阅
        /// </summary>
        private void unSubAll()
        {
            _redis.UnsubscribeAll();
        }
        /// <summary>
        /// 通过锁获取指定的最大队列序号 并把队列+1
        /// </summary>
        /// <param name="strKey"></param>
        /// <returns></returns>
        private string getMax_lock(string strKey)
        {
            string MaxQueNo = "";
            var db = _redis.GetDatabase();
            RedisValue token = Environment.MachineName;
            //实际项目秒杀此处可换成商品ID
            if (db.LockTake("sdn", token, TimeSpan.FromSeconds(10)))
            {
                try
                {
                    MaxQueNo = _redis.StringGet(strKey);
                    int iMaxQueNo = Convert.ToInt32(MaxQueNo) + 1;
                    _redis.StringSet(strKey, iMaxQueNo + "");
                }
                finally
                {
                    db.LockRelease("sdn", token);
                }
            }
            return MaxQueNo;
        }

        #endregion
        /// <summary>
        /// 测试按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTest_Click(object sender, EventArgs e)
        {
            sub_msg("sdn");
        }
    }
}
