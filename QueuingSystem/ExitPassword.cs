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
    public partial class ExitPassword : Form
    {
        public string password = "";
        public static ExitPassword ep = null;
        public ExitPassword()
        {
            InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 单例模式
        /// </summary>
        /// <returns></returns>
        public static ExitPassword GetExitPassWordSingle()
        {
            if (ep == null)
            {
                ep = new ExitPassword();
            }
            return ep;
        }
        private void btnSure_Click(object sender, EventArgs e)
        {
            try
            {
                var iniPwd = new operConfig.ReadIniFile(AppDomain.CurrentDomain.BaseDirectory + "configs\\sdnSystem.ini").ReadValue("Password", "password");//配置密码
                if (password == iniPwd)
                {
                    this.DialogResult = DialogResult.Yes;
                }
                else
                {
                    MessageBox.Show("密码错误!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void lbl1_Click(object sender, EventArgs e)
        {
            Button p = sender as Button;
            password += p.Tag.ToString();
            this.txtPwd.Text += "*";
        }
    }
}
