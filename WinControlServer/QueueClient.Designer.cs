namespace WinControlServer
{
    partial class QueueClient
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
            this.components = new System.ComponentModel.Container();
            this.btnCallNum = new sdnControls.sdnButtonEx.ButtonEx();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.btnScore = new sdnControls.sdnButtonEx.ButtonEx();
            this.btnOver = new sdnControls.sdnButtonEx.ButtonEx();
            this.btnHere = new sdnControls.sdnButtonEx.ButtonEx();
            this.btnSkip = new sdnControls.sdnButtonEx.ButtonEx();
            this.btnReCall = new sdnControls.sdnButtonEx.ButtonEx();
            this.panel2 = new System.Windows.Forms.Panel();
            this.lbName = new System.Windows.Forms.Label();
            this.lbIDNum = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lbQueNo = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lbQueNum = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lbShowMsg = new System.Windows.Forms.Label();
            this.timer_form_hidden = new System.Windows.Forms.Timer(this.components);
            this.panel1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCallNum
            // 
            this.btnCallNum.Location = new System.Drawing.Point(12, 24);
            this.btnCallNum.Name = "btnCallNum";
            this.btnCallNum.Size = new System.Drawing.Size(48, 23);
            this.btnCallNum.TabIndex = 3;
            this.btnCallNum.Text = "叫号";
            this.btnCallNum.UseVisualStyleBackColor = true;
            this.btnCallNum.Click += new System.EventHandler(this.btnCallNum_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.panel3);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 24);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(244, 229);
            this.panel1.TabIndex = 4;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.btnScore);
            this.panel3.Controls.Add(this.btnOver);
            this.panel3.Controls.Add(this.btnHere);
            this.panel3.Controls.Add(this.btnSkip);
            this.panel3.Controls.Add(this.btnReCall);
            this.panel3.Controls.Add(this.btnCallNum);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel3.Location = new System.Drawing.Point(0, 99);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(244, 130);
            this.panel3.TabIndex = 5;
            // 
            // btnScore
            // 
            this.btnScore.Location = new System.Drawing.Point(180, 74);
            this.btnScore.Name = "btnScore";
            this.btnScore.Size = new System.Drawing.Size(48, 23);
            this.btnScore.TabIndex = 8;
            this.btnScore.Text = "评价";
            this.btnScore.UseVisualStyleBackColor = true;
            this.btnScore.Click += new System.EventHandler(this.btnScore_Click);
            // 
            // btnOver
            // 
            this.btnOver.Location = new System.Drawing.Point(97, 74);
            this.btnOver.Name = "btnOver";
            this.btnOver.Size = new System.Drawing.Size(48, 23);
            this.btnOver.TabIndex = 7;
            this.btnOver.Text = "完成";
            this.btnOver.UseVisualStyleBackColor = true;
            this.btnOver.Click += new System.EventHandler(this.btnOver_Click);
            // 
            // btnHere
            // 
            this.btnHere.Location = new System.Drawing.Point(12, 74);
            this.btnHere.Name = "btnHere";
            this.btnHere.Size = new System.Drawing.Size(48, 23);
            this.btnHere.TabIndex = 6;
            this.btnHere.Text = "到达";
            this.btnHere.UseVisualStyleBackColor = true;
            this.btnHere.Click += new System.EventHandler(this.btnHere_Click);
            // 
            // btnSkip
            // 
            this.btnSkip.Location = new System.Drawing.Point(180, 24);
            this.btnSkip.Name = "btnSkip";
            this.btnSkip.Size = new System.Drawing.Size(48, 23);
            this.btnSkip.TabIndex = 5;
            this.btnSkip.Text = "跳号";
            this.btnSkip.UseVisualStyleBackColor = true;
            this.btnSkip.Click += new System.EventHandler(this.btnSkip_Click);
            // 
            // btnReCall
            // 
            this.btnReCall.Location = new System.Drawing.Point(97, 24);
            this.btnReCall.Name = "btnReCall";
            this.btnReCall.Size = new System.Drawing.Size(48, 23);
            this.btnReCall.TabIndex = 4;
            this.btnReCall.Text = "重叫";
            this.btnReCall.UseVisualStyleBackColor = true;
            this.btnReCall.Click += new System.EventHandler(this.btnReCall_Click);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.lbName);
            this.panel2.Controls.Add(this.lbIDNum);
            this.panel2.Controls.Add(this.label3);
            this.panel2.Controls.Add(this.lbQueNo);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.lbQueNum);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Controls.Add(this.lbShowMsg);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(244, 93);
            this.panel2.TabIndex = 4;
            // 
            // lbName
            // 
            this.lbName.AutoSize = true;
            this.lbName.Location = new System.Drawing.Point(132, 49);
            this.lbName.Name = "lbName";
            this.lbName.Size = new System.Drawing.Size(41, 12);
            this.lbName.TabIndex = 7;
            this.lbName.Text = "某某某";
            // 
            // lbIDNum
            // 
            this.lbIDNum.AutoSize = true;
            this.lbIDNum.Location = new System.Drawing.Point(71, 71);
            this.lbIDNum.Name = "lbIDNum";
            this.lbIDNum.Size = new System.Drawing.Size(113, 12);
            this.lbIDNum.TabIndex = 6;
            this.lbIDNum.Text = "320500000000000000";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 71);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "身份证号：";
            // 
            // lbQueNo
            // 
            this.lbQueNo.AutoSize = true;
            this.lbQueNo.Location = new System.Drawing.Point(83, 49);
            this.lbQueNo.Name = "lbQueNo";
            this.lbQueNo.Size = new System.Drawing.Size(29, 12);
            this.lbQueNo.TabIndex = 4;
            this.lbQueNo.Text = "A001";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "叫号信息：";
            // 
            // lbQueNum
            // 
            this.lbQueNum.AutoSize = true;
            this.lbQueNum.Location = new System.Drawing.Point(81, 23);
            this.lbQueNum.Name = "lbQueNum";
            this.lbQueNum.Size = new System.Drawing.Size(11, 12);
            this.lbQueNum.TabIndex = 2;
            this.lbQueNum.Text = "0";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "排队人数：";
            // 
            // lbShowMsg
            // 
            this.lbShowMsg.AutoSize = true;
            this.lbShowMsg.Location = new System.Drawing.Point(3, 0);
            this.lbShowMsg.Name = "lbShowMsg";
            this.lbShowMsg.Size = new System.Drawing.Size(53, 12);
            this.lbShowMsg.TabIndex = 0;
            this.lbShowMsg.Text = "排队信息";
            // 
            // timer_form_hidden
            // 
            this.timer_form_hidden.Interval = 1000;
            this.timer_form_hidden.Tick += new System.EventHandler(this.timer_form_hidden_Tick);
            // 
            // QueueClient
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(250, 256);
            this.Controls.Add(this.panel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "QueueClient";
            this.Text = "叫号客户端";
            this.Load += new System.EventHandler(this.QueueClient_Load);
            this.LocationChanged += new System.EventHandler(this.Main_Monitor_LocationChanged);
            this.panel1.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private sdnControls.sdnButtonEx.ButtonEx btnCallNum;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label lbShowMsg;
        private sdnControls.sdnButtonEx.ButtonEx btnSkip;
        private sdnControls.sdnButtonEx.ButtonEx btnReCall;
        private sdnControls.sdnButtonEx.ButtonEx btnOver;
        private sdnControls.sdnButtonEx.ButtonEx btnHere;
        private sdnControls.sdnButtonEx.ButtonEx btnScore;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lbName;
        private System.Windows.Forms.Label lbIDNum;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lbQueNo;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lbQueNum;
        private System.Windows.Forms.Timer timer_form_hidden;
    }
}