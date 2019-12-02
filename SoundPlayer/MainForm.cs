using Common.Redis;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SoundPlayer
{
    public partial class mainForm : Form
    {
        /// <summary>
        /// redis帮助类
        /// </summary>
        // RedisStackExchangeHelper _redis = new RedisStackExchangeHelper(); //实例化redis帮助类
         public static RedisStackExchangeHelper _redis = null; //实例化redis帮助类

        public mainForm()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void mainForm_Load(object sender, EventArgs e)
        {
            _redis = new RedisStackExchangeHelper(); //实例化redis帮助类
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
                //   MessageBox.Show("接受到发布的内容为：" + message);
                dealMessage(message);
            });
        }
        /// <summary>
        /// 取消所有订阅
        /// </summary>
        private void unSubAll()
        {
            _redis.UnsubscribeAll();
        }

        #endregion

        /// <summary>
        /// 开启服务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStart_Click(object sender, EventArgs e)
        {
            sub_msg("sdnsound"); //订阅sdnsound redis
        }
        /// <summary>
        /// 处理订阅信息
        /// </summary>
        /// <param name="strMsg"></param>
        private void dealMessage(string strMsg)
        {
            try
            {
                //   MessageBox.Show(strMsg);
                lbShowMsg.Text = strMsg;
                JObject jobj =(JObject)JsonConvert.DeserializeObject(strMsg);
                using(sdn_SoundPlayer.CallNumberAudio caller = new sdn_SoundPlayer.CallNumberAudio("wavFiles"))
                {
                    caller.Call(jobj["queue"].ToString(), jobj["winnum"].ToString());
                }


            }
            catch { }
        }
    }
}
