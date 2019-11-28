namespace WinControlServer
{
    partial class IEShowWin
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.sdnWebBrowser = new System.Windows.Forms.WebBrowser();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.btnSInName = new System.Windows.Forms.Button();
            this.btnSinNo = new System.Windows.Forms.Button();
            this.btnPrint = new System.Windows.Forms.Button();
            this.btnNoPrint = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.panel3);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(780, 538);
            this.panel1.TabIndex = 0;
            // 
            // sdnWebBrowser
            // 
            this.sdnWebBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sdnWebBrowser.Location = new System.Drawing.Point(0, 0);
            this.sdnWebBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            this.sdnWebBrowser.Name = "sdnWebBrowser";
            this.sdnWebBrowser.Size = new System.Drawing.Size(780, 496);
            this.sdnWebBrowser.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.sdnWebBrowser);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(780, 496);
            this.panel2.TabIndex = 1;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.btnNoPrint);
            this.panel3.Controls.Add(this.btnPrint);
            this.panel3.Controls.Add(this.btnSinNo);
            this.panel3.Controls.Add(this.btnSInName);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel3.Location = new System.Drawing.Point(0, 500);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(780, 38);
            this.panel3.TabIndex = 2;
            // 
            // btnSInName
            // 
            this.btnSInName.Location = new System.Drawing.Point(199, 4);
            this.btnSInName.Name = "btnSInName";
            this.btnSInName.Size = new System.Drawing.Size(128, 31);
            this.btnSInName.TabIndex = 0;
            this.btnSInName.Text = "签字(姓名）";
            this.btnSInName.UseVisualStyleBackColor = true;
            this.btnSInName.Click += new System.EventHandler(this.btnSInName_Click);
            // 
            // btnSinNo
            // 
            this.btnSinNo.Location = new System.Drawing.Point(333, 4);
            this.btnSinNo.Name = "btnSinNo";
            this.btnSinNo.Size = new System.Drawing.Size(138, 31);
            this.btnSinNo.TabIndex = 1;
            this.btnSinNo.Text = "签字(无异议）";
            this.btnSinNo.UseVisualStyleBackColor = true;
            this.btnSinNo.Click += new System.EventHandler(this.btnSinNo_Click);
            // 
            // btnPrint
            // 
            this.btnPrint.Location = new System.Drawing.Point(477, 3);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(138, 31);
            this.btnPrint.TabIndex = 2;
            this.btnPrint.Text = "确定(打印）";
            this.btnPrint.UseVisualStyleBackColor = true;
            this.btnPrint.Click += new System.EventHandler(this.btnPrint_Click);
            // 
            // btnNoPrint
            // 
            this.btnNoPrint.Location = new System.Drawing.Point(621, 4);
            this.btnNoPrint.Name = "btnNoPrint";
            this.btnNoPrint.Size = new System.Drawing.Size(138, 31);
            this.btnNoPrint.TabIndex = 3;
            this.btnNoPrint.Text = "确定(不打印）";
            this.btnNoPrint.UseVisualStyleBackColor = true;
            this.btnNoPrint.Click += new System.EventHandler(this.btnNoPrint_Click);
            // 
            // IEShowWin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(780, 538);
            this.ControlBox = false;
            this.Controls.Add(this.panel1);
            this.Name = "IEShowWin";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "签名评价---请在签名板上签字";
            this.Load += new System.EventHandler(this.IEShowWin_Load);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.WebBrowser sdnWebBrowser;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button btnNoPrint;
        private System.Windows.Forms.Button btnPrint;
        private System.Windows.Forms.Button btnSinNo;
        private System.Windows.Forms.Button btnSInName;
        private System.Windows.Forms.Panel panel2;
    }
}