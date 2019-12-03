namespace WinControlServer
{
    partial class mainControl
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(mainControl));
            this.label1 = new System.Windows.Forms.Label();
            this.sdnnotifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.notifyMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.SettingMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ExitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.queClient = new System.Windows.Forms.ToolStripMenuItem();
            this.notifyMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.ForeColor = System.Drawing.Color.Red;
            this.label1.Location = new System.Drawing.Point(1, 21);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(252, 22);
            this.label1.TabIndex = 0;
            this.label1.Text = "正在运行，请勿关闭！！";
            // 
            // sdnnotifyIcon
            // 
            this.sdnnotifyIcon.ContextMenuStrip = this.notifyMenu;
            this.sdnnotifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("sdnnotifyIcon.Icon")));
            this.sdnnotifyIcon.Text = "签名打印辅助程序";
            this.sdnnotifyIcon.Visible = true;
            // 
            // notifyMenu
            // 
            this.notifyMenu.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.notifyMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SettingMenuItem,
            this.ExitMenuItem,
            this.queClient});
            this.notifyMenu.Name = "notifyMenu";
            this.notifyMenu.Size = new System.Drawing.Size(137, 70);
            // 
            // SettingMenuItem
            // 
            this.SettingMenuItem.Name = "SettingMenuItem";
            this.SettingMenuItem.Size = new System.Drawing.Size(136, 22);
            this.SettingMenuItem.Text = "设置";
            this.SettingMenuItem.Click += new System.EventHandler(this.settingMenuItem_Click);
            // 
            // ExitMenuItem
            // 
            this.ExitMenuItem.ForeColor = System.Drawing.SystemColors.ControlText;
            this.ExitMenuItem.Name = "ExitMenuItem";
            this.ExitMenuItem.Size = new System.Drawing.Size(136, 22);
            this.ExitMenuItem.Text = "退出";
            this.ExitMenuItem.Click += new System.EventHandler(this.ExitMenuItem_Click);
            // 
            // queClient
            // 
            this.queClient.Name = "queClient";
            this.queClient.Size = new System.Drawing.Size(136, 22);
            this.queClient.Text = "叫号客户端";
            this.queClient.Click += new System.EventHandler(this.queClient_Click);
            // 
            // mainControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(262, 71);
            this.ControlBox = false;
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "mainControl";
            this.ShowInTaskbar = false;
            this.Text = "排队辅助程序";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.mainControl_FormClosing);
            this.Load += new System.EventHandler(this.mainControl_Load);
            this.SizeChanged += new System.EventHandler(this.mainControl_SizeChanged);
            this.notifyMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NotifyIcon sdnnotifyIcon;
        private System.Windows.Forms.ContextMenuStrip notifyMenu;
        private System.Windows.Forms.ToolStripMenuItem SettingMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ExitMenuItem;
        private System.Windows.Forms.ToolStripMenuItem queClient;
    }
}

