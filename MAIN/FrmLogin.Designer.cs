namespace SC
{
    partial class FrmLogin
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
            this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
            this.txt_E_NAME = new DevExpress.XtraEditors.TextEdit();
            this.labelControl6 = new DevExpress.XtraEditors.LabelControl();
            this.txt_E_PWD = new DevExpress.XtraEditors.TextEdit();
            this.Btn_Login = new DevExpress.XtraEditors.SimpleButton();
            this.btn_User = new DevExpress.XtraEditors.SimpleButton();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.txt_E_NAME.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txt_E_PWD.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // labelControl5
            // 
            this.labelControl5.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.labelControl5.Appearance.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelControl5.Location = new System.Drawing.Point(37, 172);
            this.labelControl5.Name = "labelControl5";
            this.labelControl5.Size = new System.Drawing.Size(32, 21);
            this.labelControl5.TabIndex = 30;
            this.labelControl5.Text = "账号";
            // 
            // txt_E_NAME
            // 
            this.txt_E_NAME.EditValue = "";
            this.txt_E_NAME.EnterMoveNextControl = true;
            this.txt_E_NAME.Location = new System.Drawing.Point(75, 170);
            this.txt_E_NAME.Name = "txt_E_NAME";
            this.txt_E_NAME.Properties.Appearance.BorderColor = System.Drawing.Color.Gray;
            this.txt_E_NAME.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_E_NAME.Properties.Appearance.ForeColor = System.Drawing.Color.DimGray;
            this.txt_E_NAME.Properties.Appearance.Options.UseBorderColor = true;
            this.txt_E_NAME.Properties.Appearance.Options.UseFont = true;
            this.txt_E_NAME.Properties.Appearance.Options.UseForeColor = true;
            this.txt_E_NAME.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.txt_E_NAME.Size = new System.Drawing.Size(164, 26);
            this.txt_E_NAME.TabIndex = 1;
            this.txt_E_NAME.Tag = "E_NAME";
            // 
            // labelControl6
            // 
            this.labelControl6.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.labelControl6.Appearance.Font = new System.Drawing.Font("微软雅黑", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelControl6.Location = new System.Drawing.Point(37, 211);
            this.labelControl6.Name = "labelControl6";
            this.labelControl6.Size = new System.Drawing.Size(32, 21);
            this.labelControl6.TabIndex = 31;
            this.labelControl6.Text = "密码";
            // 
            // txt_E_PWD
            // 
            this.txt_E_PWD.EditValue = "";
            this.txt_E_PWD.Location = new System.Drawing.Point(75, 209);
            this.txt_E_PWD.Name = "txt_E_PWD";
            this.txt_E_PWD.Properties.Appearance.BorderColor = System.Drawing.Color.Gray;
            this.txt_E_PWD.Properties.Appearance.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_E_PWD.Properties.Appearance.ForeColor = System.Drawing.Color.DimGray;
            this.txt_E_PWD.Properties.Appearance.Options.UseBorderColor = true;
            this.txt_E_PWD.Properties.Appearance.Options.UseFont = true;
            this.txt_E_PWD.Properties.Appearance.Options.UseForeColor = true;
            this.txt_E_PWD.Properties.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.txt_E_PWD.Properties.PasswordChar = '*';
            this.txt_E_PWD.Size = new System.Drawing.Size(164, 26);
            this.txt_E_PWD.TabIndex = 2;
            this.txt_E_PWD.Tag = "E_PWD";
            this.txt_E_PWD.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txt_E_PWD_KeyDown);
            // 
            // Btn_Login
            // 
            this.Btn_Login.Appearance.BackColor = System.Drawing.Color.White;
            this.Btn_Login.Appearance.Font = new System.Drawing.Font("Tahoma", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Btn_Login.Appearance.Options.UseBackColor = true;
            this.Btn_Login.Appearance.Options.UseFont = true;
            this.Btn_Login.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.UltraFlat;
            this.Btn_Login.Image = global::SC.Properties.Resources.login1;
            this.Btn_Login.Location = new System.Drawing.Point(28, 275);
            this.Btn_Login.Name = "Btn_Login";
            this.Btn_Login.Size = new System.Drawing.Size(108, 32);
            this.Btn_Login.TabIndex = 54;
            this.Btn_Login.Text = "登陆";
            this.Btn_Login.Click += new System.EventHandler(this.Btn_Login_Click);
            // 
            // btn_User
            // 
            this.btn_User.Appearance.BackColor = System.Drawing.Color.White;
            this.btn_User.Appearance.Options.UseBackColor = true;
            this.btn_User.ButtonStyle = DevExpress.XtraEditors.Controls.BorderStyles.UltraFlat;
            this.btn_User.Image = global::SC.Properties.Resources.client_account_template;
            this.btn_User.Location = new System.Drawing.Point(245, 172);
            this.btn_User.Name = "btn_User";
            this.btn_User.Size = new System.Drawing.Size(20, 16);
            this.btn_User.TabIndex = 58;
            this.btn_User.Click += new System.EventHandler(this.btn_User_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Image = global::SC.Properties.Resources.bgimag;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(736, 405);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 56;
            this.pictureBox1.TabStop = false;
            // 
            // FrmLogin
            // 
            this.Appearance.BackColor = System.Drawing.Color.White;
            this.Appearance.Options.UseBackColor = true;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(736, 405);
            this.Controls.Add(this.Btn_Login);
            this.Controls.Add(this.btn_User);
            this.Controls.Add(this.labelControl5);
            this.Controls.Add(this.txt_E_NAME);
            this.Controls.Add(this.labelControl6);
            this.Controls.Add(this.txt_E_PWD);
            this.Controls.Add(this.pictureBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FrmLogin";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "账本管理系统";
            this.Activated += new System.EventHandler(this.FrmLogin_Activated);
            this.Load += new System.EventHandler(this.FrmLogin_Load);
            ((System.ComponentModel.ISupportInitialize)(this.txt_E_NAME.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txt_E_PWD.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DevExpress.XtraEditors.LabelControl labelControl5;
        private DevExpress.XtraEditors.TextEdit txt_E_NAME;
        private DevExpress.XtraEditors.LabelControl labelControl6;
        private DevExpress.XtraEditors.SimpleButton Btn_Login;
        private System.Windows.Forms.PictureBox pictureBox1;
        private DevExpress.XtraEditors.TextEdit txt_E_PWD;
        private DevExpress.XtraEditors.SimpleButton btn_User;

    }
}