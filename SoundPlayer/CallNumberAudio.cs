using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Media;

namespace sdn_SoundPlayer
{
    public class CallNumberAudio : IDisposable
    {
        private System.Media.SoundPlayer player = null;
        private string loc = "";
        private int _span = 250;
        /// <summary>
        /// 每个文件开始播放后，线程休眠的时间，单位为毫秒
        /// </summary>
        public int play_span
        {
            get
            {
                return _span;
            }
            set
            {
                _span = value <= 200 ? 200 : value;
            }
        }

        public CallNumberAudio()
        {
            player = new System.Media.SoundPlayer();
            loc = AppDomain.CurrentDomain.BaseDirectory;
        }

        public CallNumberAudio(string audio_loc)
        {
            player = new System.Media.SoundPlayer();
            if (audio_loc != null)
            {
                loc = audio_loc;
            }
            else
            {
                loc = AppDomain.CurrentDomain.BaseDirectory;
            }
        }
        /// <summary>
        /// 播放叫号语音
        /// </summary>
        /// <param name="call_number">号码</param>
        /// <param name="service_num">服务窗口号</param>
        public void Call(string call_number, string service_num)
        {
            int iWinNum = 0;
            int.TryParse(service_num, out iWinNum); //窗口号转换成数字
            if (iWinNum == 10)//如果等于10
            {
                service_num = "X";
            }
            else if (iWinNum > 10 && iWinNum < 20)
            {
                service_num = "X" + iWinNum % 10; //语音十几
            }
            string code = "Q" + call_number + "M" + service_num + "H";
            code = code.ToUpper();
            foreach (var ch in code)
            {
                var file = findResource(ch);
                if (file != "")
                {
                    player.SoundLocation = file;
                    // player.Play();
                    player.PlaySync();
                    System.Threading.Thread.Sleep(_span);
                }
            }
        }

        private string findResource(char ch)
        {
            string outcome = loc;
            if (outcome.Last() != '\\')
            {
                outcome += '\\';
            }
            switch (ch)
            {
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case 'A':
                case 'B':
                case 'D':
                case 'C':
                    {
                        outcome += ch;
                        break;
                    }
                case 'Q':
                    {
                        outcome += "Qing";
                        break;
                    }
                case 'M':
                    {
                        outcome += "Dao";
                        break;
                    }
                case 'H':
                    {
                        outcome += "HaoChuangKouBanLiYeWu";
                        break;
                    }
                case 'X': //数字10
                    {
                        outcome += "10";
                        break;
                    }
                default:
                    {
                        return "";
                    }
            }
            outcome += ".wav";
            return outcome;
        }

        public void Dispose()
        {
            if (player != null)
            {
                player.Dispose();
            }
        }
    }
}
