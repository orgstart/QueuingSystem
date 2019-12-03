namespace WinControlServer
{
    partial class wj_pj
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(wj_pj));
            this.btn5Star = new System.Windows.Forms.Button();
            this.btn4Star = new System.Windows.Forms.Button();
            this.btn3Star = new System.Windows.Forms.Button();
            this.btn2Star = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btn5Star
            // 
            this.btn5Star.BackColor = System.Drawing.Color.Transparent;
            this.btn5Star.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btn5Star.BackgroundImage")));
            this.btn5Star.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn5Star.Location = new System.Drawing.Point(236, 155);
            this.btn5Star.Name = "btn5Star";
            this.btn5Star.Size = new System.Drawing.Size(290, 99);
            this.btn5Star.TabIndex = 0;
            this.btn5Star.UseVisualStyleBackColor = false;
            this.btn5Star.Click += new System.EventHandler(this.btn5Star_Click);
            // 
            // btn4Star
            // 
            this.btn4Star.BackColor = System.Drawing.Color.Transparent;
            this.btn4Star.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btn4Star.BackgroundImage")));
            this.btn4Star.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn4Star.Location = new System.Drawing.Point(236, 260);
            this.btn4Star.Name = "btn4Star";
            this.btn4Star.Size = new System.Drawing.Size(89, 77);
            this.btn4Star.TabIndex = 1;
            this.btn4Star.UseVisualStyleBackColor = false;
            this.btn4Star.Click += new System.EventHandler(this.btn4Star_Click);
            // 
            // btn3Star
            // 
            this.btn3Star.BackColor = System.Drawing.Color.Transparent;
            this.btn3Star.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btn3Star.BackgroundImage")));
            this.btn3Star.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn3Star.Location = new System.Drawing.Point(338, 260);
            this.btn3Star.Name = "btn3Star";
            this.btn3Star.Size = new System.Drawing.Size(89, 77);
            this.btn3Star.TabIndex = 2;
            this.btn3Star.UseVisualStyleBackColor = false;
            this.btn3Star.Click += new System.EventHandler(this.btn3Star_Click);
            // 
            // btn2Star
            // 
            this.btn2Star.BackColor = System.Drawing.Color.Transparent;
            this.btn2Star.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btn2Star.BackgroundImage")));
            this.btn2Star.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btn2Star.Location = new System.Drawing.Point(437, 260);
            this.btn2Star.Name = "btn2Star";
            this.btn2Star.Size = new System.Drawing.Size(89, 77);
            this.btn2Star.TabIndex = 3;
            this.btn2Star.UseVisualStyleBackColor = false;
            this.btn2Star.Click += new System.EventHandler(this.btn2Star_Click);
            // 
            // wj_pj
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(784, 411);
            this.Controls.Add(this.btn2Star);
            this.Controls.Add(this.btn3Star);
            this.Controls.Add(this.btn4Star);
            this.Controls.Add(this.btn5Star);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "wj_pj";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "评价";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.wj_pj_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btn5Star;
        private System.Windows.Forms.Button btn4Star;
        private System.Windows.Forms.Button btn3Star;
        private System.Windows.Forms.Button btn2Star;
    }
}