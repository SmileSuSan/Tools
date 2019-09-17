namespace SC
{
    partial class FrmMainUpdatePwd
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
            this.labelControl4 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
            this.txt_NewPwd2 = new DevExpress.XtraEditors.TextEdit();
            this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
            this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
            this.txt_NewPwd1 = new DevExpress.XtraEditors.TextEdit();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.lbl_Areaname = new DevExpress.XtraEditors.LabelControl();
            this.txt_OldPwd = new DevExpress.XtraEditors.TextEdit();
            this.btn_Close = new DevExpress.XtraEditors.SimpleButton();
            this.btn_Save = new DevExpress.XtraEditors.SimpleButton();
            ((System.ComponentModel.ISupportInitialize)(this.txt_NewPwd2.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txt_NewPwd1.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txt_OldPwd.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // labelControl4
            // 
            this.labelControl4.Appearance.ForeColor = System.Drawing.Color.Red;
            this.labelControl4.Location = new System.Drawing.Point(203, 77);
            this.labelControl4.Name = "labelControl4";
            this.labelControl4.Size = new System.Drawing.Size(7, 14);
            this.labelControl4.TabIndex = 58;
            this.labelControl4.Text = "*";
            // 
            // labelControl5
            // 
            this.labelControl5.Location = new System.Drawing.Point(14, 77);
            this.labelControl5.Name = "labelControl5";
            this.labelControl5.Size = new System.Drawing.Size(60, 14);
            this.labelControl5.TabIndex = 57;
            this.labelControl5.Text = "确认新密码";
            // 
            // txt_NewPwd2
            // 
            this.txt_NewPwd2.EnterMoveNextControl = true;
            this.txt_NewPwd2.Location = new System.Drawing.Point(80, 74);
            this.txt_NewPwd2.Name = "txt_NewPwd2";
            this.txt_NewPwd2.Properties.MaxLength = 30;
            this.txt_NewPwd2.Size = new System.Drawing.Size(120, 20);
            this.txt_NewPwd2.TabIndex = 2;
            this.txt_NewPwd2.Tag = "AA_NAME";
            // 
            // labelControl2
            // 
            this.labelControl2.Appearance.ForeColor = System.Drawing.Color.Red;
            this.labelControl2.Location = new System.Drawing.Point(203, 46);
            this.labelControl2.Name = "labelControl2";
            this.labelControl2.Size = new System.Drawing.Size(7, 14);
            this.labelControl2.TabIndex = 56;
            this.labelControl2.Text = "*";
            // 
            // labelControl3
            // 
            this.labelControl3.Location = new System.Drawing.Point(38, 45);
            this.labelControl3.Name = "labelControl3";
            this.labelControl3.Size = new System.Drawing.Size(36, 14);
            this.labelControl3.TabIndex = 55;
            this.labelControl3.Text = "新密码";
            // 
            // txt_NewPwd1
            // 
            this.txt_NewPwd1.EnterMoveNextControl = true;
            this.txt_NewPwd1.Location = new System.Drawing.Point(80, 43);
            this.txt_NewPwd1.Name = "txt_NewPwd1";
            this.txt_NewPwd1.Properties.MaxLength = 30;
            this.txt_NewPwd1.Size = new System.Drawing.Size(120, 20);
            this.txt_NewPwd1.TabIndex = 1;
            this.txt_NewPwd1.Tag = "AA_NAME";
            // 
            // labelControl1
            // 
            this.labelControl1.Appearance.ForeColor = System.Drawing.Color.Red;
            this.labelControl1.Location = new System.Drawing.Point(204, 15);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(7, 14);
            this.labelControl1.TabIndex = 54;
            this.labelControl1.Text = "*";
            // 
            // lbl_Areaname
            // 
            this.lbl_Areaname.Location = new System.Drawing.Point(38, 15);
            this.lbl_Areaname.Name = "lbl_Areaname";
            this.lbl_Areaname.Size = new System.Drawing.Size(36, 14);
            this.lbl_Areaname.TabIndex = 53;
            this.lbl_Areaname.Text = "原密码";
            // 
            // txt_OldPwd
            // 
            this.txt_OldPwd.EnterMoveNextControl = true;
            this.txt_OldPwd.Location = new System.Drawing.Point(80, 12);
            this.txt_OldPwd.Name = "txt_OldPwd";
            this.txt_OldPwd.Properties.MaxLength = 30;
            this.txt_OldPwd.Size = new System.Drawing.Size(120, 20);
            this.txt_OldPwd.TabIndex = 0;
            this.txt_OldPwd.Tag = "AA_NAME";
            // 
            // btn_Close
            // 
            this.btn_Close.Location = new System.Drawing.Point(135, 101);
            this.btn_Close.Name = "btn_Close";
            this.btn_Close.Size = new System.Drawing.Size(65, 23);
            this.btn_Close.TabIndex = 4;
            this.btn_Close.Text = "取　消";
            this.btn_Close.Click += new System.EventHandler(this.btn_Close_Click);
            // 
            // btn_Save
            // 
            this.btn_Save.Location = new System.Drawing.Point(38, 101);
            this.btn_Save.Name = "btn_Save";
            this.btn_Save.Size = new System.Drawing.Size(65, 23);
            this.btn_Save.TabIndex = 3;
            this.btn_Save.Text = "确　定";
            this.btn_Save.Click += new System.EventHandler(this.btn_Save_Click);
            // 
            // FrmMainUpdatePwd
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(230, 135);
            this.Controls.Add(this.labelControl4);
            this.Controls.Add(this.labelControl5);
            this.Controls.Add(this.txt_NewPwd2);
            this.Controls.Add(this.labelControl2);
            this.Controls.Add(this.labelControl3);
            this.Controls.Add(this.txt_NewPwd1);
            this.Controls.Add(this.labelControl1);
            this.Controls.Add(this.lbl_Areaname);
            this.Controls.Add(this.txt_OldPwd);
            this.Controls.Add(this.btn_Close);
            this.Controls.Add(this.btn_Save);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmMainUpdatePwd";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "修改密码";
            this.Load += new System.EventHandler(this.FrmMainUpdatePwd_Load);
            ((System.ComponentModel.ISupportInitialize)(this.txt_NewPwd2.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txt_NewPwd1.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txt_OldPwd.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.LabelControl labelControl4;
        private DevExpress.XtraEditors.LabelControl labelControl5;
        private DevExpress.XtraEditors.TextEdit txt_NewPwd2;
        private DevExpress.XtraEditors.LabelControl labelControl2;
        private DevExpress.XtraEditors.LabelControl labelControl3;
        private DevExpress.XtraEditors.TextEdit txt_NewPwd1;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.LabelControl lbl_Areaname;
        private DevExpress.XtraEditors.TextEdit txt_OldPwd;
        private DevExpress.XtraEditors.SimpleButton btn_Close;
        private DevExpress.XtraEditors.SimpleButton btn_Save;
    }
}