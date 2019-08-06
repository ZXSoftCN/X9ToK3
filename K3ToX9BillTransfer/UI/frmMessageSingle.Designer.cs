namespace K3ToX9BillTransfer.UI
{
    partial class frmMessageSingle
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
            this.lbErrorInfo = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnShowAllInfor = new System.Windows.Forms.Button();
            this.txtAllInfo = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lbErrorInfo);
            this.panel1.Controls.Add(this.btnClose);
            this.panel1.Controls.Add(this.btnShowAllInfor);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(437, 97);
            this.panel1.TabIndex = 0;
            // 
            // lbErrorInfo
            // 
            this.lbErrorInfo.Font = new System.Drawing.Font("宋体", 10.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lbErrorInfo.Location = new System.Drawing.Point(27, 20);
            this.lbErrorInfo.Name = "lbErrorInfo";
            this.lbErrorInfo.Size = new System.Drawing.Size(392, 47);
            this.lbErrorInfo.TabIndex = 2;
            this.lbErrorInfo.Text = "错误信息";
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(344, 70);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "关闭";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnShowAllInfor
            // 
            this.btnShowAllInfor.Location = new System.Drawing.Point(234, 70);
            this.btnShowAllInfor.Name = "btnShowAllInfor";
            this.btnShowAllInfor.Size = new System.Drawing.Size(88, 23);
            this.btnShowAllInfor.TabIndex = 0;
            this.btnShowAllInfor.Text = "显示详细信息";
            this.btnShowAllInfor.UseVisualStyleBackColor = true;
            this.btnShowAllInfor.Click += new System.EventHandler(this.btnShowAllInfo_Click);
            // 
            // txtAllInfo
            // 
            this.txtAllInfo.BackColor = System.Drawing.SystemColors.Window;
            this.txtAllInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtAllInfo.Location = new System.Drawing.Point(0, 97);
            this.txtAllInfo.Multiline = true;
            this.txtAllInfo.Name = "txtAllInfo";
            this.txtAllInfo.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtAllInfo.Size = new System.Drawing.Size(437, 0);
            this.txtAllInfo.TabIndex = 1;
            this.txtAllInfo.Visible = false;
            // 
            // frmMessageSingle
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(437, 97);
            this.Controls.Add(this.txtAllInfo);
            this.Controls.Add(this.panel1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmMessageSingle";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "提示";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.frmMessageSingle_Load);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnShowAllInfor;
        private System.Windows.Forms.TextBox txtAllInfo;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label lbErrorInfo;

    }
}