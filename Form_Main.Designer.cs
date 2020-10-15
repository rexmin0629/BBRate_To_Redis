namespace BBRate_To_Redis
{
    partial class Form_Main
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器
        /// 修改這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tsslb_IPushStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.tsslb_RedisStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.gb_IPush = new System.Windows.Forms.GroupBox();
            this.txt_IPushDelay = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.txt_IPushPassword = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txt_IPushUser = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txt_IPushProduct = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txt_IPushCompany = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.txt_IPushPort = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txt_IPushIP = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.chkbtn_Connect = new System.Windows.Forms.CheckBox();
            this.tabControl_Main = new System.Windows.Forms.TabControl();
            this.tp_Log = new System.Windows.Forms.TabPage();
            this.rtb_Log = new System.Windows.Forms.RichTextBox();
            this.gb_Redis = new System.Windows.Forms.GroupBox();
            this.lb_Redis_DB = new System.Windows.Forms.Label();
            this.lb_Redis_Channel = new System.Windows.Forms.Label();
            this.lb_Redis_Port = new System.Windows.Forms.Label();
            this.lb_Redis_IP = new System.Windows.Forms.Label();
            this.chkbtn_ShowLog = new System.Windows.Forms.CheckBox();
            this.lb_LogTimeLast = new System.Windows.Forms.Label();
            this.statusStrip1.SuspendLayout();
            this.gb_IPush.SuspendLayout();
            this.tabControl_Main.SuspendLayout();
            this.tp_Log.SuspendLayout();
            this.gb_Redis.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsslb_IPushStatus,
            this.tsslb_RedisStatus});
            this.statusStrip1.Location = new System.Drawing.Point(0, 438);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1380, 24);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tsslb_IPushStatus
            // 
            this.tsslb_IPushStatus.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.tsslb_IPushStatus.ForeColor = System.Drawing.Color.Red;
            this.tsslb_IPushStatus.Name = "tsslb_IPushStatus";
            this.tsslb_IPushStatus.Size = new System.Drawing.Size(127, 19);
            this.tsslb_IPushStatus.Text = "IPush:UnConnect";
            // 
            // tsslb_RedisStatus
            // 
            this.tsslb_RedisStatus.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.tsslb_RedisStatus.ForeColor = System.Drawing.Color.Red;
            this.tsslb_RedisStatus.Name = "tsslb_RedisStatus";
            this.tsslb_RedisStatus.Size = new System.Drawing.Size(128, 19);
            this.tsslb_RedisStatus.Text = "Redis:UnConnect";
            // 
            // gb_IPush
            // 
            this.gb_IPush.Controls.Add(this.txt_IPushDelay);
            this.gb_IPush.Controls.Add(this.label7);
            this.gb_IPush.Controls.Add(this.txt_IPushPassword);
            this.gb_IPush.Controls.Add(this.label6);
            this.gb_IPush.Controls.Add(this.txt_IPushUser);
            this.gb_IPush.Controls.Add(this.label5);
            this.gb_IPush.Controls.Add(this.txt_IPushProduct);
            this.gb_IPush.Controls.Add(this.label4);
            this.gb_IPush.Controls.Add(this.txt_IPushCompany);
            this.gb_IPush.Controls.Add(this.label3);
            this.gb_IPush.Controls.Add(this.txt_IPushPort);
            this.gb_IPush.Controls.Add(this.label2);
            this.gb_IPush.Controls.Add(this.txt_IPushIP);
            this.gb_IPush.Controls.Add(this.label1);
            this.gb_IPush.Controls.Add(this.chkbtn_Connect);
            this.gb_IPush.Location = new System.Drawing.Point(12, 12);
            this.gb_IPush.Name = "gb_IPush";
            this.gb_IPush.Size = new System.Drawing.Size(256, 269);
            this.gb_IPush.TabIndex = 1;
            this.gb_IPush.TabStop = false;
            this.gb_IPush.Text = "IPush 連線";
            // 
            // txt_IPushDelay
            // 
            this.txt_IPushDelay.Location = new System.Drawing.Point(81, 235);
            this.txt_IPushDelay.Name = "txt_IPushDelay";
            this.txt_IPushDelay.Size = new System.Drawing.Size(165, 22);
            this.txt_IPushDelay.TabIndex = 16;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 238);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(38, 12);
            this.label7.TabIndex = 15;
            this.label7.Text = "Delay :";
            // 
            // txt_IPushPassword
            // 
            this.txt_IPushPassword.Location = new System.Drawing.Point(81, 207);
            this.txt_IPushPassword.Name = "txt_IPushPassword";
            this.txt_IPushPassword.Size = new System.Drawing.Size(165, 22);
            this.txt_IPushPassword.TabIndex = 14;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 210);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(54, 12);
            this.label6.TabIndex = 13;
            this.label6.Text = "Password :";
            // 
            // txt_IPushUser
            // 
            this.txt_IPushUser.Location = new System.Drawing.Point(81, 179);
            this.txt_IPushUser.Name = "txt_IPushUser";
            this.txt_IPushUser.Size = new System.Drawing.Size(165, 22);
            this.txt_IPushUser.TabIndex = 12;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 182);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(32, 12);
            this.label5.TabIndex = 11;
            this.label5.Text = "User :";
            // 
            // txt_IPushProduct
            // 
            this.txt_IPushProduct.Location = new System.Drawing.Point(81, 151);
            this.txt_IPushProduct.Name = "txt_IPushProduct";
            this.txt_IPushProduct.Size = new System.Drawing.Size(165, 22);
            this.txt_IPushProduct.TabIndex = 10;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 154);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(47, 12);
            this.label4.TabIndex = 9;
            this.label4.Text = "Product :";
            // 
            // txt_IPushCompany
            // 
            this.txt_IPushCompany.Location = new System.Drawing.Point(81, 123);
            this.txt_IPushCompany.Name = "txt_IPushCompany";
            this.txt_IPushCompany.Size = new System.Drawing.Size(165, 22);
            this.txt_IPushCompany.TabIndex = 8;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 126);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(57, 12);
            this.label3.TabIndex = 7;
            this.label3.Text = "Company :";
            // 
            // txt_IPushPort
            // 
            this.txt_IPushPort.Location = new System.Drawing.Point(81, 95);
            this.txt_IPushPort.Name = "txt_IPushPort";
            this.txt_IPushPort.Size = new System.Drawing.Size(165, 22);
            this.txt_IPushPort.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 98);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(30, 12);
            this.label2.TabIndex = 5;
            this.label2.Text = "Port :";
            // 
            // txt_IPushIP
            // 
            this.txt_IPushIP.Location = new System.Drawing.Point(81, 67);
            this.txt_IPushIP.Name = "txt_IPushIP";
            this.txt_IPushIP.Size = new System.Drawing.Size(165, 22);
            this.txt_IPushIP.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 70);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(21, 12);
            this.label1.TabIndex = 3;
            this.label1.Text = "IP :";
            // 
            // chkbtn_Connect
            // 
            this.chkbtn_Connect.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkbtn_Connect.Location = new System.Drawing.Point(6, 27);
            this.chkbtn_Connect.Name = "chkbtn_Connect";
            this.chkbtn_Connect.Size = new System.Drawing.Size(96, 23);
            this.chkbtn_Connect.TabIndex = 2;
            this.chkbtn_Connect.Text = "IPush連線";
            this.chkbtn_Connect.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chkbtn_Connect.UseVisualStyleBackColor = true;
            this.chkbtn_Connect.Click += new System.EventHandler(this.chkbtn_Connect_Click);
            // 
            // tabControl_Main
            // 
            this.tabControl_Main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_Main.Controls.Add(this.tp_Log);
            this.tabControl_Main.Location = new System.Drawing.Point(274, 1);
            this.tabControl_Main.Name = "tabControl_Main";
            this.tabControl_Main.SelectedIndex = 0;
            this.tabControl_Main.Size = new System.Drawing.Size(1106, 434);
            this.tabControl_Main.TabIndex = 2;
            // 
            // tp_Log
            // 
            this.tp_Log.BackColor = System.Drawing.SystemColors.Control;
            this.tp_Log.Controls.Add(this.rtb_Log);
            this.tp_Log.Location = new System.Drawing.Point(4, 22);
            this.tp_Log.Name = "tp_Log";
            this.tp_Log.Padding = new System.Windows.Forms.Padding(3);
            this.tp_Log.Size = new System.Drawing.Size(1098, 408);
            this.tp_Log.TabIndex = 0;
            this.tp_Log.Text = "Log";
            // 
            // rtb_Log
            // 
            this.rtb_Log.BackColor = System.Drawing.SystemColors.Control;
            this.rtb_Log.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtb_Log.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.rtb_Log.Location = new System.Drawing.Point(3, 3);
            this.rtb_Log.Name = "rtb_Log";
            this.rtb_Log.ReadOnly = true;
            this.rtb_Log.Size = new System.Drawing.Size(1092, 402);
            this.rtb_Log.TabIndex = 2;
            this.rtb_Log.Text = "";
            // 
            // gb_Redis
            // 
            this.gb_Redis.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.gb_Redis.Controls.Add(this.lb_Redis_DB);
            this.gb_Redis.Controls.Add(this.lb_Redis_Channel);
            this.gb_Redis.Controls.Add(this.lb_Redis_Port);
            this.gb_Redis.Controls.Add(this.lb_Redis_IP);
            this.gb_Redis.Location = new System.Drawing.Point(12, 287);
            this.gb_Redis.Name = "gb_Redis";
            this.gb_Redis.Size = new System.Drawing.Size(256, 112);
            this.gb_Redis.TabIndex = 3;
            this.gb_Redis.TabStop = false;
            this.gb_Redis.Text = "Redis";
            // 
            // lb_Redis_DB
            // 
            this.lb_Redis_DB.AutoSize = true;
            this.lb_Redis_DB.Location = new System.Drawing.Point(115, 57);
            this.lb_Redis_DB.Name = "lb_Redis_DB";
            this.lb_Redis_DB.Size = new System.Drawing.Size(27, 12);
            this.lb_Redis_DB.TabIndex = 19;
            this.lb_Redis_DB.Text = "DB :";
            // 
            // lb_Redis_Channel
            // 
            this.lb_Redis_Channel.AutoSize = true;
            this.lb_Redis_Channel.Location = new System.Drawing.Point(115, 29);
            this.lb_Redis_Channel.Name = "lb_Redis_Channel";
            this.lb_Redis_Channel.Size = new System.Drawing.Size(50, 12);
            this.lb_Redis_Channel.TabIndex = 18;
            this.lb_Redis_Channel.Text = "Channel :";
            // 
            // lb_Redis_Port
            // 
            this.lb_Redis_Port.AutoSize = true;
            this.lb_Redis_Port.Location = new System.Drawing.Point(6, 57);
            this.lb_Redis_Port.Name = "lb_Redis_Port";
            this.lb_Redis_Port.Size = new System.Drawing.Size(30, 12);
            this.lb_Redis_Port.TabIndex = 17;
            this.lb_Redis_Port.Text = "Port :";
            // 
            // lb_Redis_IP
            // 
            this.lb_Redis_IP.AutoSize = true;
            this.lb_Redis_IP.Location = new System.Drawing.Point(6, 29);
            this.lb_Redis_IP.Name = "lb_Redis_IP";
            this.lb_Redis_IP.Size = new System.Drawing.Size(21, 12);
            this.lb_Redis_IP.TabIndex = 16;
            this.lb_Redis_IP.Text = "IP :";
            // 
            // chkbtn_ShowLog
            // 
            this.chkbtn_ShowLog.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkbtn_ShowLog.Appearance = System.Windows.Forms.Appearance.Button;
            this.chkbtn_ShowLog.Location = new System.Drawing.Point(193, 405);
            this.chkbtn_ShowLog.Name = "chkbtn_ShowLog";
            this.chkbtn_ShowLog.Size = new System.Drawing.Size(75, 23);
            this.chkbtn_ShowLog.TabIndex = 18;
            this.chkbtn_ShowLog.Text = "顯示Log";
            this.chkbtn_ShowLog.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.chkbtn_ShowLog.UseVisualStyleBackColor = true;
            this.chkbtn_ShowLog.Click += new System.EventHandler(this.chkbtn_ShowLog_Click);
            // 
            // lb_LogTimeLast
            // 
            this.lb_LogTimeLast.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lb_LogTimeLast.AutoSize = true;
            this.lb_LogTimeLast.Location = new System.Drawing.Point(10, 410);
            this.lb_LogTimeLast.Name = "lb_LogTimeLast";
            this.lb_LogTimeLast.Size = new System.Drawing.Size(94, 12);
            this.lb_LogTimeLast.TabIndex = 19;
            this.lb_LogTimeLast.Text = "Publish Log時間 : ";
            // 
            // Form_Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1380, 462);
            this.Controls.Add(this.lb_LogTimeLast);
            this.Controls.Add(this.gb_IPush);
            this.Controls.Add(this.chkbtn_ShowLog);
            this.Controls.Add(this.gb_Redis);
            this.Controls.Add(this.tabControl_Main);
            this.Controls.Add(this.statusStrip1);
            this.MinimumSize = new System.Drawing.Size(276, 100);
            this.Name = "Form_Main";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form_Main";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_Main_FormClosing);
            this.Load += new System.EventHandler(this.Form_Main_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.gb_IPush.ResumeLayout(false);
            this.gb_IPush.PerformLayout();
            this.tabControl_Main.ResumeLayout(false);
            this.tp_Log.ResumeLayout(false);
            this.gb_Redis.ResumeLayout(false);
            this.gb_Redis.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.GroupBox gb_IPush;
        private System.Windows.Forms.TabControl tabControl_Main;
        private System.Windows.Forms.TabPage tp_Log;
        private System.Windows.Forms.RichTextBox rtb_Log;
        private System.Windows.Forms.CheckBox chkbtn_Connect;
        private System.Windows.Forms.ToolStripStatusLabel tsslb_IPushStatus;
        private System.Windows.Forms.TextBox txt_IPushIP;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txt_IPushCompany;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txt_IPushPort;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txt_IPushDelay;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txt_IPushPassword;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txt_IPushUser;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txt_IPushProduct;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox gb_Redis;
        private System.Windows.Forms.Label lb_Redis_Port;
        private System.Windows.Forms.Label lb_Redis_IP;
        private System.Windows.Forms.Label lb_Redis_Channel;
        private System.Windows.Forms.ToolStripStatusLabel tsslb_RedisStatus;
        private System.Windows.Forms.CheckBox chkbtn_ShowLog;
        private System.Windows.Forms.Label lb_LogTimeLast;
        private System.Windows.Forms.Label lb_Redis_DB;
    }
}

