using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueuingSystem.hardware
{
    class QueuePrinter : IDisposable
    {
        private System.Drawing.Printing.PrintDocument _printer = null;
        private string _call_num = "";
        private int _rest_count = 0;
        private string _id_num = "";
        private string _name = "";
        public QueuePrinter()
        {
            _printer = new System.Drawing.Printing.PrintDocument();
            _printer.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(_printer_PrintPage);
        }

        void _printer_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            string stradress = "";
            string strPath = AppDomain.CurrentDomain.BaseDirectory + @"config\sdnSystem.ini";
            if (!string.IsNullOrEmpty(strPath))
            {
                operConfig.ReadIniFile sdnReadIni = new operConfig.ReadIniFile(strPath);
                stradress = sdnReadIni.ReadValue("Adress", "adress");//得到公司地址

            }
            Brush bstr = Brushes.Black;
            Font normalFont = new System.Drawing.Font("宋体", 9.5F, FontStyle.Regular);//其他行字体
            Font biggerFont = new System.Drawing.Font("宋体", 48F, FontStyle.Bold);//号码字体
            int first_row = 70;//首行位置
            int row_det = 20;//下一行增加的位移
            int row = first_row;
            e.Graphics.DrawString(_call_num, biggerFont, bstr, 12, 0);//号码加大居中显示
            e.Graphics.DrawString("*前面有" + _rest_count + "人等待", normalFont, bstr, 0, row);
            row += row_det;
            e.Graphics.DrawString("身份证号：" + _id_num, normalFont, bstr, 0, row);
            row += row_det;
            e.Graphics.DrawString("姓名：" + _name, normalFont, bstr, 0, row);
            row += row_det;
            e.Graphics.DrawString("时间：" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), normalFont, bstr, 0, row);
            row += row_det;
            e.Graphics.DrawString("*当日当次有效，过号作废", normalFont, bstr, 0, row);
            row += row_det;
            e.Graphics.DrawString("*" + stradress, normalFont, bstr, 0, row);
            // e.Graphics.DrawString("*邓尉路555号 虎丘大队", normalFont, bstr, 0, row);
            row += row_det;
            return;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="call_num">编号</param>
        /// <param name="rest_count">前面剩余人数</param>
        /// <param name="id_num">身份证号</param>
        /// <param name="name">姓名</param>
        public void PrintData(string call_num, int rest_count, string id_num, string name)
        {
            _call_num = call_num;
            _rest_count = rest_count;
            int length = id_num.Length;
            int first = length - 6 <= 0 ? length - 3 : length - 6;
            _id_num = id_num.Substring(0, first).PadRight(18, '*');//加*号
            _name = name;
            _printer.Print();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="call_num">编号</param>
        /// <param name="rest_count">前面剩余人数</param>
        /// <param name="id_num">身份证号</param>
        /// <param name="name">姓名</param>
        public void PrintDataNoID(string call_num, int rest_count, string id_num, string name)
        {
            _call_num = call_num;
            _rest_count = rest_count;
            //int length = id_num.Length;
            //int first = length - 6 <= 0 ? length - 3 : length - 6;
            _id_num = id_num;//加*号
            _name = name;
            _printer.Print();
        }
        public void Dispose()
        {
            if (_printer != null)
            {
                _printer.Dispose();
            }
        }
    }
}