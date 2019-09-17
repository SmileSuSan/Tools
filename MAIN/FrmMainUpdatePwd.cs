using DevExpress.XtraEditors;
using K9;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace SC
{
    public partial class FrmMainUpdatePwd : BaseFrm
    {
        public FrmMainUpdatePwd()
        {
            InitializeComponent();
        }

        public string password = string.Empty;//原始密码
        public bool isSuccess = false;//修改密码是否成功标识

        //加载
        private void FrmMainUpdatePwd_Load(object sender, EventArgs e)
        {
            this.txt_OldPwd.Properties.PasswordChar = '*';
            this.txt_NewPwd1.Properties.PasswordChar = '*';
            this.txt_NewPwd2.Properties.PasswordChar = '*';
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Save_Click(object sender, EventArgs e)
        {
            try
            {
                string pwds = "";
                if (string.IsNullOrWhiteSpace(this.txt_OldPwd.Text))
                {
                    this.txt_OldPwd.Focus();
                    XtraMessageBox.Show("原密码不能为空！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(this.txt_NewPwd1.Text))
                {
                    this.txt_NewPwd1.Focus();
                    XtraMessageBox.Show("新密码不能为空！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (BaseInfoCommon.CurrentUser.EMPLOYEE_CODE == "k9admin" && this.txt_NewPwd1.Text.Length < 8)
                {
                    this.txt_NewPwd1.Focus();
                    XtraMessageBox.Show("管理员新密码长度不能少于8位！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (this.txt_NewPwd1.Text.Length < 6)
                {
                    this.txt_NewPwd1.Focus();
                    XtraMessageBox.Show("新密码长度不能少于6位！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (this.txt_NewPwd1.Text.Length > 20)
                {
                    this.txt_NewPwd1.Focus();
                    XtraMessageBox.Show("新密码长度不能大于20位！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (Utility.IsAllNumber(this.txt_NewPwd1.Text.Trim()) || IsEnCh(this.txt_NewPwd1.Text.Trim()))
                {
                    this.txt_NewPwd1.Focus();
                    XtraMessageBox.Show("新密码不能为纯数字或纯字母！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (this.txt_NewPwd1.Text.Trim() == BaseInfoCommon.CurrentUser.EMPLOYEE_CODE)
                {
                    this.txt_NewPwd1.Focus();
                    XtraMessageBox.Show("密码不能与账号相同！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                Dictionary<string, string> dis = new Dictionary<string, string>();
                dis.Add("EMPLOYEE_CODE", BaseInfoCommon.CurrentUser.EMPLOYEE_CODE);
                string strJson = JsonConvertHelper.DicToTableJson(dis, "TAB_EMPLOYEE_QRY");
                DataSet dsResult = HttpHelper.TransData(ServerName.K9, "qryTAB_EMPLOYEE", strJson);
                if (Utility.IsTranOK(dsResult))
                {
                    if (Utility.GetRowCount(dsResult) > 0)
                    {
                        password = dsResult.Tables["TAB_EMPLOYEE"].Rows[0]["E_PWD"].ToString();
                        pwds = dsResult.Tables["TAB_EMPLOYEE"].Rows[0]["E_PWDS"].ToString();
                    }
                    else
                    {
                        XtraMessageBox.Show("获取员工信息失败！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
                else
                {
                    XtraMessageBox.Show("访问服务器失败！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (password != Utility.Encrypt(txt_OldPwd.Text))
                {
                    this.txt_OldPwd.Focus();
                    XtraMessageBox.Show("原密码错误！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (txt_OldPwd.Text == txt_NewPwd1.Text)
                {
                    this.txt_OldPwd.Focus();
                    XtraMessageBox.Show("新密码不能跟原密码相同！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (Utility.IsAllNumber(this.txt_NewPwd1.Text.Trim()))
                {
                    this.txt_NewPwd1.Focus();
                    XtraMessageBox.Show("新密码不能为纯数字！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (this.txt_NewPwd1.Text.Trim() == BaseInfoCommon.CurrentUser.EMPLOYEE_CODE)
                {
                    this.txt_NewPwd1.Focus();
                    XtraMessageBox.Show("新密码不能与账号相同！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (pwds.Contains(Utility.Encrypt(this.txt_NewPwd1.Text.Trim()) + ";"))
                {
                    this.txt_NewPwd1.Focus();
                    XtraMessageBox.Show("不能与最近设置的三次旧密码相同！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(this.txt_NewPwd2.Text))
                {
                    this.txt_NewPwd2.Focus();
                    XtraMessageBox.Show("确认新密码不能为空！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (Utility.IsAllNumber(this.txt_NewPwd2.Text.Trim()))
                {
                    this.txt_NewPwd2.Focus();
                    XtraMessageBox.Show("确认新密码不能为纯数字！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (!this.txt_NewPwd1.Text.Trim().Equals(this.txt_NewPwd2.Text.Trim()))
                {
                    this.txt_NewPwd1.Focus();
                    XtraMessageBox.Show("两次输入的新密码不一致！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("OWNER_SITE", dsResult.Tables["TAB_EMPLOYEE"].Rows[0]["OWNER_SITE"].ToString());
                dic.Add("EMPLOYEE_CODE", dsResult.Tables["TAB_EMPLOYEE"].Rows[0]["EMPLOYEE_CODE"].ToString());
                dic.Add("E_PWD", Utility.Encrypt(this.txt_NewPwd1.Text.Trim()));
                dic.Add("CHECK_PWD", "1");
                dic.Add("MODIFY_MAN", BaseInfoCommon.CurrentUser.EMPLOYEE_NAME);
                dic.Add("MODIFY_SITE", BaseInfoCommon.CurrentUser.OWNER_SITE);
                if (string.IsNullOrWhiteSpace(pwds))
                {
                    pwds = Utility.Encrypt(this.txt_NewPwd1.Text.Trim()) + ";";
                }
                else
                {
                    string[] ps = pwds.TrimEnd(';').Split(';');
                    if (ps.Length <= 2)
                    {
                        pwds = pwds + Utility.Encrypt(this.txt_NewPwd1.Text.Trim()) + ";";
                    }
                    else
                    {
                        pwds = ps[1] + ps[2] + ";" + Utility.Encrypt(this.txt_NewPwd1.Text.Trim()) + ";";
                    }
                }
                dic.Add("E_PWDS", pwds);
                string strJsonPwd = JsonConvertHelper.DicToTableJson(dic, "TAB_EMPLOYEE_UPT");
                DataSet ds = HttpHelper.TransData(ServerName.K9, "saveTAB_EMPLOYEE", strJsonPwd);
                if (Utility.IsTranOK(ds))
                {
                    isSuccess = true;
                    XtraMessageBox.Show("密码修改成功！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.Close();
                }
                else
                {
                    isSuccess = false;
                    XtraMessageBox.Show("密码修改失败！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("查询员工报错：", ex);
                XtraMessageBox.Show(ex.Message, "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_Close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>  
        /// 判断输入的字符串是否只包含英文字母  
        /// </summary>  
        /// <param name="input"></param>  
        /// <returns></returns>  
        public static bool IsEnCh(string input)
        {
            string pattern = @"^[A-Za-z]+$";
            Regex regex = new Regex(pattern);
            return regex.IsMatch(input);
        }
    }
}
