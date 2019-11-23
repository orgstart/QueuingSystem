namespace QueuingSystem
{
    partial class sdnMainForm
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
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(sdnMainForm));
            this.sdnWebBrowser = new System.Windows.Forms.WebBrowser();
            this.btn_exit = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // sdnWebBrowser
            // 
            this.sdnWebBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sdnWebBrowser.Location = new System.Drawing.Point(0, 0);
            this.sdnWebBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            this.sdnWebBrowser.Name = "sdnWebBrowser";
            this.sdnWebBrowser.ScrollBarsEnabled = false;
            this.sdnWebBrowser.Size = new System.Drawing.Size(800, 450);
            this.sdnWebBrowser.TabIndex = 0;
            this.sdnWebBrowser.WebBrowserShortcutsEnabled = false;
            // 
            // btn_exit
            // 
            this.btn_exit.BackColor = System.Drawing.Color.Transparent;
            this.btn_exit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_exit.ForeColor = System.Drawing.Color.Transparent;
            this.btn_exit.Image = global::QueuingSystem.Properties.Resources._1;
            this.btn_exit.Location = new System.Drawing.Point(160, 68);
            this.btn_exit.Name = "btn_exit";
            this.btn_exit.Size = new System.Drawing.Size(8, 8);
            this.btn_exit.TabIndex = 1;
            this.btn_exit.Text = "button1";
            this.btn_exit.UseVisualStyleBackColor = false;
            this.btn_exit.UseWaitCursor = true;
            // 
            // sdnMainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.ControlBox = false;
            this.Controls.Add(this.btn_exit);
            this.Controls.Add(this.sdnWebBrowser);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "sdnMainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "排队系统";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.sdnMainForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser sdnWebBrowser;
        private System.Windows.Forms.Button btn_exit;
    }
}

