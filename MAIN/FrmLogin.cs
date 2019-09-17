using DevExpress.LookAndFeel;
using DevExpress.XtraBars.Docking2010;
using DevExpress.XtraBars.Localization;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraGrid.Localization;
using DevExpress.XtraPrinting.Localization;
using DevExpress.XtraRichEdit.Localization;
using ICSharpCode.SharpZipLib.Zip;
using K9;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace SC
{
    public partial class FrmLogin : BaseFrm
    {
        public FrmLogin()
        {
            InitializeComponent();
        }

        SQLiteBusiness SqliteDB = null;
        SQLiteBusiness sh = null;
        SQLiteBusiness addressSql = null;

        public string site = "";
        public string username = "";
        public string password = "";
        public int Flag = 1;
        public bool isFlag = false;

        string dbName = "K9YYJR.db";//本地连接信息表
        DataTable dtLocalParams = null;
        DataTable dbInfo = null; //登录用户历史信息
        DataTable AddressDt = new DataTable();

        ContextMenuStrip cms = new ContextMenuStrip(); //批量修改数据菜单

        bool isUpdate = false;

        /// <summary>
        /// 重构始化页面
        /// </summary>
        /// <param name="uname"></param>
        /// <param name="pwd"></param>
        public FrmLogin(string uname, string pwd, string st, string index)
        {
            username = uname;
            password = pwd;
            site = st;
            Flag = int.Parse(index);
            InitializeComponent();
        }

        /// <summary>
        /// 加载事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmLogin_Load(object sender, EventArgs e)
        {
            try
            {
                #region 操作系统位数判断,用不同sqlite
                if (Environment.Is64BitOperatingSystem)
                {
                    //检查是否存在目的目录
                    string filepath = AppDomain.CurrentDomain.BaseDirectory.ToString() + "\\Sqlite\\Sqlite64\\System.Data.SQLite.dll";
                    if (File.Exists(filepath))
                    {
                        //复制文件
                        File.Copy(filepath, AppDomain.CurrentDomain.BaseDirectory.ToString() + "\\System.Data.SQLite.dll", true);
                    }
                }
                #endregion

                //DevExpress控件本地化
                DocumentManagerLocalizer.Active = new ChineseDocumentManagerLocalizer();
                Localizer.Active = new ChineseXtraEditorsLocalizer();
                BarLocalizer.Active = new ChineseXtreBarsLocalizer();
                GridLocalizer.Active = new XtraGridLocalizer_zh_chs();
                PreviewLocalizer.Active = new MyPreviewLocalizer();
                XtraRichEditLocalizer.Active = new MyRichEditLocalizer();
                DevExpress.UserSkins.BonusSkins.Register();
                DevExpress.Skins.SkinManager.EnableFormSkins();
                DevExpress.Skins.SkinManager.EnableMdiFormSkins();

                UserLookAndFeel.Default.SetDefaultStyle();

                #region 创建数据库连接
                addressSql = new SQLiteBusiness(string.Format(@"Data Source={0}" + dbName, AppDomain.CurrentDomain.BaseDirectory.ToString()));
                addressSql.CreateDB();
                int flg1 = addressSql.ValidateIsInt("CONFIG_YYJR1");
                if (flg1 == 0)
                {
                    AddressDt.Columns.Add("ID");
                    AddressDt.Columns.Add("ADDRESS");
                    AddressDt.Columns.Add("PORT");
                    AddressDt.Columns.Add("REMARK");
                    AddressDt.Columns.Add("FLG");

                    //string str_address = "jrwldg.cfss.net.cn";
                    string str_address = "jrwlcs.cfss.net.cn";
                    //string str_address = "220.231.140.35";

                    DataRow row1 = AddressDt.NewRow();
                    row1["ID"] = "0";
                    row1["ADDRESS"] = str_address;
                    row1["PORT"] = "8989";
                    row1["REMARK"] = "域名";
                    row1["FLG"] = "0";

                    DataRow row2 = AddressDt.NewRow();
                    row2["ID"] = "1";
                    row2["ADDRESS"] = str_address;
                    row2["PORT"] = "8989";
                    row2["REMARK"] = "联通";
                    row2["FLG"] = "0";

                    DataRow row3 = AddressDt.NewRow();
                    row3["ID"] = "2";
                    row3["ADDRESS"] = str_address;
                    row3["PORT"] = "8989";
                    row3["REMARK"] = "电信";
                    row3["FLG"] = "0";

                    DataRow row4 = AddressDt.NewRow();
                    row4["ID"] = "3";
                    row4["ADDRESS"] = str_address;
                    row4["PORT"] = "8989";
                    row4["REMARK"] = "移动";
                    row4["FLG"] = "0";

                    AddressDt.Rows.Add(row1);
                    AddressDt.Rows.Add(row2);
                    AddressDt.Rows.Add(row3);
                    AddressDt.Rows.Add(row4);

                    addressSql.AddNewTable(AddressDt, "CONFIG_YYJR1");
                }
                else
                {
                    AddressDt = addressSql.GetSqliteData("CONFIG_YYJR1");
                }

                #endregion

                rad_NetWeb.SelectedIndex = 0;

                sh = new SQLiteBusiness(string.Format(@"Data Source={0}SysInfo.db", AppDomain.CurrentDomain.BaseDirectory.ToString()));
                sh.CreateDB();

                int flg = sh.ValidateIsInt("BaseInfo");

                if (flg > 0)
                {
                    dbInfo = sh.GetSqliteData("BaseInfo");
                    if (dbInfo != null && dbInfo.Rows.Count > 0)
                    {
                        this.txt_SE_NAME.Text = dbInfo.Rows[dbInfo.Rows.Count - 1]["OWNER_SITE"].ToString();
                        this.txt_E_NAME.Text = dbInfo.Rows[dbInfo.Rows.Count - 1]["EMPLOYEE_CODE"].ToString();

                        if (dbInfo.Columns.Contains("NetWork"))
                        {
                            if (!string.IsNullOrWhiteSpace(dbInfo.Rows[dbInfo.Rows.Count - 1]["NetWork"].ToString()))
                            {
                                rad_NetWeb.SelectedIndex = Convert.ToInt32(dbInfo.Rows[dbInfo.Rows.Count - 1]["NetWork"]);
                            }
                        }

                        #region 加载缓存用户信息
                        cms.Name = "用户信息";
                        cms.Items.Add("【清除登录信息】");
                        cms.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { new System.Windows.Forms.ToolStripSeparator() });

                        for (int i = 1; i <= dbInfo.Rows.Count; i++)
                        {
                            cms.Items.Add(dbInfo.Rows[dbInfo.Rows.Count - i]["OWNER_SITE"].ToString() + "/" + dbInfo.Rows[dbInfo.Rows.Count - i]["EMPLOYEE_CODE"].ToString());
                            if (i == 9)
                            {
                                break;
                            }
                        }
                        this.btn_User.ContextMenuStrip = cms;
                        for (int i = 0; i < cms.Items.Count; i++)
                        {
                            cms.Items[i].Click += new EventHandler(cms_click);
                        }
                        #endregion
                    }
                }
            }
            catch (DllNotFoundException)
            {
                //确实系统文件，自动从文件夹复制出来
                //检查是否存在目的目录
                string dllFilepath = AppDomain.CurrentDomain.BaseDirectory.ToString() + "\\SystemDll";
                if (File.Exists(dllFilepath + "\\msvcp100.dll") && !File.Exists(AppDomain.CurrentDomain.BaseDirectory.ToString() + "\\msvcp100.dll"))
                {
                    //复制文件
                    File.Copy(dllFilepath + "\\msvcp100.dll", AppDomain.CurrentDomain.BaseDirectory.ToString() + "\\mfc100.dll", true);
                    File.Copy(dllFilepath + "\\msvcp100.dll", AppDomain.CurrentDomain.BaseDirectory.ToString() + "\\mfc100u.dll", true);
                    File.Copy(dllFilepath + "\\msvcp100.dll", AppDomain.CurrentDomain.BaseDirectory.ToString() + "\\msvcp100.dll", true);
                    File.Copy(dllFilepath + "\\msvcr100.dll", AppDomain.CurrentDomain.BaseDirectory.ToString() + "\\msvcr100.dll", true);
                    File.Copy(dllFilepath + "\\msvcr100_clr0400.dll", AppDomain.CurrentDomain.BaseDirectory.ToString() + "\\msvcr100_clr0400.dll", true);

                    XtraMessageBox.Show("电脑操作系统缺少文件,现已修复,请重新打开系统！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("登录报错！", ex);
                XtraMessageBox.Show(ex.Message, "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// 重写窗体Show
        /// </summary>
        /// <param name="e"></param>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            System.Threading.Thread.Sleep(100);
            if (Flag == 2)//调用登录不检查更新
            {
                this.txt_E_PWD.Text = password;
                Login();
            }
            if (!string.IsNullOrWhiteSpace(txt_E_NAME.Text))
            {
                this.txt_E_PWD.Focus();
            }
        }

        /// <summary>
        /// 显示缓存用户信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_User_Click(object sender, EventArgs e)
        {
            SimpleButton btn = (SimpleButton)sender;
            if (btn.ContextMenuStrip != null)
            {
                btn.ContextMenuStrip.Show(MousePosition);
            }
        }

        /// <summary>
        /// 按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void cms_click(object sender, EventArgs e)
        {
            try
            {
                ToolStripMenuItem item = (ToolStripMenuItem)sender;
                if (item.Text == "【清除登录信息】")
                {
                    cms.Items.Clear();
                    sh.DelTableData("BaseInfo", " and 1=1");
                }
                else
                {
                    string[] strs = item.Text.Split('/');
                    if (strs.Length > 1)
                    {
                        this.txt_SE_NAME.Text = strs[0];
                        this.txt_E_NAME.Text = strs[1];
                        this.txt_E_PWD.Focus();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 登录按钮单击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Login_Click(object sender, EventArgs e)
        {
            try
            {
                Login();
            }
            catch (Exception ex)
            {
                LogHelper.Error("登录报错！", ex);
            }
        }

        /// <summary>
        /// 获取93服务器的配置文件
        /// </summary>
        /// <returns></returns>
        private bool GetAddressFile()
        {
            try
            {
                //BussHelper.HttpDownLoadUrl("http://202.104.151.93:8091/DownLoad/" + dbName, AppDomain.CurrentDomain.BaseDirectory.ToString() + dbName);
                return true;
            }
            catch (Exception ex)
            {
                CloseWait();
                XtraMessageBox.Show("访问服务器失败", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LogHelper.Error("下载配置文件失败：" + ex.Message, ex);
                return false;
            }
        }

        /// <summary>
        /// 登录
        /// </summary>
        private void Login()
        {
            //登录日志
            Dictionary<string, string> di = new Dictionary<string, string>();

            ShowWait();
            SetWaitDescription("登录中...");
            try
            {
                GlobalVariable.GlobalSystem.OffLine = ck_OffLine.Checked;

                //返回内容
                Dictionary<string, string> dis = new Dictionary<string, string>();

                if (string.IsNullOrWhiteSpace(this.txt_SE_NAME.Text))
                {
                    CloseWait();
                    XtraMessageBox.Show("站点不能为空！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.txt_SE_NAME.SelectAll();
                    this.txt_SE_NAME.Focus();
                    return;
                }
                if (string.IsNullOrWhiteSpace(this.txt_E_NAME.Text))
                {
                    CloseWait();
                    XtraMessageBox.Show("账号不能为空！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.txt_E_NAME.SelectAll();
                    this.txt_E_NAME.Focus();
                    return;
                }
                if (string.IsNullOrWhiteSpace(this.txt_E_PWD.Text))
                {
                    CloseWait();
                    XtraMessageBox.Show("密码不能为空！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.txt_E_PWD.SelectAll();
                    this.txt_E_PWD.Focus();
                    return;
                }

                //用户名
                username = this.txt_E_NAME.Text.Replace("&", "");
                //密码
                password = this.txt_E_PWD.Text.ToString();
                //网点
                site = this.txt_SE_NAME.Text.ToString();

                #region 联机登录
                if (!ck_OffLine.Checked)
                {
                    if (txt_E_NAME.Text != "ztdadmin&")
                    {
                        isUpdate = true;
                    }

                    string Mac = GetMAC();//本机机器码

                    dis.Add("EMPLOYEE_CODE", username);
                    dis.Add("E_PWD", Utility.Encrypt(password));
                    dis.Add("OWNER_SITE", this.txt_SE_NAME.Text.Trim());
                    dis.Add("MCA", Mac);

                    SetWaitDescription("正在检测网络...");
                    #region 验证访问IP
                    //IP地址 正式
                    string ipAddress = string.Empty;
                    //IP地址 端口
                    string port = string.Empty;

                    foreach (DataRow row in AddressDt.Rows)
                    {
                        ipAddress = row["ADDRESS"].ToString();
                        port = row["PORT"].ToString();
                        if (Utility.SendPing(string.Format("{2}://{0}:{1}/YinYanServer/basedata", ipAddress, port, HttpHelper.GetHttpType())))
                        {
                            HttpHelper.ipAddress = ipAddress;
                            HttpHelper.port = port;
                            if (row["REMARK"].ToString() == "域名" || row["REMARK"].ToString() == "电信" || row["REMARK"].ToString() == "联通")
                            {
                                rad_NetWeb.SelectedIndex = 2;
                            }
                            else
                            {
                                rad_NetWeb.SelectedIndex = 3;
                            }
                            break;
                        }
                    }
                    if (string.IsNullOrWhiteSpace(HttpHelper.ipAddress))
                    {
                        if (GetAddressFile())
                        {
                            foreach (DataRow row in AddressDt.Rows)
                            {
                                ipAddress = row["ADDRESS"].ToString();
                                port = row["PORT"].ToString();
                                if (Utility.SendPing(string.Format("{2}://{0}:{1}/YinYanServer/basedata", ipAddress, port, HttpHelper.GetHttpType())))
                                {
                                    HttpHelper.ipAddress = ipAddress;
                                    HttpHelper.port = port;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            return;
                        }
                    }
                    #endregion
                    SetWaitDescription("验证用户信息...");


                    string strJson = JsonConvertHelper.DicToTableJson(dis, "TAB_EMPLOYEE_QRY");
                    DataSet dsResult = HttpHelper.TransData(ServerName.K9, "Get_Login", strJson);
                    if (!Utility.IsTranOK(dsResult))
                    {
                        CloseWait();
                        if (dsResult.Tables["ErrorList"].Rows[0]["ErrorMsg"] != null)
                        {
                            if (!string.IsNullOrWhiteSpace(dsResult.Tables["ErrorList"].Rows[0]["Code"].ToString()))
                            {
                                int num = Convert.ToInt32(dsResult.Tables["ErrorList"].Rows[0]["Code"]);
                                if (num == 4)
                                {
                                    #region 记录密码错误次数
                                    Dictionary<string, string> dic = new Dictionary<string, string>();
                                    dic.Add("EMPLOYEE_CODE", username);
                                    dic.Add("LOOCK", "0");
                                    dic.Add("ERROR", "0");
                                    string strJsonPwd = JsonConvertHelper.DicToTableJson(dic, "TAB_EMPLOYEE_UPT");
                                    DataSet ds = HttpHelper.TransData(ServerName.K9, "saveTAB_EMPLOYEE", strJsonPwd);
                                    #endregion

                                    XtraMessageBox.Show("用户登录密码错误超过五次，请两小时候后在登录，或联系系统管理员！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }
                            }
                            if (dsResult.Tables["ErrorList"].Rows[0]["ErrorMsg"].ToString() == "用户名或密码错误！")
                            {
                                #region 记录密码错误次数
                                Dictionary<string, string> dic = new Dictionary<string, string>();
                                dic.Add("EMPLOYEE_CODE", username);
                                dic.Add("ERROR", "0");
                                string strJsonPwd = JsonConvertHelper.DicToTableJson(dic, "TAB_EMPLOYEE_UPT");
                                DataSet ds = HttpHelper.TransData(ServerName.K9, "saveTAB_EMPLOYEE", strJsonPwd);

                                XtraMessageBox.Show("用户名或密码错误，连续输入错误五次，用户将被锁定两小时！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                                #endregion
                            }
                            XtraMessageBox.Show(dsResult.Tables["ErrorList"].Rows[0]["ErrorMsg"].ToString(), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        else
                        {
                            XtraMessageBox.Show(dsResult.Tables["ErrorList"].Rows[0]["ErrorCode"].ToString(), "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        this.txt_E_PWD.SelectAll();
                        this.txt_E_PWD.Focus();
                        return;
                    }
                    else
                    {
                        if (Utility.GetRowCount(dsResult.Tables["TAB_EMPLOYEE"]) == 0 || Utility.GetRowCount(dsResult.Tables["BaseInfo"]) == 0)
                        {
                            CloseWait();
                            XtraMessageBox.Show("此用戶不存在或密码错误，请检查！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            this.txt_SE_NAME.SelectAll();
                            this.txt_SE_NAME.Focus();
                            return;

                        }
                        else
                        {
                            #region 验证用户或网点是否停用
                            if (dsResult.Tables["TAB_EMPLOYEE"].Rows[0]["BL_NOT_INPUT"].ToString() == "1")
                            {
                                CloseWait();
                                XtraMessageBox.Show("此网点已停用，请联系管理员！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                this.txt_SE_NAME.SelectAll();
                                this.txt_SE_NAME.Focus();
                                return;
                            }
                            if (dsResult.Tables["TAB_EMPLOYEE"].Rows[0]["BL_OPEN"].ToString() == "0")
                            {
                                CloseWait();
                                XtraMessageBox.Show("此用戶未启用，请联系管理员！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                this.txt_SE_NAME.SelectAll();
                                this.txt_SE_NAME.Focus();
                                return;
                            }
                            #endregion

                            #region 组装数据转为登录用户信息实体类
                            DataTable dtBaseInfo = dsResult.Tables["BaseInfo"];//登录用户 信息
                            dtBaseInfo.Columns.Add("E_COMPUTID"); //电脑MAC地址
                            dtBaseInfo.Columns.Add("NetWork");    //选择网络类型
                            for (int i = 0; i < dtBaseInfo.Rows.Count; i++)
                            {
                                dtBaseInfo.Rows[i]["E_COMPUTID"] = Mac;
                                dtBaseInfo.Rows[i]["NetWork"] = rad_NetWeb.SelectedIndex.ToString();
                            }

                            //组装员工信息Json串
                            DataRow dr = dtBaseInfo.Rows[0];
                            StringBuilder sbUser = new StringBuilder();
                            sbUser.Append("{");
                            for (int i = 0; i < dtBaseInfo.Columns.Count; i++)
                            {
                                DataColumn dc = dtBaseInfo.Columns[i];
                                if ((dc.ColumnName == "POST_ID" || dc.ColumnName == "DEP_ID" || dc.ColumnName == "IS_QRYALL") && dr[dc].ToString() == "null")
                                {
                                    dr[dc] = "0";
                                }
                                sbUser.AppendFormat("\"{0}\":\"{1}\"", dc.ColumnName, dr[dc]);
                                if (i != dtBaseInfo.Columns.Count - 1)
                                {
                                    sbUser.Append(",");
                                }
                            }
                            sbUser.Append("}");
                            BaseInfoCommon.CurrentUser = JsonConvertHelper.MassDeserializeFromJson<BaseInfoModel>(sbUser.ToString());

                            #endregion

                            #region 验证用户完整性
                            if (BaseInfoCommon.CurrentUser == null)
                            {
                                CloseWait();
                                XtraMessageBox.Show("此用戶不存在或密码错误，请检查！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                                this.txt_SE_NAME.SelectAll();
                                this.txt_SE_NAME.Focus();
                                return;
                            }

                            if (string.IsNullOrWhiteSpace(BaseInfoCommon.CurrentUser.EMPLOYEE_NAME)
                                || string.IsNullOrWhiteSpace(BaseInfoCommon.CurrentUser.EMPLOYEE_CODE)
                                || string.IsNullOrWhiteSpace(BaseInfoCommon.CurrentUser.OWNER_SITE))
                            {
                                CloseWait();
                                XtraMessageBox.Show("该账号信息与网点不匹配，请联系管理员", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                                this.txt_SE_NAME.SelectAll();
                                this.txt_SE_NAME.Focus();
                                return;
                            }
                            #endregion

                            #region 保存BaseInfo到本地数据库
                            if (dtBaseInfo != null && dtBaseInfo.Rows.Count > 0)
                            {
                                try
                                {
                                    //初始化赋值给username,防止在线登录时采用老版本的方式登录使用错名称
                                    string code = BaseInfoCommon.CurrentUser.SITE_CODE.ToString();

                                    ///判断BaseInfo表是否存在
                                    if (sh.ValidateIsInt("BaseInfo") > 0)
                                    {
                                        sh.DelAndInsertData(dtBaseInfo, "BaseInfo", "EMPLOYEE_CODE", "OWNER_SITE");

                                    }
                                    else
                                    {
                                        sh.AddNewTable(dtBaseInfo, "BaseInfo");
                                    }

                                    Utility.dataPath = string.Format("{0}DB\\{1}\\", AppDomain.CurrentDomain.BaseDirectory.ToString(), dtBaseInfo.Rows[0]["SITE_CODE"].ToString() + username);
                                    if (!Directory.Exists(Utility.dataPath))
                                        Directory.CreateDirectory(Utility.dataPath);
                                    SqliteDB = new SQLiteBusiness(string.Format(@"Data Source={0}SysInfo.db", Utility.dataPath));
                                    SqliteDB.CreateDB();
                                    SqliteDB.AddNewTable(dtBaseInfo, dtBaseInfo.TableName);
                                }
                                catch (Exception ex)
                                {
                                    CloseWait();
                                    LogHelper.Error("登录时创建本地BaseInfo数据", ex);
                                }
                            }
                            #endregion

                            #region 判断密码是否需要强制更改
                            //如果密码全是数字则强制要求修改密码
                            if (Utility.IsAllNumber(password))
                            {
                                CloseWait();
                                XtraMessageBox.Show("系统检测到您的密码是纯数字，强制要求您修改密码后才能进入系统！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                //FrmUpdatePwd updatePwd = new FrmUpdatePwd();
                                //updatePwd.Owner = this;
                                //updatePwd.password = password;
                                //updatePwd.ShowDialog();
                                //if (!updatePwd.isSuccess)
                                //{
                                //    return;
                                //}
                                ShowWait();
                            }
                            else
                            {
                                StringBuilder StrSql = new StringBuilder();
                                StrSql.AppendFormat("SELECT A.E_PWD_UPDATE_DATE FROM TAB_EMPLOYEE A WHERE 1= 1 AND A.EMPLOYEE_CODE = '{0}' AND A.OWNER_SITE = '{1}'", this.txt_E_NAME.Text.Trim(), this.txt_SE_NAME.Text.Trim());
                                Dictionary<string, string> EmpDic = new Dictionary<string, string>();
                                EmpDic.Add("SQL", StrSql.ToString());
                                string StrJJson = JsonConvertHelper.DicToTableJson(EmpDic, "DATABASE_QRY");
                                DataSet dsr = HttpHelper.TransData(ServerName.K9, "qryDATABASE_INTERFACE", StrJJson);
                                if (Utility.IsTranOK(dsr))
                                {
                                    if (Utility.GetRowCount(dsr) > 0)
                                    {
                                        string Date = dsr.Tables[0].Rows[0]["E_PWD_UPDATE_DATE"].ToString();
                                        if (!string.IsNullOrWhiteSpace(Date))
                                        {
                                            //if (Utility.GetSystemDate() > DateTime.Parse(Date).AddDays(60))
                                            //{
                                            //    CloseWait();
                                            //    XtraMessageBox.Show("系统检测到您的用户密码60天未修改，强制要求您修改密码后才能进入系统！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                            //    FrmUpdatePwd updatePwd = new FrmUpdatePwd();
                                            //    updatePwd.Owner = this;
                                            //    updatePwd.password = password;
                                            //    updatePwd.ShowDialog();
                                            //    if (!updatePwd.isSuccess)
                                            //    {
                                            //        return;
                                            //    }
                                            //    ShowWait();
                                            //}
                                        }
                                    }
                                }
                            }
                            #endregion

                            SetWaitDescription("获取下载本地数据...");
                            #region 判断本地数据是否有修改

                            //有数据更新标识
                            bool hasNewData = false;

                            //查询本地下载数据
                            List<string> tableNameList = new List<string>();
                            DataTable dtDownLoadSys = SqliteDB.GetSqliteData("T_BASE_GLOBAL_CDS");

                            //查询服务器下载数据
                            Dictionary<string, string> dic = new Dictionary<string, string>();
                            string json = JsonConvertHelper.DicToTableJson(dic, "T_BASE_GLOBAL_CDS_QRY");
                            DataSet dsDownLoad = HttpHelper.TransData(ServerName.K9, "qryT_BASE_GLOBAL_CDS", json);

                            if (Utility.IsTranOK(dsDownLoad))
                            {
                                foreach (DataRow row in dsDownLoad.Tables["T_BASE_GLOBAL_CDS"].Rows)
                                {
                                    if (row["CDS_NAME"] != null)
                                    {
                                        if (row["CHANGE_FLAG"].ToString() != "0")
                                        {
                                            if (dtDownLoadSys != null && dtDownLoadSys.Rows.Count > 0)
                                            {
                                                //根据获取本地改变标识
                                                string localFLAG = Utility.GetLookUpIdOrCode(dtDownLoadSys, "CHANGE_FLAG", "CDS_NAME", row["CDS_NAME"].ToString());
                                                if (string.IsNullOrWhiteSpace(localFLAG) || localFLAG != row["CHANGE_FLAG"].ToString())
                                                {
                                                    if (row["CDS_NAME"].ToString() == "T_BASE_SYSPARAM")
                                                    {
                                                        if (!GetSysparam())
                                                        {
                                                            CloseWait();
                                                            XtraMessageBox.Show("获取系统参数失败！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                            return;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        tableNameList.Add(row["CDS_NAME"].ToString());
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (row["CDS_NAME"].ToString() == "T_BASE_SYSPARAM")
                                                {
                                                    if (!GetSysparam())
                                                    {
                                                        CloseWait();
                                                        XtraMessageBox.Show("获取系统参数失败！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                                        return;
                                                    }
                                                }
                                                else
                                                {
                                                    tableNameList.Add(row["CDS_NAME"].ToString());
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion

                            #region 登录成功之后将登录错误次数置为0
                            Dictionary<string, string> doc1 = new Dictionary<string, string>();
                            doc1.Add("EMPLOYEE_CODE", username);
                            doc1.Add("LOOCK_DATE", "1990-01-01");
                            doc1.Add("LOGIN_ERROR", "0");
                            string strJsonPwd = JsonConvertHelper.DicToTableJson(doc1, "TAB_EMPLOYEE_UPT");
                            DataSet ds = HttpHelper.TransData(ServerName.K9, "saveTAB_EMPLOYEE", strJsonPwd);
                            #endregion

                            SetWaitDescription("检查更新...");

                            //可更新列表不为空
                            if (tableNameList.Count > 0)
                            {
                                hasNewData = true;
                            }

                            //将服务器时间同步到本机电脑时间
                            WinAPI.SetSysTime(Convert.ToDateTime(BaseInfoCommon.CurrentUser.DTSERVERDATE));

                            Utility.LoginID = Utility.GetOrderNo();
                            Utility.LoginPWD = Utility.Encrypt(password);

                            //添加登录日志
                            di.Add("GUID", Utility.LoginID);
                            di.Add("USER_NAME", username);
                            di.Add("OWNER_SITE", site);//登录网点
                            di.Add("LOGIN_TIME", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            di.Add("USER_TYPE", BaseInfoCommon.CurrentUser.TYPE);
                            di.Add("OWNER_SITE_CODE", BaseInfoCommon.CurrentUser.SITE_CODE);
                            di.Add("LOGIN_STATE", "登录成功");
                            loginlog(di);

                            this.txt_E_PWD.Focus();
                            CloseWait();
                            if (isFlag)
                            {
                                FrmMain main = new FrmMain();
                                main.Show();
                                main.Activate();
                                this.Hide();
                            }
                            else
                            {
                                if (hasNewData)
                                {
                                    //FrmLoadData fld = new FrmLoadData(dtLocalParams, SqliteDB, tableNameList);
                                    //fld.Username = username;
                                    //fld.Pwd = password;
                                    //fld.Show();
                                    //this.Hide();
                                }
                                else
                                {
                                    FrmMain main = new FrmMain();
                                    main.Show();
                                    main.Activate();
                                    this.Hide();
                                }
                            }
                        }
                    }
                }
                #endregion
                #region 脱机登录
                else
                {
                    if (Utility.GetRowCount(dbInfo) > 0)
                    {
                        DataRow[] userRow = dbInfo.Select(string.Format("(OWNER_SITE='{0}' or SITE_CODE='{0}') and (EMPLOYEE_CODE='{1}' or DRIVING_LICENCE='{1}')", site.Trim(), username.Trim()));
                        if (userRow != null && userRow.Length > 0)
                        {
                            if (userRow[0]["E_PWD"].ToString() == Utility.Encrypt(password.Trim()))
                            {
                                //组装员工信息Json串
                                DataRow dr = userRow[0];
                                StringBuilder sbUser = new StringBuilder();
                                sbUser.Append("{");
                                for (int i = 0; i < dbInfo.Columns.Count; i++)
                                {
                                    DataColumn dc = dbInfo.Columns[i];
                                    if ((dc.ColumnName == "POST_ID"
                                            || dc.ColumnName == "DEP_ID"
                                            || dc.ColumnName == "IS_QRYALL")
                                        && dr[dc].ToString() == "null")
                                    {
                                        dr[dc] = "0";
                                    }
                                    sbUser.AppendFormat("\"{0}\":\"{1}\"", dc.ColumnName, dr[dc]);
                                    if (i != dbInfo.Columns.Count - 1)
                                    {
                                        sbUser.Append(",");
                                    }
                                }
                                sbUser.Append("}");
                                BaseInfoCommon.CurrentUser = JsonConvertHelper.MassDeserializeFromJson<BaseInfoModel>(sbUser.ToString());
                                Utility.dataPath = string.Format("{0}DB\\{1}\\", AppDomain.CurrentDomain.BaseDirectory.ToString(), userRow[0]["SITE_CODE"].ToString() + username);

                                GlobalVariable.GlobalSystem.OffLine = true;
                                this.txt_E_PWD.Focus();

                                FrmMain main = new FrmMain();
                                main.Show();
                                main.Activate();
                                this.Hide();
                            }
                            else
                            {
                                XtraMessageBox.Show("用户名或密码错误！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }
                        }
                        else
                        {
                            XtraMessageBox.Show("用户未联机登录过,请先将用户联机登录一次！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }
                    else
                    {
                        XtraMessageBox.Show("未联机登录过用户！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                CloseWait();
                XtraMessageBox.Show(ex.Message, "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogHelper.Error(" PC端登陆错误日志记录:", ex);
                return;
            }
        }

        /// <summary>
        /// 保存登录日志
        /// </summary>
        /// <param name="di"></param>
        public void loginlog(Dictionary<string, string> di)
        {
            NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();//获取本地计算机上网络接口的对象
            foreach (NetworkInterface adapter in adapters)
            {
                string mac = adapter.GetPhysicalAddress().ToString();
                if (adapter.SupportsMulticast && !string.IsNullOrWhiteSpace(mac))
                {
                    di.Add("REAL_COMPUTER_ID", mac); //实际用户机器码
                    break;
                }
            }
            try
            {
                string jsons = JsonConvertHelper.DicToTableJson(di, "T_BASE_LOGIN_LOG_ADD");
                DataSet ds = HttpHelper.TransData(ServerName.K9, "saveT_BASE_LOGIN_LOG", jsons);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 重置按钮单击事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Clear_Click(object sender, EventArgs e)
        {
            OldClearLoginInfo();
        }

        /// <summary>
        /// 清除窗体上的旧版登录信息
        /// </summary>
        public void ClearPwd()
        {
            this.txt_E_PWD.Text = "";
            this.txt_SE_NAME.SelectAll();
            this.txt_SE_NAME.Focus();
        }

        /// <summary>
        /// 清除窗体上的旧版登录信息
        /// </summary>
        public void OldClearLoginInfo()
        {
            this.txt_SE_NAME.Text = "";
            this.txt_E_NAME.Text = "";
            this.txt_E_PWD.Text = "";
            this.txt_SE_NAME.SelectAll();
            this.txt_SE_NAME.Focus();
        }

        /// <summary>
        /// 密码键盘事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txt_E_PWD_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Btn_Login_Click(sender, e);
            }
        }

        /// <summary>
        /// 窗体激活
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmLogin_Activated(object sender, EventArgs e)
        {
            UserLookAndFeel.Default.SetDefaultStyle();
        }

        /// <summary>
        /// 获取本机MCA码
        /// </summary>
        /// <returns></returns>
        private static string GetMAC()
        {
            HardwareInfo hdw = new HardwareInfo();
            return hdw.GetMacAddress();
        }

        /// <summary>
        /// 检测任务栏是否打开多个K9系统
        /// </summary>
        /// <returns></returns>
        private int ValidateWin()
        {
            int i = 0;
            try
            {
                Process[] MyProcesses = Process.GetProcesses();
                foreach (Process MyProcess in MyProcesses)
                {
                    if (MyProcess.MainWindowTitle.Length > 0)
                    {
                        if (MyProcess.MainModule.FileName == Application.ExecutablePath)
                        {
                            i++;
                        }
                    }
                }
            }
            catch
            {
            }
            if (i > 1)
            {
                CloseWait();
                XtraMessageBox.Show("系统即将更新，请关闭本机登陆的所有K9系统，重新登陆", "系统提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                return 0;
            }
            return 1;
        }

        #region 暂时存放更新代码
        /// <summary>
        /// 获取系统参数表
        /// </summary>
        /// <returns></returns>
        private bool GetSysparam()
        {
            #region 初始化公共DB数据
            try
            {
                Dictionary<string, string> dis_SysParam = new Dictionary<string, string>();
                dis_SysParam.Add("SM_FLAG", "0");
                string strJson_SysParam = JsonConvertHelper.DicToTableJson(dis_SysParam, "T_BASE_SYSPARAM_QRY");
                DataSet ds = HttpHelper.TransData(ServerName.K9, "qryT_BASE_SYSPARAM", strJson_SysParam);

                if (Utility.IsTranOK(ds))
                {
                    dtLocalParams = ds.Tables["T_BASE_SYSPARAM"];//必须参数
                    sh.AddNewTable(dtLocalParams, "T_BASE_SYSPARAM");
                }
                else
                {
                    if (ds.Tables.Count > 0 && ds.Tables.Contains("ErrorList"))
                    {
                        if (ds.Tables["ErrorList"].Rows[0]["ErrorMsg"] != null)
                        {
                            LogHelper.Error(" PC端登陆-获取系统参数为空" + ds.Tables["ErrorList"].Rows[0]["ErrorMsg"].ToString());
                            return false;
                        }
                    }
                }
                if (dtLocalParams == null || dtLocalParams.Rows.Count < 1)
                {
                    LogHelper.Error(" PC端登陆-获取系统参数为空");
                    return false;
                }
            #endregion

                //更新相关用户的系统信息参数
                SqliteDB.AddNewTable(dtLocalParams, "T_BASE_SYSPARAM");
            }
            catch (Exception ex)
            {
                XtraMessageBox.Show("获取系统参数失败！" + ex.Message, "系统提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            }


            return true;
        }

        /// <summary>
        /// 检查更新
        /// </summary>
        /// <returns></returns>
        int CheckForUpdate()
        {
            //更新标识
            int isUpdate = 0;
            try
            {
                //ftp用户名
                string ftpUser = GetSysParam("FtpUser");
                BussHelper.FileUser = ftpUser;
                //ftp密码
                string ftpPassword = GetSysParam("FtpPassword");
                BussHelper.FilePassword = ftpPassword;
                if (string.IsNullOrWhiteSpace(ftpUser) || string.IsNullOrWhiteSpace(ftpPassword))
                {
                    CloseWait();
                    if (XtraMessageBox.Show("获取升级用户密码信息失败，请重新下载更新！", "系统提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                    {
                        CheckForUpdate();
                        isUpdate = 9;
                        return isUpdate;
                    }
                    else
                    {
                        isUpdate = 9;
                        return isUpdate;
                    }

                }
                DataTable dtFiles = null;

                //根据员工编码获取可用更新
                string id = BaseInfoCommon.CurrentUser.EMPLOYEE_CODE.ToString();
                Dictionary<string, string> dis = new Dictionary<string, string>();
                dis.Add("U_FLAG", "1");
                dis.Add("UPDATE_DESC", "电信");

                //if (rad_NetWeb.Text == "电信" || rad_NetWeb.Text == "联通")
                //{
                //    dis.Add("UPDATE_DESC", "电信");
                //}
                //else if (rad_NetWeb.Text == "智能")
                //{
                //    dis.Add("UPDATE_DESC", "联通");
                //}
                //else
                //{
                //    dis.Add("UPDATE_DESC", rad_NetWeb.Text);
                //}

                string strJson = JsonConvertHelper.DicToTableJson(dis, "T_BASE_AUTO_UPDATE_QRY");

                SetWaitDescription("获取更新文件");
                DataSet dsResult = HttpHelper.TransData(ServerName.K9, "qryT_BASE_AUTO_UPDATE", strJson);

                if (dsResult != null && dsResult.Tables["ErrorList"].Rows[0]["ErrorCode"].ToString() == "200")
                {
                    if (dsResult.Tables.Count > 1)
                    {
                        if (dsResult.Tables["T_BASE_AUTO_UPDATE"].Rows.Count > 0)
                        {
                            dtFiles = dsResult.Tables["T_BASE_AUTO_UPDATE"];
                        }
                    }
                }

                //获取需要下载更新的文件列表
                DataTable fileList = null;

                if (dtFiles != null && dtFiles.Rows.Count > 0)
                {
                    fileList = GetDownLoadFiles(dtFiles);
                }

                DataTable dtUpdateRecord = dtFiles.Clone();

                if (Utility.GetRowCount(fileList) > 0 || Utility.GetRowCount(dtUpdateRecord) > 0)
                {
                    isUpdate = 3;

                    DataRow drGetUpdate = null;

                    if (Utility.GetRowCount(fileList) > 0)
                    {
                        foreach (DataRow dr in fileList.Rows)
                        {
                            if (dr["FILE_PATH"].ToString().Trim().Equals("Update.exe"))
                            {
                                drGetUpdate = dr;
                            }
                            else
                            {
                                dtUpdateRecord.ImportRow(dr);
                            }
                        }
                    }
                    if (drGetUpdate != null)
                    {
                        string downloadPath = Path.Combine(Application.StartupPath + "\\Update");
                        string backUpPath = Path.Combine(Application.StartupPath + "\\Backup");

                        if (!Directory.Exists(downloadPath))
                        {

                            Directory.CreateDirectory(downloadPath);
                        }
                        else
                        {
                            Directory.Delete(downloadPath, true);
                            Directory.CreateDirectory(downloadPath);
                        }

                        if (!Directory.Exists(backUpPath))
                            Directory.CreateDirectory(backUpPath);

                        //下载成功标识
                        bool flg = true;
                        string url = string.Empty;

                        url = drGetUpdate["DOWN_ADDRESS"].ToString();

                        string fileName = "UPDATE.zip";

                        /// <summary>
                        /// 下载DLL
                        /// </summary>
                        FileStream outputUPdate = new FileStream(Path.Combine(downloadPath, fileName), FileMode.Create);

                        try
                        {
                            Thread.Sleep(100);
                            HttpHelper http = new HttpHelper();
                            flg = http.TransData(url, outputUPdate, ftpUser, ftpPassword);
                            outputUPdate.Close();
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Error("更新的下载UPDATE.EXE方法(FileDown)报错", ex);
                            flg = false;
                            outputUPdate.Close();
                        }

                        if (flg == false)
                        {
                            CloseWait();
                            if (XtraMessageBox.Show("升级更新程序失败，请重新下载更新！", "系统提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                            {
                                CheckForUpdate();
                                isUpdate = 9;
                                return isUpdate;
                            }
                            else
                            {
                                isUpdate = 9;
                                return isUpdate;
                            }
                        }
                        else
                        {
                            try
                            {
                                bool blZip = UnMakeZipFile(downloadPath + "\\" + fileName, downloadPath);
                                if (blZip)//解压成功
                                {
                                    if (File.Exists(downloadPath + "\\" + fileName))
                                    {
                                        //如果存在则删除
                                        File.Delete(downloadPath + "\\" + fileName);
                                    }

                                    DirectoryInfo dir = new DirectoryInfo(downloadPath);
                                    if (dir.Exists)
                                    {
                                        foreach (FileInfo fileitem in dir.GetFiles())
                                        {
                                            string item = fileitem.Name;

                                            string excutePath = Path.Combine(Application.StartupPath, item);
                                            if (File.Exists(excutePath))
                                            {
                                                File.Replace(Path.Combine(downloadPath, item), excutePath, Path.Combine(backUpPath, item));
                                            }
                                            else
                                            {
                                                File.Move(Path.Combine(downloadPath, item), excutePath);
                                            }

                                            File.Delete(fileitem.FullName);
                                        }
                                    }
                                }
                                else
                                {
                                    CloseWait();
                                    XtraMessageBox.Show("压缩UPDATE.EXE程序失败", "系统提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                                    isUpdate = 9;
                                    return isUpdate;
                                }
                            }
                            catch
                            {
                                CloseWait();
                                if (XtraMessageBox.Show("升级更新程序失败，请重新下载更新！", "系统提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                                {
                                    CheckForUpdate();
                                    isUpdate = 9;
                                    return isUpdate;
                                }
                                else
                                {
                                    isUpdate = 9;
                                    return isUpdate;
                                }
                            }
                        }
                    }

                    if (ValidateWin() == 0)
                    {
                        return 11;
                    }

                    //未找到文件
                    if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory.ToString() + "\\UPDATE.exe"))
                    {
                        CloseWait();
                        XtraMessageBox.Show("未能找到升级程序，请联系管理员！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return 11;
                    }

                    //组装更新文件信息列表保存到本地库
                    if (dtUpdateRecord != null && dtUpdateRecord.Rows.Count > 0)
                    {

                        sh.AddNewTable(dtUpdateRecord, "UPDATETABLE");

                        string dtJson = JsonConvertHelper.ConvertDataTableToJson(dtUpdateRecord);

                        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(dtJson);

                        string json = Convert.ToBase64String(bytes);


                        //强制更新时不提示直接下载
                        string param = string.Format("{0} {1} {2} {3} {4} {5}", json, ftpUser, ftpPassword, username, password, site);
                        System.Diagnostics.Process process = new Process();
                        process.StartInfo.FileName = AppDomain.CurrentDomain.BaseDirectory.ToString() + "\\UPDATE.exe";
                        process.StartInfo.Arguments = param;
                        process.StartInfo.UseShellExecute = false;
                        process.Start();

                        Environment.Exit(0);
                    }
                }
            }
            catch (Exception ex)
            {
                CloseWait();
                LogHelper.Error(" PC端检查更新错误日志记录:", ex);

                if (XtraMessageBox.Show("获取升级更新失败,错误信息为：" + ex.Message + "，请重新下载更新！", "系统提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                {
                    CheckForUpdate();
                    isUpdate = 9;
                    return isUpdate;
                }
                else
                {
                    isUpdate = 11;
                    return isUpdate;
                }
            }
            return isUpdate;
        }

        /// <summary>
        /// 根据MD5判断哪些文件需要更新
        /// </summary>
        /// <param name="dtFileList"></param>
        /// <returns></returns>
        DataTable GetDownLoadFiles(DataTable dtFileList)
        {
            DataTable dtFiles = dtFileList.Clone();
            //string Dllstr = string.Empty;
            //对比文件版本是否相同
            foreach (DataRow dr in dtFileList.Rows)
            {
                string file = dr["FILE_PATH"].ToString();

                if (!string.IsNullOrWhiteSpace(file))
                {
                    if (File.Exists(AppDomain.CurrentDomain.BaseDirectory.ToString() + "\\" + file))
                    {
                        //MD5验证
                        StringComparer sc = StringComparer.OrdinalIgnoreCase;
                        string filemd5 = ZTDMD5.MDFile(AppDomain.CurrentDomain.BaseDirectory.ToString() + "\\" + file);
                        string checkcode = dr["CHECK_ID"].ToString().Trim();
                        //MessageBox.Show("本地DLL的Md5码是 " + filemd5 + " | " + "数据库的MD5码是 " + checkcode);
                        if (0 != sc.Compare(filemd5, checkcode))
                        {
                            dtFiles.ImportRow(dr);
                        }
                    }
                    else
                    {
                        //文件不存在，需要下载更新
                        dtFiles.ImportRow(dr);
                    }
                }
            }
            return dtFiles;
        }

        /// <summary>
        /// 获取系统参数
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        string GetSysParam(string key)
        {
            if (dtLocalParams == null || dtLocalParams.Rows.Count == 0)
            {
                dtLocalParams = sh.GetSqliteData("T_BASE_SYSPARAM");
            }
            string value = string.Empty;
            var result = dtLocalParams.Select(string.Format("SM_KEY='{0}'", key));
            if (result.Length > 0)
            {
                if (key == "FtpServer")
                {
                    long times = 0;
                    foreach (DataRow row in result)
                    {
                        Ping p = new Ping();
                        PingReply pr;
                        pr = p.Send(row["SM_VALUE"].ToString().Split(':')[0]);
                        if (pr.Status == IPStatus.Success && times == 0)
                        {
                            times = pr.RoundtripTime;
                            value = row["SM_VALUE"].ToString();
                            continue;
                        }
                        if (pr.Status == IPStatus.Success && pr.RoundtripTime < times)
                        {
                            times = pr.RoundtripTime;
                            value = row["SM_VALUE"].ToString();
                        }
                    }
                }
                else
                {
                    value = result[0]["SM_VALUE"].ToString();
                }
            }
            return value;
        }

        #endregion

        #region 解压缩文件
        public bool UnMakeZipFile(string zipfilename, string UnZipDir)
        {
            bool isFlag = false;
            //判断待解压文件路径
            if (!File.Exists(zipfilename))
            {
                return false;
            }
            //创建ZipInputStream
            ZipInputStream newinStream = new ZipInputStream(File.OpenRead(zipfilename));
            //执行解压操作
            try
            {
                ZipEntry theEntry;
                string dirpatch = string.Empty;
                //获取Zip中单个File
                while ((theEntry = newinStream.GetNextEntry()) != null)
                {

                    //获得子集文件名
                    string filename = Path.GetFileName(theEntry.Name);
                    //解压指定子目录
                    if (filename != string.Empty)
                    {
                        FileStream newstream = File.Create(UnZipDir + "\\" + dirpatch + filename);

                        int size = 2048;
                        byte[] newbyte = new byte[size];
                        while (true)
                        {
                            size = newinStream.Read(newbyte, 0, newbyte.Length);
                            if (size > 0)
                            {
                                //写入数据
                                newstream.Write(newbyte, 0, size);
                            }
                            else
                            {
                                break;
                            }
                        }
                        newstream.Close();
                    }
                    else
                    {
                        dirpatch = theEntry.Name;
                        if (!string.IsNullOrWhiteSpace(dirpatch))
                        {
                            if (!Directory.Exists(UnZipDir + "\\" + dirpatch))//判断文件夹是否已经存在
                            {
                                Directory.CreateDirectory(UnZipDir + "\\" + dirpatch);//创建文件夹
                            }

                        }
                    }
                }
                newinStream.Close();
                isFlag = true;
            }
            catch (Exception se)
            {
                LogHelper.Error(se);
            }
            finally
            {
                newinStream.Close();
            }
            return isFlag;
        }

        #endregion

        #region 汉化入口
        /// <summary>
        /// MDI窗体标题本地化类
        /// </summary>
        public class ChineseDocumentManagerLocalizer : DocumentManagerLocalizer
        {
            public override string Language { get { return "简体中文"; } }
            public override string GetLocalizedString(DocumentManagerStringId id)
            {
                switch (id)
                {
                    case DocumentManagerStringId.CommandFloat: return "漂动";
                    case DocumentManagerStringId.CommandClose: return "关闭";
                    case DocumentManagerStringId.CommandCloseAll: return "关闭所有";
                    case DocumentManagerStringId.CommandCloseAllButThis: return "除此之外全关闭";
                    case DocumentManagerStringId.CommandNewVerticalDocumentGroup: return "新建垂直选项卡组";
                    case DocumentManagerStringId.CommandNewHorizontalDocumentGroup: return "新建水平选项卡组";
                    case DocumentManagerStringId.CommandMoveToNextDocumentGroup: return "移至下一组选项卡";
                    case DocumentManagerStringId.CommandMoveToPrevDocumentGroup: return "移至上一组选项卡";
                    case DocumentManagerStringId.CommandOpenedWindowsDialog: return "窗体";
                    case DocumentManagerStringId.OpenedWindowsDialogNameColumnCaption: return "名称";
                    case DocumentManagerStringId.OpenedWindowsDialogPathColumnCaption: return "路径";
                    case DocumentManagerStringId.CommandActivate: return "激活";
                    default: return base.GetLocalizedString(id);
                }
            }
        }

        /// <summary>
        /// XtreBars 控件汉化
        /// </summary>
        public class ChineseXtreBarsLocalizer : BarLocalizer
        {
            public override string Language { get { return "简体中文"; } }
            public override string GetLocalizedString(DevExpress.XtraBars.Localization.BarString id)
            {
                switch (id)
                {
                    case BarString.RibbonToolbarMinimizeRibbon: return "最小化";
                    default: return base.GetLocalizedString(id);
                }
            }
        }

        /// <summary>
        /// XtraEditors 控件汉化
        /// </summary>
        public class ChineseXtraEditorsLocalizer : Localizer
        {
            public override string Language { get { return "简体中文"; } }
            public override string GetLocalizedString(StringId id)
            {
                switch (id)
                {
                    case StringId.TextEditMenuCopy: return "复制(&C)";
                    case StringId.TextEditMenuCut: return "剪切(&X)";
                    case StringId.TextEditMenuDelete: return "删除(&D)";
                    case StringId.TextEditMenuPaste: return "粘贴(&V)";
                    case StringId.TextEditMenuSelectAll: return "全选(&A)";
                    case StringId.TextEditMenuUndo: return "撤消(&Z)";
                    case StringId.UnknownPictureFormat: return "未知图片格式";
                    case StringId.DateEditToday: return "今天";
                    case StringId.DateEditClear: return "清空";
                    case StringId.DataEmpty: return "无图像";
                    case StringId.ColorTabWeb: return "网页";
                    case StringId.ColorTabSystem: return "系统";
                    case StringId.ColorTabCustom: return "自定义";
                    case StringId.CheckUnchecked: return "未选择";
                    case StringId.CheckIndeterminate: return "不确定";
                    case StringId.CheckChecked: return "已选择";
                    case StringId.CaptionError: return "标题错误";
                    case StringId.Cancel: return "取消";
                    case StringId.CalcError: return "计算错误";
                    case StringId.CalcButtonBack: return base.GetLocalizedString(id);
                    case StringId.CalcButtonC: return base.GetLocalizedString(id);
                    case StringId.CalcButtonCE: return base.GetLocalizedString(id); ;
                    case StringId.CalcButtonMC: return base.GetLocalizedString(id);
                    case StringId.CalcButtonMR: return base.GetLocalizedString(id);
                    case StringId.CalcButtonMS: return base.GetLocalizedString(id);
                    case StringId.CalcButtonMx: return base.GetLocalizedString(id);
                    case StringId.CalcButtonSqrt: return base.GetLocalizedString(id);
                    case StringId.OK: return "确定";
                    case StringId.PictureEditMenuCopy: return "复制(&C)";
                    case StringId.PictureEditMenuCut: return "剪切(&X)";
                    case StringId.PictureEditMenuDelete: return "删除(&D)";
                    case StringId.PictureEditMenuLoad: return "加载(&L)";
                    case StringId.InvalidValueText: return "无效的值";
                    case StringId.NavigatorRemoveButtonHint: return "删除";
                    case StringId.NavigatorTextStringFormat: return "记录{0}/{1}";
                    case StringId.None: return "";
                    case StringId.NotValidArrayLength: return "无效的数组长度.";
                    case StringId.XtraMessageBoxCancelButtonText: return "取消";
                    case StringId.XtraMessageBoxOkButtonText: return "确定";
                    case StringId.XtraMessageBoxYesButtonText: return "是";
                    case StringId.XtraMessageBoxNoButtonText: return "否";
                    case StringId.XtraMessageBoxIgnoreButtonText: return "忽略";
                    case StringId.XtraMessageBoxAbortButtonText: return "中止";
                    case StringId.XtraMessageBoxRetryButtonText: return "重试";
                    case StringId.FilterClauseIsNotNullOrEmpty: return "为空";
                    case StringId.FilterClauseIsNullOrEmpty: return "不为空";
                    case StringId.FilterClauseBeginsWith: return "头部包含";
                    case StringId.FilterClauseBetween: return "范围内";
                    case StringId.FilterClauseBetweenAnd: return "至";
                    case StringId.FilterClauseContains: return "包含";
                    case StringId.FilterClauseEndsWith: return "尾部包含";
                    case StringId.FilterClauseEquals: return "等于";
                    case StringId.FilterClauseGreater: return "大于";
                    case StringId.FilterClauseGreaterOrEqual: return "大于等于";
                    case StringId.FilterClauseLess: return "小于";
                    case StringId.FilterClauseLessOrEqual: return "小于等于";
                    case StringId.FilterClauseLike: return "Like包含";
                    case StringId.FilterClauseNotBetween: return "不在范围内";
                    case StringId.FilterClauseDoesNotContain: return "不包含";
                    case StringId.FilterClauseDoesNotEqual: return "不等于";
                    case StringId.FilterClauseNotLike: return "Not Like不包含";
                    case StringId.FilterEmptyEnter: return "输入值";
                    default: return base.GetLocalizedString(id);
                }
            }
        }

        public class XtraGridLocalizer_zh_chs : GridLocalizer
        {
            public override string Language
            {
                get
                {
                    return "简体中文";
                }
            }
            public override string GetLocalizedString(DevExpress.XtraGrid.Localization.GridStringId id)
            {
                switch (id)
                {
                    case GridStringId.CardViewCaptionFormat: return "记录N{0}";
                    case GridStringId.CardViewNewCard: return "新建卡";
                    case GridStringId.CardViewQuickCustomizationButton: return "自定义";
                    case GridStringId.CardViewQuickCustomizationButtonFilter: return "过滤器　";
                    case GridStringId.CardViewQuickCustomizationButtonSort: return "排序方式:";
                    case GridStringId.ColumnViewExceptionMessage: return "要修正当前值吗?";
                    case GridStringId.CustomFilterDialog2FieldCheck: return "字段";
                    case GridStringId.CustomFilterDialogCancelButton: return "取消(&C)";
                    case GridStringId.CustomFilterDialogCaption: return "显示符合下列条件的行:";
                    case GridStringId.CustomFilterDialogClearFilter: return "清除过滤器(&L)";
                    case GridStringId.CustomFilterDialogEmptyOperator: return "(选择一个操作)";
                    case GridStringId.CustomFilterDialogEmptyValue: return "(输入一个值)";
                    case GridStringId.CustomFilterDialogFormCaption: return "用户自定义自动过滤器";
                    case GridStringId.CustomFilterDialogHint: return "用_替代一个单字符#用%替代其他任何类型的字符";
                    case GridStringId.CustomFilterDialogOkButton: return "确定(&O)";
                    case GridStringId.CustomFilterDialogRadioAnd: return "并且(&A)";
                    case GridStringId.CustomFilterDialogRadioOr: return "或者(&O)";
                    case GridStringId.CustomizationBands: return "带宽";
                    case GridStringId.CustomizationCaption: return "自定义";
                    case GridStringId.CustomizationColumns: return "列";
                    case GridStringId.CustomizationFormBandHint: return "在此拖拉条来定制布局";
                    case GridStringId.CustomizationFormColumnHint: return "在此拖拉列来定制布局";
                    case GridStringId.FileIsNotFoundError: return "文件{0}找不到";
                    case GridStringId.FilterBuilderApplyButton: return "应用(&A)";
                    case GridStringId.FilterBuilderCancelButton: return "取消(&C)";
                    case GridStringId.FilterBuilderCaption: return "数据筛选条件设定：";
                    case GridStringId.FilterBuilderOkButton: return "确定(&O)";
                    case GridStringId.FilterPanelCustomizeButton: return "自定义";
                    case GridStringId.GridGroupPanelText: return "拖动列标题至此,根据该列分组";
                    case GridStringId.GridNewRowText: return "在此处添加一行";
                    case GridStringId.GridOutlookIntervals: return "更早;上个月;本月初;三周之前;两周之前;上周;;;;;;;;昨天;今天;明天;;;;;;;;下周;两周后;三周后;本月底;下个月;一个月之后;";
                    case GridStringId.LayoutModifiedWarning: return "布局已被更改，确定要保存更改吗？";
                    case GridStringId.LayoutViewButtonApply: return "应用(&A)";
                    case GridStringId.LayoutViewButtonCancel: return "取消(&C)";
                    case GridStringId.LayoutViewButtonCustomizeHide: return "隐藏自定义(&z)";
                    case GridStringId.LayoutViewButtonCustomizeShow: return "显示自定义(&S)";
                    case GridStringId.LayoutViewButtonLoadLayout: return "加载面板(&L)...";
                    case GridStringId.LayoutViewButtonOk: return "确定(&O)";
                    case GridStringId.LayoutViewButtonPreview: return "显示更多卡(&M)";
                    case GridStringId.LayoutViewButtonReset: return "重置卡模板(&R)";
                    case GridStringId.LayoutViewButtonSaveLayout: return "保存版面...(&v)";
                    case GridStringId.LayoutViewButtonShrinkToMinimum: return "收缩卡模板(&S)";
                    case GridStringId.LayoutViewCardCaptionFormat: return "记录[{0}/{1}]";
                    case GridStringId.LayoutViewCarouselModeBtnHint: return "旋转模式";
                    case GridStringId.LayoutViewCloseZoomBtnHintClose: return "还原视图";
                    case GridStringId.LayoutViewCloseZoomBtnHintZoom: return "最大化详细信息";
                    case GridStringId.LayoutViewColumnModeBtnHint: return "一个栏位";
                    case GridStringId.LayoutViewCustomizationFormCaption: return "自定义查看面板";
                    case GridStringId.LayoutViewCustomizationFormDescription: return "通过拖放自定义卡面板和菜单，并且可在查看面板中预览数据.";
                    case GridStringId.LayoutViewCustomizeBtnHint: return "自定义";
                    case GridStringId.LayoutViewGroupCaptions: return "主题";
                    case GridStringId.LayoutViewGroupCards: return "卡";
                    case GridStringId.LayoutViewGroupCustomization: return "自定义";
                    case GridStringId.LayoutViewGroupFields: return "区域";
                    case GridStringId.LayoutViewGroupHiddenItems: return "隐藏项";
                    case GridStringId.LayoutViewGroupIndents: return "缩进";
                    case GridStringId.LayoutViewGroupIntervals: return "间隔";
                    case GridStringId.LayoutViewGroupLayout: return "布局";
                    case GridStringId.LayoutViewGroupPropertyGrid: return "属性栅格";
                    case GridStringId.LayoutViewGroupTreeStructure: return "树形布局查看";
                    case GridStringId.LayoutViewGroupView: return "查看";
                    case GridStringId.LayoutViewLabelAllowFieldHotTracking: return "允许热跟踪";
                    case GridStringId.LayoutViewLabelCaptionLocation: return "区域主题位置";
                    case GridStringId.LayoutViewLabelCardArrangeRule: return "排列规则:";
                    case GridStringId.LayoutViewLabelCardEdgeAlignment: return "卡边缘对齐方式:";
                    case GridStringId.LayoutViewLabelGroupCaptionLocation: return "组标题位置:";
                    case GridStringId.LayoutViewLabelHorizontal: return "水平间隔";
                    case GridStringId.LayoutViewLabelPadding: return "填充";
                    case GridStringId.LayoutViewLabelScrollVisibility: return "滚动条可见:";
                    case GridStringId.LayoutViewLabelShowCardBorder: return "显示边界";
                    case GridStringId.LayoutViewLabelShowCardCaption: return "显示标题";
                    case GridStringId.LayoutViewLabelShowCardExpandButton: return "显示展开按钮";
                    case GridStringId.LayoutViewLabelShowFieldBorder: return "显示边界";
                    case GridStringId.LayoutViewLabelShowFieldHint: return "显示提示";
                    case GridStringId.LayoutViewLabelShowFilterPanel: return "显示过滤面板";
                    case GridStringId.LayoutViewLabelShowHeaderPanel: return "显示表头面板";
                    case GridStringId.LayoutViewLabelShowLines: return "显示线条";
                    case GridStringId.LayoutViewLabelSpacing: return "间距";
                    case GridStringId.LayoutViewLabelTextAlignment: return "文本对其方式:";
                    case GridStringId.LayoutViewLabelTextIndent: return "文本缩进";
                    case GridStringId.LayoutViewLabelVertical: return "垂直间隔";
                    case GridStringId.LayoutViewLabelViewMode: return "查看模式";
                    case GridStringId.LayoutViewMultiColumnModeBtnHint: return "多列";
                    case GridStringId.LayoutViewMultiRowModeBtnHint: return "多行";
                    case GridStringId.LayoutViewPageTemplateCard: return "模板卡";
                    case GridStringId.LayoutViewPageViewLayout: return "查看版面";
                    case GridStringId.LayoutViewPanBtnHint: return "面板";
                    case GridStringId.LayoutViewRowModeBtnHint: return "单行";
                    case GridStringId.LayoutViewSingleModeBtnHint: return "单卡";
                    case GridStringId.MenuColumnBestFit: return "单列自适应宽度";
                    case GridStringId.MenuColumnBestFitAllColumns: return "所有列自适应宽度";
                    case GridStringId.MenuColumnClearFilter: return "清除过滤器";
                    case GridStringId.MenuColumnClearSorting: return "清除排序设置";
                    case GridStringId.MenuColumnColumnCustomization: return "列选择";
                    case GridStringId.MenuColumnFilter: return "允许筛选数据";
                    case GridStringId.MenuColumnFilterEditor: return "设定数据筛选条件";
                    case GridStringId.MenuColumnGroup: return "根据此列分组";
                    case GridStringId.MenuColumnGroupBox: return "分组依据框";
                    case GridStringId.MenuColumnGroupSummarySortFormat: return "{1}依照-'{0}'-{2}";
                    case GridStringId.MenuColumnRemoveColumn: return "移除列";
                    case GridStringId.MenuColumnResetGroupSummarySort: return "清除摘要排序";
                    case GridStringId.MenuColumnShowColumn: return "显示列";
                    case GridStringId.MenuColumnSortAscending: return "升序排列";
                    case GridStringId.MenuColumnSortDescending: return "降序排列";
                    case GridStringId.MenuColumnSortGroupBySummaryMenu: return "按摘要排序";
                    case GridStringId.MenuColumnUnGroup: return "不分组";
                    case GridStringId.MenuFooterAverage: return "平均值";
                    case GridStringId.MenuFooterAverageFormat: return "平均={0:#.##}";
                    case GridStringId.MenuFooterCount: return "计数";
                    case GridStringId.MenuFooterCountGroupFormat: return "计数={0}";
                    case GridStringId.MenuFooterCustomFormat: return "统计值={0}";
                    case GridStringId.MenuFooterMax: return "最大值";
                    case GridStringId.MenuFooterMaxFormat: return "最大值={0}";
                    case GridStringId.MenuFooterMin: return "最小值";
                    case GridStringId.MenuFooterMinFormat: return "最小值={0}";
                    case GridStringId.MenuFooterNone: return "无";
                    case GridStringId.MenuFooterSum: return "和";
                    case GridStringId.MenuFooterSumFormat: return "和={0:#.##}";
                    case GridStringId.MenuGroupPanelClearGrouping: return "清除分组";
                    case GridStringId.MenuGroupPanelFullCollapse: return "全部收合";
                    case GridStringId.MenuGroupPanelFullExpand: return "全部展开";
                    case GridStringId.PopupFilterAll: return "(全部)";
                    case GridStringId.PopupFilterBlanks: return "(空白)";
                    case GridStringId.PopupFilterCustom: return "(自定义)";
                    case GridStringId.PopupFilterNonBlanks: return "(无空白)";
                    case GridStringId.PrintDesignerBandedView: return "打印设置(BandedView)";
                    case GridStringId.PrintDesignerBandHeader: return "起始带宽";
                    case GridStringId.PrintDesignerCardView: return "打印设置(卡视图)";
                    case GridStringId.PrintDesignerDescription: return "为当前视图设置不同的打印选项";
                    case GridStringId.PrintDesignerGridView: return "打印设置(网格视图)";
                    case GridStringId.PrintDesignerLayoutView: return "打印设置(版面视图)";
                    case GridStringId.MenuColumnAutoFilterRowShow: return "显示自动过滤行";
                    case GridStringId.MenuColumnAutoFilterRowHide: return "隐藏自动过滤行";
                    case GridStringId.MenuColumnFindFilterHide: return "隐藏搜索面板";
                    case GridStringId.MenuColumnFindFilterShow: return "显示搜索面板";
                    case GridStringId.MenuGroupPanelShow: return "显示分组框";
                    case GridStringId.MenuGroupPanelHide: return "隐藏分组框";
                    case GridStringId.MenuColumnBandCustomization: return "列选择";

                }
                System.Diagnostics.Debug.WriteLine(id.ToString() + "的默认值(" + this.GetType().ToString() + ")=" + base.GetLocalizedString(id));
                return base.GetLocalizedString(id);
            }
        }

        public class MyPreviewLocalizer : DevExpress.XtraPrinting.Localization.PreviewLocalizer
        {
            public override string GetLocalizedString(DevExpress.XtraPrinting.Localization.PreviewStringId id)
            {
                switch (id)
                {
                    case PreviewStringId.BarText_MainMenu: return "主菜单";
                    case PreviewStringId.BarText_StatusBar: return "状态条";
                    case PreviewStringId.BarText_Toolbar: return "工具条";
                    case PreviewStringId.Button_Apply: return "应用";
                    case PreviewStringId.Button_Cancel: return "取消";
                    case PreviewStringId.Button_Help: return "帮助";
                    case PreviewStringId.Button_Ok: return "确定";
                    case PreviewStringId.EMail_From: return "来自";
                    case PreviewStringId.ExportOption_ConfirmationDoesNotMatchForm_Msg: return "确认密码不匹配。请从头开始，再次输入该密码。";
                    case PreviewStringId.ExportOption_ConfirmOpenPasswordForm_Caption: return "确认文档打开密码";
                    case PreviewStringId.ExportOption_ConfirmOpenPasswordForm_Name: return "文档打开密码:";
                    case PreviewStringId.ExportOption_ConfirmOpenPasswordForm_Note: return "请确认文档打开口令。一定要记下该密码。它将需要打开的文档。";
                    case PreviewStringId.ExportOption_ConfirmPermissionsPasswordForm_Caption: return "确认权限密码";
                    case PreviewStringId.ExportOption_ConfirmPermissionsPasswordForm_Name: return "权限密码:";
                    case PreviewStringId.ExportOption_ConfirmPermissionsPasswordForm_Note: return "请确认权限密码。一定要记下该密码。你将会需要它在将来更改这些设置。";
                    case PreviewStringId.ExportOption_HtmlCharacterSet: return "个性化设置：";
                    case PreviewStringId.ExportOption_HtmlEmbedImagesInHTML: return "在 HTML 中嵌入图像";
                    case PreviewStringId.ExportOption_HtmlExportMode: return "输出模式：";
                    case PreviewStringId.ExportOption_HtmlExportMode_DifferentFiles: return "不同的文件";
                    case PreviewStringId.ExportOption_HtmlExportMode_SingleFile: return "排成一列";
                    case PreviewStringId.ExportOption_HtmlExportMode_SingleFilePageByPage: return "逐页排成一列";
                    case PreviewStringId.ExportOption_HtmlPageBorderColor: return "页面边界颜色：";
                    case PreviewStringId.ExportOption_HtmlPageBorderWidth: return "页面边界宽度：";
                    case PreviewStringId.ExportOption_HtmlPageRange: return "页面范围：";
                    case PreviewStringId.ExportOption_HtmlRemoveSecondarySymbols: return "删除回车";
                    case PreviewStringId.ExportOption_HtmlTitle: return "标题：";
                    case PreviewStringId.ExportOption_ImageExportMode: return "输出模式：";
                    case PreviewStringId.ExportOption_ImageExportMode_DifferentFiles: return "不同的文件";
                    case PreviewStringId.ExportOption_ImageExportMode_SingleFile: return "单个文件";
                    case PreviewStringId.ExportOption_ImageExportMode_SingleFilePageByPage: return "单文件页";
                    case PreviewStringId.ExportOption_ImageFormat: return "图象格式：";
                    case PreviewStringId.ExportOption_ImagePageBorderColor: return "页边框颜色：";
                    case PreviewStringId.ExportOption_ImagePageBorderWidth: return "页边框宽度：";
                    case PreviewStringId.ExportOption_ImagePageRange: return "页范围：";
                    case PreviewStringId.ExportOption_ImageResolution: return "分辨率 (dpi):";
                    case PreviewStringId.ExportOption_NativeFormatCompressed: return "压缩";
                    case PreviewStringId.ExportOption_PdfChangingPermissions_AnyExceptExtractingPages: return "除了提取页面";
                    case PreviewStringId.ExportOption_PdfChangingPermissions_CommentingFillingSigning: return "注释、 填写表单域和签名的现有签名域";
                    case PreviewStringId.ExportOption_PdfChangingPermissions_FillingSigning: return "在表单域中填写及签署的现有签名域";
                    case PreviewStringId.ExportOption_PdfChangingPermissions_InsertingDeletingRotating: return "插入、 删除和旋转页面";
                    case PreviewStringId.ExportOption_PdfChangingPermissions_None: return "无";
                    case PreviewStringId.ExportOption_PdfCompressed: return "压缩";
                    case PreviewStringId.ExportOption_PdfConvertImagesToJpeg: return "转换为 Jpeg 图像";
                    case PreviewStringId.ExportOption_PdfDocumentApplication: return "应用：";
                    case PreviewStringId.ExportOption_PdfDocumentAuthor: return "作者：";
                    case PreviewStringId.ExportOption_PdfDocumentKeywords: return "关键字：";
                    case PreviewStringId.ExportOption_PdfDocumentSubject: return "主题：";
                    case PreviewStringId.ExportOption_PdfDocumentTitle: return "标题：";
                    case PreviewStringId.ExportOption_PdfImageQuality: return "图象质量：";
                    case PreviewStringId.ExportOption_PdfImageQuality_High: return "高";
                    case PreviewStringId.ExportOption_PdfImageQuality_Highest: return "最高";
                    case PreviewStringId.ExportOption_PdfImageQuality_Low: return "低";
                    case PreviewStringId.ExportOption_PdfImageQuality_Lowest: return "最低";
                    case PreviewStringId.ExportOption_PdfImageQuality_Medium: return "中等";
                    case PreviewStringId.ExportOption_PdfNeverEmbeddedFonts: return "不插入这些字体：";
                    case PreviewStringId.ExportOption_PdfPageRange: return "页面范围：";
                    case PreviewStringId.ExportOption_PdfPasswordSecurityOptions: return "密码安全：";
                    case PreviewStringId.ExportOption_PdfPasswordSecurityOptions_DocumentOpenPassword: return "文档打开密码";
                    case PreviewStringId.ExportOption_PdfPasswordSecurityOptions_None: return "(无)";
                    case PreviewStringId.ExportOption_PdfPasswordSecurityOptions_Permissions: return "权限";
                    case PreviewStringId.ExportOption_PdfPrintingPermissions_HighResolution: return "高分辨率";
                    case PreviewStringId.ExportOption_PdfPrintingPermissions_LowResolution: return "低分辨率 (150 dpi)";
                    case PreviewStringId.ExportOption_PdfPrintingPermissions_None: return "无";
                    case PreviewStringId.ExportOption_PdfShowPrintDialogOnOpen: return "打开时显示打印对话框";
                    case PreviewStringId.ExportOption_PdfSignature_EmptyCertificate: return "无";
                    case PreviewStringId.ExportOption_PdfSignature_Issuer: return "发行人：";
                    case PreviewStringId.ExportOption_PdfSignatureOptions: return "数字签名：";
                    case PreviewStringId.ExportOption_PdfSignatureOptions_Certificate: return "证书";
                    case PreviewStringId.ExportOption_PdfSignatureOptions_ContactInfo: return "联系信息";
                    case PreviewStringId.ExportOption_PdfSignatureOptions_Location: return "位置";
                    case PreviewStringId.ExportOption_PdfSignatureOptions_None: return "(无)";
                    case PreviewStringId.ExportOption_PdfSignatureOptions_Reason: return "原因";
                    case PreviewStringId.ExportOption_RtfExportMode: return "导出模式：";
                    case PreviewStringId.ExportOption_RtfExportMode_SingleFile: return "单个文件";
                    case PreviewStringId.ExportOption_RtfExportMode_SingleFilePageByPage: return "单文件页";
                    case PreviewStringId.ExportOption_RtfExportWatermarks: return "导出水印";
                    case PreviewStringId.ExportOption_RtfPageRange: return "页范围：";
                    case PreviewStringId.ExportOption_TextEncoding: return "编码：";
                    case PreviewStringId.ExportOption_TextExportMode: return "文本导出模式：";
                    case PreviewStringId.ExportOption_TextExportMode_Text: return "文本";
                    case PreviewStringId.ExportOption_TextExportMode_Value: return "值";
                    case PreviewStringId.ExportOption_TextSeparator: return "文本分隔器：";
                    case PreviewStringId.ExportOption_TextSeparator_TabAlias: return "选项卡";
                    case PreviewStringId.ExportOption_XlsExportHyperlinks: return "导出超链接";
                    case PreviewStringId.ExportOption_XlsExportMode: return "导出模式：";
                    case PreviewStringId.ExportOption_XlsExportMode_DifferentFiles: return "不同的文件";
                    case PreviewStringId.ExportOption_XlsExportMode_SingleFile: return "单个文件";
                    case PreviewStringId.ExportOption_XlsPageRange: return "页范围：";
                    case PreviewStringId.ExportOption_XlsRawDataMode: return "原始数据模式";
                    case PreviewStringId.ExportOption_XlsSheetName: return "工作表名称:";
                    case PreviewStringId.ExportOption_XlsShowGridLines: return "显示栅格线";
                    case PreviewStringId.ExportOption_XlsUseNativeFormat: return "以相应格式输出值";
                    case PreviewStringId.ExportOption_XlsxExportMode: return "出口模式：";
                    case PreviewStringId.ExportOption_XlsxExportMode_DifferentFiles: return "不同的文件";
                    case PreviewStringId.ExportOption_XlsxExportMode_SingleFile: return "单个文件";
                    case PreviewStringId.ExportOption_XlsxExportMode_SingleFilePageByPage: return "单文件页";
                    case PreviewStringId.ExportOption_XlsxPageRange: return "页范围：";
                    case PreviewStringId.ExportOption_XpsCompression: return "压缩：";
                    case PreviewStringId.ExportOption_XpsCompression_Fast: return "快速";
                    case PreviewStringId.ExportOption_XpsCompression_Maximum: return "最大";
                    case PreviewStringId.ExportOption_XpsCompression_Normal: return "正常";
                    case PreviewStringId.ExportOption_XpsCompression_NotCompressed: return "不压缩";
                    case PreviewStringId.ExportOption_XpsCompression_SuperFast: return "超级快";
                    case PreviewStringId.ExportOption_XpsDocumentCategory: return "类别：";
                    case PreviewStringId.ExportOption_XpsDocumentCreator: return "创建者：";
                    case PreviewStringId.ExportOption_XpsDocumentDescription: return "说明：";
                    case PreviewStringId.ExportOption_XpsDocumentKeywords: return "关键字：";
                    case PreviewStringId.ExportOption_XpsDocumentSubject: return "主题:";
                    case PreviewStringId.ExportOption_XpsDocumentTitle: return "标题：";
                    case PreviewStringId.ExportOption_XpsDocumentVersion: return "版本：";
                    case PreviewStringId.ExportOption_XpsPageRange: return "页面范围：";
                    case PreviewStringId.ExportOptionsForm_CaptionCsv: return "Csv输出选项";
                    case PreviewStringId.ExportOptionsForm_CaptionHtml: return "Html输出选项";
                    case PreviewStringId.ExportOptionsForm_CaptionImage: return "图象输出选项";
                    case PreviewStringId.ExportOptionsForm_CaptionMht: return "Mht输出选项";
                    case PreviewStringId.ExportOptionsForm_CaptionNativeOptions: return "本机格式选项";
                    case PreviewStringId.ExportOptionsForm_CaptionPdf: return "Pdf输出选项";
                    case PreviewStringId.ExportOptionsForm_CaptionRtf: return "Rtf输出选项";
                    case PreviewStringId.ExportOptionsForm_CaptionTxt: return "文本输出选项";
                    case PreviewStringId.ExportOptionsForm_CaptionXls: return "Xls输出选项";
                    case PreviewStringId.ExportOptionsForm_CaptionXlsx: return "XLSX导出选项";
                    case PreviewStringId.ExportOptionsForm_CaptionXps: return "XPS导出选项";
                    case PreviewStringId.FolderBrowseDlg_ExportDirectory: return "选择一个文件夹保存输出文档：";
                    case PreviewStringId.Margin_BottomMargin: return "下页边距";
                    case PreviewStringId.Margin_Inch: return "英寸";
                    case PreviewStringId.Margin_LeftMargin: return "左页边距";
                    case PreviewStringId.Margin_Millimeter: return "毫米";
                    case PreviewStringId.Margin_RightMargin: return "右页边距";
                    case PreviewStringId.Margin_TopMargin: return "上页边距";
                    case PreviewStringId.MenuItem_BackgrColor: return "颜色...";
                    case PreviewStringId.MenuItem_Background: return "背景...";
                    case PreviewStringId.MenuItem_CsvDocument: return "CSV文件";
                    case PreviewStringId.MenuItem_Exit: return "退出";
                    case PreviewStringId.MenuItem_Export: return "输出到";
                    case PreviewStringId.MenuItem_File: return "文件";
                    case PreviewStringId.MenuItem_GraphicDocument: return "图象文件";
                    case PreviewStringId.MenuItem_HtmDocument: return "HTML文件";
                    case PreviewStringId.MenuItem_MhtDocument: return "MHT文件";
                    case PreviewStringId.MenuItem_PageLayout: return "页面布局";
                    case PreviewStringId.MenuItem_PageSetup: return "页面调整";
                    case PreviewStringId.MenuItem_PdfDocument: return "PDF文件";
                    case PreviewStringId.MenuItem_Print: return "打印...";
                    case PreviewStringId.MenuItem_PrintDirect: return "打印";
                    case PreviewStringId.MenuItem_RtfDocument: return "RTF文件";
                    case PreviewStringId.MenuItem_Send: return "以...格式发送";
                    case PreviewStringId.MenuItem_TxtDocument: return "文本文件";
                    case PreviewStringId.MenuItem_View: return "视图";
                    case PreviewStringId.MenuItem_ViewContinuous: return "继续";
                    case PreviewStringId.MenuItem_ViewFacing: return "朝向";
                    case PreviewStringId.MenuItem_ViewStatusbar: return "状态条";
                    case PreviewStringId.MenuItem_ViewToolbar: return "工具条";
                    case PreviewStringId.MenuItem_Watermark: return "水印...";
                    case PreviewStringId.MenuItem_XlsDocument: return "Excel文件";
                    case PreviewStringId.MenuItem_XlsxDocument: return "兑换 XLSX 文件";
                    case PreviewStringId.MenuItem_ZoomPageWidth: return "页面宽度";
                    case PreviewStringId.MenuItem_ZoomTextWidth: return "文本宽度";
                    case PreviewStringId.MenuItem_ZoomTwoPages: return "两页";
                    case PreviewStringId.MenuItem_ZoomWholePage: return "整个页面";
                    case PreviewStringId.MPForm_Lbl_Pages: return "页面";
                    case PreviewStringId.Msg_CannotAccessFile: return "这个进程无法读取文件\"{0}\"，因为它正在被另一个进程使用。";
                    case PreviewStringId.Msg_CannotLoadDocument: return "指定的文件不能加载，因为它不包含有效的 XML 数据或超出允许的大小。";
                    case PreviewStringId.Msg_CantFitBarcodeToControlBounds: return "对于条码来说控件的边界太小。";
                    case PreviewStringId.Msg_Caption: return "打印";
                    case PreviewStringId.Msg_CreatingDocument: return "建立文档...";
                    case PreviewStringId.Msg_CustomDrawWarning: return "警告！";
                    case PreviewStringId.Msg_EmptyDocument: return "该文档不包含任何页。";
                    case PreviewStringId.Msg_ErrorTitle: return "错误";
                    case PreviewStringId.Msg_FileDoesNotContainValidXml: return "指定的文件不包含有效的 XML 数据中的 PRNX 格式。停止加载。";
                    case PreviewStringId.Msg_FileReadOnly: return "文件\"{0}\"设置为只读，请用不同的文件名再试。";
                    case PreviewStringId.Msg_FontInvalidNumber: return "字体大小不能设置为0或者负数。";
                    case PreviewStringId.Msg_GoToNonExistentPage: return "在此文档中有没有页面编号为 {0}。";
                    case PreviewStringId.Msg_IncorrectPageRange: return "这不是有效的页面范围";
                    case PreviewStringId.Msg_IncorrectZoomFactor: return "数字大小必须界于{0}，{1}之间。";
                    case PreviewStringId.Msg_InvalidBarcodeData: return "二进制数据不能超过 1033 字节。";
                    case PreviewStringId.Msg_InvalidBarcodeText: return "文本中有无效字符。";
                    case PreviewStringId.Msg_InvalidBarcodeTextFormat: return "无效的文本格式";
                    case PreviewStringId.Msg_InvalidMeasurement: return "这不是一个有效的度量值。";
                    case PreviewStringId.Msg_InvPropName: return "无效的属性名称";
                    case PreviewStringId.Msg_NeedPrinter: return "没有安装打印机。";
                    case PreviewStringId.Msg_NoParameters: return "不存在指定的参数： {0}。";
                    case PreviewStringId.Msg_NotSupportedFont: return "尚不支持该字体";
                    case PreviewStringId.Msg_OpenFileQuestion: return "你想打开该文件吗？";
                    case PreviewStringId.Msg_OpenFileQuestionCaption: return "输出";
                    case PreviewStringId.Msg_PageMarginsWarning: return "一个或多个页边距被设置到也可打印的页面范围之外，是否要继续？";
                    case PreviewStringId.Msg_PathTooLong: return "路径太长。尝试较短的名称。";
                    case PreviewStringId.Msg_SearchDialogFinishedSearching: return "完成对整个文档的搜索";
                    case PreviewStringId.Msg_SearchDialogReady: return "准备就绪";
                    case PreviewStringId.Msg_SearchDialogTotalFound: return "总共发现：";
                    case PreviewStringId.Msg_SeparatorCannotBeEmptyString: return "分隔符不能为空字符串。";
                    case PreviewStringId.Msg_UnavailableNetPrinter: return "网络打印机无法使用。";
                    case PreviewStringId.Msg_WrongPageSettings: return "当前打印机不支持所选择页面大小。一定要继续吗？";
                    case PreviewStringId.Msg_WrongPrinter: return "打印机的名字是无效的。请检查打印机的设置。";
                    case PreviewStringId.NoneString: return "(无)";
                    case PreviewStringId.OpenFileDialog_Filter: return "预览的文档文件 (* {0}) | * {0} |所有文件 (*.*) | *.*";
                    case PreviewStringId.OpenFileDialog_Title: return "打开";
                    case PreviewStringId.PageInfo_PageDate: return "[已打印数据]";
                    case PreviewStringId.PageInfo_PageNumber: return "[页#]";
                    case PreviewStringId.PageInfo_PageNumberOfTotal: return "[页#，共#页]";
                    case PreviewStringId.PageInfo_PageTime: return "[打印耗时]";
                    case PreviewStringId.PageInfo_PageTotal: return "[页 #]";
                    case PreviewStringId.PageInfo_PageUserName: return "[用户名]";
                    case PreviewStringId.ParametersRequest_Caption: return "参数";
                    case PreviewStringId.ParametersRequest_Reset: return "重置";
                    case PreviewStringId.ParametersRequest_Submit: return "提交";
                    case PreviewStringId.PreviewForm_Caption: return "预览";
                    //case PreviewStringId.PrinterStatus_Error :  return "错误";  
                    case PreviewStringId.RibbonPreview_ClosePreview_Caption: return "关闭打印预览";
                    case PreviewStringId.RibbonPreview_ClosePreview_STipContent: return "关闭该文档的打印预览";
                    case PreviewStringId.RibbonPreview_ClosePreview_STipTitle: return "关闭打印预览";
                    case PreviewStringId.RibbonPreview_Customize_Caption: return "选项";
                    case PreviewStringId.RibbonPreview_Customize_STipContent: return "打开可打印的组件编辑器对话框，并可以改变打印选项。";
                    case PreviewStringId.RibbonPreview_Customize_STipTitle: return "选项";
                    case PreviewStringId.RibbonPreview_DocumentMap_Caption: return "书签";
                    case PreviewStringId.RibbonPreview_DocumentMap_STipContent: return "打开文档结构图为你导航文档的结构。";
                    case PreviewStringId.RibbonPreview_DocumentMap_STipTitle: return "文档结构图";
                    case PreviewStringId.RibbonPreview_EditPageHF_Caption: return "页眉／页脚";
                    case PreviewStringId.RibbonPreview_EditPageHF_STipContent: return "编辑该文档的页眉和页脚";
                    case PreviewStringId.RibbonPreview_EditPageHF_STipTitle: return "页眉和页脚";
                    case PreviewStringId.RibbonPreview_ExportCsv_Caption: return "CSV文件";
                    case PreviewStringId.RibbonPreview_ExportCsv_Description: return "逗号分隔值文本";
                    case PreviewStringId.RibbonPreview_ExportCsv_STipContent: return "将该文档以CSV格式输出并保存到磁盘文件上。";
                    case PreviewStringId.RibbonPreview_ExportCsv_STipTitle: return "以CSV格式输出";
                    case PreviewStringId.RibbonPreview_ExportFile_Caption: return "输出";
                    case PreviewStringId.RibbonPreview_ExportFile_STipContent: return "将当前文档以一个可用的格式输出，并将其保存到磁盘文件上。";
                    case PreviewStringId.RibbonPreview_ExportFile_STipTitle: return "输出...";
                    case PreviewStringId.RibbonPreview_ExportGraphic_Caption: return "图象文件";
                    case PreviewStringId.RibbonPreview_ExportGraphic_Description: return "BMP、 GIF、 JPEG、 PNG、 TIFF、 EMF、 WMF";
                    case PreviewStringId.RibbonPreview_ExportGraphic_STipContent: return "将该文档以图象格式输出并保存到磁盘文件上。";
                    case PreviewStringId.RibbonPreview_ExportGraphic_STipTitle: return "以图象格式输出";
                    case PreviewStringId.RibbonPreview_ExportHtm_Caption: return "HTML文件";
                    case PreviewStringId.RibbonPreview_ExportHtm_Description: return "Web页面";
                    case PreviewStringId.RibbonPreview_ExportHtm_STipContent: return "将该文档以HTML格式输出并保存到磁盘文件上。";
                    case PreviewStringId.RibbonPreview_ExportHtm_STipTitle: return "以HTML格式输出";
                    case PreviewStringId.RibbonPreview_ExportMht_Caption: return "MHT文件";
                    case PreviewStringId.RibbonPreview_ExportMht_Description: return "单一文件的Web页";
                    case PreviewStringId.RibbonPreview_ExportMht_STipContent: return "将该文档以MHT格式输出并保存到磁盘文件上。";
                    case PreviewStringId.RibbonPreview_ExportMht_STipTitle: return "以MHT格式输出";
                    case PreviewStringId.RibbonPreview_ExportPdf_Caption: return "PDF文件";
                    case PreviewStringId.RibbonPreview_ExportPdf_Description: return "Adobe便携式文档格式";
                    case PreviewStringId.RibbonPreview_ExportPdf_STipContent: return "将该文档以PDF格式输出并保存到磁盘文件上。";
                    case PreviewStringId.RibbonPreview_ExportPdf_STipTitle: return "以PDF格式输出";
                    case PreviewStringId.RibbonPreview_ExportRtf_Caption: return "RTF文件";
                    case PreviewStringId.RibbonPreview_ExportRtf_Description: return "多本文格式";
                    case PreviewStringId.RibbonPreview_ExportRtf_STipContent: return "将该文档以RTF格式输出并保存到磁盘文件上。";
                    case PreviewStringId.RibbonPreview_ExportRtf_STipTitle: return "以RTF格式输出";
                    case PreviewStringId.RibbonPreview_ExportTxt_Caption: return "文本文件";
                    case PreviewStringId.RibbonPreview_ExportTxt_Description: return "纯文本";
                    case PreviewStringId.RibbonPreview_ExportTxt_STipContent: return "将该文档以文本格式输出并保存到磁盘文件上。";
                    case PreviewStringId.RibbonPreview_ExportTxt_STipTitle: return "以文本格式输出";
                    case PreviewStringId.RibbonPreview_ExportXls_Caption: return "Excel文件";
                    case PreviewStringId.RibbonPreview_ExportXls_Description: return "Microsoft Excel工作薄";
                    case PreviewStringId.RibbonPreview_ExportXls_STipContent: return "将该文档以XLS格式输出并保存到磁盘文件上。";
                    case PreviewStringId.RibbonPreview_ExportXls_STipTitle: return "以XLS格式输出";
                    case PreviewStringId.RibbonPreview_ExportXlsx_Caption: return "XLSX 文件";
                    case PreviewStringId.RibbonPreview_ExportXlsx_Description: return "Microsoft Excel 2007 工作簿";
                    case PreviewStringId.RibbonPreview_ExportXlsx_STipContent: return "将文档导出到 XLSX 并将其保存到磁盘上的文件。";
                    case PreviewStringId.RibbonPreview_ExportXlsx_STipTitle: return "导出到 XLSX";
                    case PreviewStringId.RibbonPreview_ExportXps_Caption: return "XPS 文件";
                    case PreviewStringId.RibbonPreview_ExportXps_Description: return "XPS";
                    case PreviewStringId.RibbonPreview_FillBackground_Caption: return "页面颜色";
                    case PreviewStringId.RibbonPreview_FillBackground_STipContent: return "为文档页面背景选择颜色。";
                    case PreviewStringId.RibbonPreview_FillBackground_STipTitle: return "背景颜色";
                    case PreviewStringId.RibbonPreview_Find_Caption: return "查找";
                    case PreviewStringId.RibbonPreview_Find_STipContent: return "显示查找对话框，查找文档中的文本。";
                    case PreviewStringId.RibbonPreview_Find_STipTitle: return "查找";
                    case PreviewStringId.RibbonPreview_GalleryItem_PageMargins_Description: return "上:{0}下:{1}左:{2}右:{3}";
                    case PreviewStringId.RibbonPreview_GalleryItem_PageMarginsModerate_Caption: return "中等";
                    case PreviewStringId.RibbonPreview_GalleryItem_PageMarginsModerate_Description: return "中等";
                    case PreviewStringId.RibbonPreview_GalleryItem_PageMarginsNarrow_Caption: return "窄";
                    case PreviewStringId.RibbonPreview_GalleryItem_PageMarginsNarrow_Description: return "窄";
                    case PreviewStringId.RibbonPreview_GalleryItem_PageMarginsNormal_Caption: return "正常";
                    case PreviewStringId.RibbonPreview_GalleryItem_PageMarginsNormal_Description: return "正常";
                    case PreviewStringId.RibbonPreview_GalleryItem_PageMarginsWide_Caption: return "宽";
                    case PreviewStringId.RibbonPreview_GalleryItem_PageMarginsWide_Description: return "宽";
                    case PreviewStringId.RibbonPreview_GalleryItem_PageOrientationLandscape_Caption: return "横向";
                    case PreviewStringId.RibbonPreview_GalleryItem_PageOrientationPortrait_Caption: return "纵向";
                    case PreviewStringId.RibbonPreview_GalleryItem_PaperSize_Description: return "{0} x {1}";
                    case PreviewStringId.RibbonPreview_HandTool_Caption: return "抓取工具";
                    case PreviewStringId.RibbonPreview_HandTool_STipContent: return "调用抓取工具手动拖拽查看页面。";
                    case PreviewStringId.RibbonPreview_HandTool_STipTitle: return "抓取工具";
                    case PreviewStringId.RibbonPreview_Magnifier_Caption: return "放大";
                    case PreviewStringId.RibbonPreview_Magnifier_STipContent: return "调用放大镜工具";
                    case PreviewStringId.RibbonPreview_Magnifier_STipTitle: return "放大镜";
                    case PreviewStringId.RibbonPreview_MultiplePages_Caption: return "多页";
                    case PreviewStringId.RibbonPreview_MultiplePages_STipContent: return "选择页面布局以便在预览中排放文档页面。";
                    case PreviewStringId.RibbonPreview_MultiplePages_STipTitle: return "多页查看";
                    case PreviewStringId.RibbonPreview_Open_Caption: return "打开";
                    case PreviewStringId.RibbonPreview_PageGroup_Background: return "页面背景";
                    case PreviewStringId.RibbonPreview_PageGroup_Document: return "文档";
                    case PreviewStringId.RibbonPreview_PageGroup_Export: return "输出";
                    case PreviewStringId.RibbonPreview_PageGroup_Navigation: return "导航";
                    case PreviewStringId.RibbonPreview_PageGroup_PageSetup: return "页面调整";
                    case PreviewStringId.RibbonPreview_PageGroup_PageSetup_STipContent: return "显示页面调整对话框。";
                    case PreviewStringId.RibbonPreview_PageGroup_PageSetup_STipTitle: return "页面调整";
                    case PreviewStringId.RibbonPreview_PageGroup_Print: return "打印";
                    case PreviewStringId.RibbonPreview_PageGroup_Zoom: return "缩放";
                    case PreviewStringId.RibbonPreview_PageMargins_Caption: return "页边距";
                    case PreviewStringId.RibbonPreview_PageMargins_STipContent: return "为整个文档选择页边距大小。点击定制页边距为文档应用指定的页边距大小。";
                    case PreviewStringId.RibbonPreview_PageMargins_STipTitle: return "页边距";
                    case PreviewStringId.RibbonPreview_PageOrientation_Caption: return "方位";
                    case PreviewStringId.RibbonPreview_PageOrientation_STipContent: return "在纵向和横向布局之间转换页面。";
                    case PreviewStringId.RibbonPreview_PageOrientation_STipTitle: return "页面方向";
                    case PreviewStringId.RibbonPreview_PageSetup_Caption: return "定制页边距...";
                    case PreviewStringId.RibbonPreview_PageSetup_STipContent: return "显示页面调整对话框。";
                    case PreviewStringId.RibbonPreview_PageSetup_STipTitle: return "页面调整";
                    case PreviewStringId.RibbonPreview_PageText: return "打印预览";
                    case PreviewStringId.RibbonPreview_PaperSize_Caption: return "大小";
                    case PreviewStringId.RibbonPreview_PaperSize_STipContent: return "选择文档的页面大小。";
                    case PreviewStringId.RibbonPreview_PaperSize_STipTitle: return "页面大小";
                    case PreviewStringId.RibbonPreview_Parameters_Caption: return "参数";
                    case PreviewStringId.RibbonPreview_Parameters_STipContent: return "打开参数窗格，使您可以为报表参数输入值。";
                    case PreviewStringId.RibbonPreview_Parameters_STipTitle: return "参数";
                    case PreviewStringId.RibbonPreview_Pointer_Caption: return "指针";
                    case PreviewStringId.RibbonPreview_Pointer_STipContent: return "显示鼠标指针。";
                    case PreviewStringId.RibbonPreview_Pointer_STipTitle: return "鼠标指针";
                    case PreviewStringId.RibbonPreview_Print_Caption: return "打印";
                    case PreviewStringId.RibbonPreview_Print_STipContent: return "在打印前选择打印机，打印份数以及其他打印选项。";
                    case PreviewStringId.RibbonPreview_Print_STipTitle: return "打印(Ctrl+P)";
                    case PreviewStringId.RibbonPreview_PrintDirect_Caption: return "快速打印";
                    case PreviewStringId.RibbonPreview_PrintDirect_STipContent: return "将文档不作任何修改直接送往默认打印机。";
                    case PreviewStringId.RibbonPreview_PrintDirect_STipTitle: return "快速打印";
                    case PreviewStringId.RibbonPreview_Save_Caption: return "保存";
                    case PreviewStringId.RibbonPreview_Scale_Caption: return "比例";
                    case PreviewStringId.RibbonPreview_Scale_STipContent: return "按实际大小的百分比伸展或收缩打印输出。";
                    case PreviewStringId.RibbonPreview_Scale_STipTitle: return "比例";
                    case PreviewStringId.RibbonPreview_SendCsv_Caption: return "CSV文件";
                    case PreviewStringId.RibbonPreview_SendCsv_Description: return "逗号分隔值文本";
                    case PreviewStringId.RibbonPreview_SendCsv_STipContent: return "以CSV格式输出文档，并且将其附到电子邮件中。";
                    case PreviewStringId.RibbonPreview_SendCsv_STipTitle: return "在电子邮件中以CSV格式发送";
                    case PreviewStringId.RibbonPreview_SendFile_Caption: return "在电子邮件中以...格式发送";
                    case PreviewStringId.RibbonPreview_SendFile_STipContent: return "以一种可用格式输出当前文档，并且将其附到电子邮件中。";
                    case PreviewStringId.RibbonPreview_SendFile_STipTitle: return "在电子邮件中以...格式发送";
                    case PreviewStringId.RibbonPreview_SendGraphic_Caption: return "图象文件";
                    case PreviewStringId.RibbonPreview_SendGraphic_STipContent: return "以图象格式输出文档，并且将其附到电子邮件中。";
                    case PreviewStringId.RibbonPreview_SendGraphic_STipTitle: return "在电子邮件中以图象格式发送";
                    case PreviewStringId.RibbonPreview_SendMht_Caption: return "MHT文件";
                    case PreviewStringId.RibbonPreview_SendMht_Description: return "单文件网页";
                    case PreviewStringId.RibbonPreview_SendMht_STipContent: return "以MHT格式输出文档，并且将其附到电子邮件中。";
                    case PreviewStringId.RibbonPreview_SendMht_STipTitle: return "在电子邮件中以MHT格式发送";
                    case PreviewStringId.RibbonPreview_SendPdf_Caption: return "PDF文件";
                    case PreviewStringId.RibbonPreview_SendPdf_Description: return "Adobe便携式文档格式";
                    case PreviewStringId.RibbonPreview_SendPdf_STipContent: return "以PDF格式输出文档，并且将其附到电子邮件中。";
                    case PreviewStringId.RibbonPreview_SendPdf_STipTitle: return "在电子邮件中以PDF格式发送";
                    case PreviewStringId.RibbonPreview_SendRtf_Caption: return "RTF文件";
                    case PreviewStringId.RibbonPreview_SendRtf_Description: return "多文本格式";
                    case PreviewStringId.RibbonPreview_SendRtf_STipContent: return "以RTF格式输出文档，并且将其附到电子邮件中。";
                    case PreviewStringId.RibbonPreview_SendRtf_STipTitle: return "在电子邮件中以RTF格式发送";
                    case PreviewStringId.RibbonPreview_SendTxt_Caption: return "文本文件";
                    case PreviewStringId.RibbonPreview_SendTxt_Description: return "纯文本";
                    case PreviewStringId.RibbonPreview_SendTxt_STipContent: return "以文本格式输出文档，并且将其附到电子邮件中。";
                    case PreviewStringId.RibbonPreview_SendTxt_STipTitle: return "在电子邮件中以文本格式发送";
                    case PreviewStringId.RibbonPreview_SendXls_Caption: return "Excel文件";
                    case PreviewStringId.RibbonPreview_SendXls_Description: return "Microsoft Excel工作薄";
                    case PreviewStringId.RibbonPreview_SendXls_STipContent: return "以XLS格式输出文档，并且将其附到电子邮件中。";
                    case PreviewStringId.RibbonPreview_SendXls_STipTitle: return "在电子邮件中以XLS格式发送";
                    case PreviewStringId.RibbonPreview_SendXlsx_Caption: return "兑换 XLSX 文件";
                    case PreviewStringId.RibbonPreview_SendXlsx_Description: return "Microsoft Excel 2007 工作簿";
                    case PreviewStringId.RibbonPreview_SendXlsx_STipContent: return "将文档导出到 XLSX 并将其附加到电子邮件。";
                    case PreviewStringId.RibbonPreview_SendXps_Caption: return "XPS 文件";
                    case PreviewStringId.RibbonPreview_SendXps_Description: return "XPS";
                    case PreviewStringId.RibbonPreview_ShowFirstPage_Caption: return "第一页";
                    case PreviewStringId.RibbonPreview_ShowFirstPage_STipContent: return "查看文档第一页。";
                    case PreviewStringId.RibbonPreview_ShowFirstPage_STipTitle: return "第一页(Ctrl+Home)";
                    case PreviewStringId.RibbonPreview_ShowLastPage_Caption: return "最后一页";
                    case PreviewStringId.RibbonPreview_ShowLastPage_STipContent: return "查看文档最后一页。";
                    case PreviewStringId.RibbonPreview_ShowLastPage_STipTitle: return "最后一页(Ctrl+End)";
                    case PreviewStringId.RibbonPreview_ShowNextPage_Caption: return "下一页";
                    case PreviewStringId.RibbonPreview_ShowNextPage_STipContent: return "查看文档下一页。";
                    case PreviewStringId.RibbonPreview_ShowNextPage_STipTitle: return "下一页(PageDown)";
                    case PreviewStringId.RibbonPreview_ShowPrevPage_Caption: return "上一页";
                    case PreviewStringId.RibbonPreview_ShowPrevPage_STipContent: return "查看文档上一页。";
                    case PreviewStringId.RibbonPreview_ShowPrevPage_STipTitle: return "上一页(PageUp)";
                    case PreviewStringId.RibbonPreview_Watermark_Caption: return "水印";
                    case PreviewStringId.RibbonPreview_Watermark_STipContent: return "在页面的目录后插入文本或者图象的镜象。这通常用于指示一个文档被特殊处理过。";
                    case PreviewStringId.RibbonPreview_Watermark_STipTitle: return "水印";
                    case PreviewStringId.RibbonPreview_Zoom_Caption: return "缩放";
                    case PreviewStringId.RibbonPreview_Zoom_STipContent: return "改变文档预览的缩放等级。";
                    case PreviewStringId.RibbonPreview_Zoom_STipTitle: return "缩放";
                    case PreviewStringId.RibbonPreview_ZoomExact_Caption: return "精确度：";
                    case PreviewStringId.RibbonPreview_ZoomIn_Caption: return "放大";
                    case PreviewStringId.RibbonPreview_ZoomIn_STipContent: return "放大以便得到文档的近视图。";
                    case PreviewStringId.RibbonPreview_ZoomIn_STipTitle: return "放大";
                    case PreviewStringId.RibbonPreview_ZoomOut_Caption: return "缩小";
                    case PreviewStringId.RibbonPreview_ZoomOut_STipContent: return "缩小以便在一个减小的尺寸上看到页面的更多部分。";
                    case PreviewStringId.RibbonPreview_ZoomOut_STipTitle: return "缩小";
                    case PreviewStringId.SaveDlg_FilterBmp: return "BMP比特图格式";
                    case PreviewStringId.SaveDlg_FilterCsv: return "CSV文档";
                    case PreviewStringId.SaveDlg_FilterEmf: return "EMF 增强的视窗图元元件";
                    case PreviewStringId.SaveDlg_FilterGif: return "GIF图形交换格式";
                    case PreviewStringId.SaveDlg_FilterHtm: return "HTML文档";
                    case PreviewStringId.SaveDlg_FilterJpeg: return "JPEG可交换文件格式";
                    case PreviewStringId.SaveDlg_FilterMht: return "MHT文档";
                    case PreviewStringId.SaveDlg_FilterPdf: return "PDF文档";
                    case PreviewStringId.SaveDlg_FilterPng: return "PNG流式网络图形格式";
                    case PreviewStringId.SaveDlg_FilterRtf: return "多文本文档";
                    case PreviewStringId.SaveDlg_FilterTiff: return "TIFF标签图像文件格式";
                    case PreviewStringId.SaveDlg_FilterTxt: return "文本文档";
                    case PreviewStringId.SaveDlg_FilterWmf: return "WMF 视窗图元文件";
                    case PreviewStringId.SaveDlg_FilterXls: return "Excel文档";
                    case PreviewStringId.SaveDlg_FilterXps: return "XPS 文档";
                    case PreviewStringId.SaveDlg_Title: return "存储为";
                    case PreviewStringId.SB_PageInfo: return "{0}在 {1}之中";
                    case PreviewStringId.SB_PageNone: return "空";
                    case PreviewStringId.SB_TTip_Stop: return "停止";
                    case PreviewStringId.SB_ZoomFactor: return "缩放比例：";
                    case PreviewStringId.ScalePopup_AdjustTo: return "调整至：";
                    case PreviewStringId.ScalePopup_FitTo: return "调整到";
                    case PreviewStringId.ScalePopup_GroupText: return "缩放比例";
                    case PreviewStringId.ScalePopup_NormalSize: return "%正常大小";
                    case PreviewStringId.ScalePopup_PagesWide: return "页面范围";
                    case PreviewStringId.ScrollingInfo_Page: return "页面";
                    case PreviewStringId.Shapes_Arrow: return "箭头";
                    case PreviewStringId.Shapes_BottomArrow: return "底部箭头";
                    case PreviewStringId.Shapes_Brace: return "大括号";
                    case PreviewStringId.Shapes_EightPointStar: return "8 角星";
                    case PreviewStringId.Shapes_Ellipse: return "椭圆";
                    case PreviewStringId.Shapes_FivePointStar: return "5 角星";
                    case PreviewStringId.Shapes_FourPointStar: return "4 角星";
                    case PreviewStringId.Shapes_Hexagon: return "六角形";
                    case PreviewStringId.Shapes_HorizontalLine: return "水平线";
                    case PreviewStringId.Shapes_Line: return "线";
                    case PreviewStringId.Shapes_Octagon: return "八角形";
                    case PreviewStringId.Shapes_Pentagon: return "五角形";
                    case PreviewStringId.Shapes_Polygon: return "多边形";
                    case PreviewStringId.Shapes_SixPointStar: return "6 角星";
                    case PreviewStringId.Shapes_SlantLine: return "斜线";
                    case PreviewStringId.Shapes_Square: return "正方形";
                    case PreviewStringId.Shapes_Star: return "星形";
                    case PreviewStringId.Shapes_ThreePointStar: return "3 角星";
                    case PreviewStringId.Shapes_TopArrow: return "顶部箭头";
                    case PreviewStringId.Shapes_Triangle: return "三角形";
                    case PreviewStringId.TB_TTip_Backgr: return "背景";
                    case PreviewStringId.TB_TTip_Close: return "关闭预览";
                    case PreviewStringId.TB_TTip_Customize: return "定制";
                    case PreviewStringId.TB_TTip_EditPageHF: return "页眉和页脚";
                    case PreviewStringId.TB_TTip_Export: return "输出文档...";
                    case PreviewStringId.TB_TTip_FirstPage: return "第一页";
                    case PreviewStringId.TB_TTip_HandTool: return "抓取工具";
                    case PreviewStringId.TB_TTip_LastPage: return "最后一页";
                    case PreviewStringId.TB_TTip_Magnifier: return "放大镜";
                    case PreviewStringId.TB_TTip_Map: return "文档结构图";
                    case PreviewStringId.TB_TTip_MultiplePages: return "显示多页";
                    case PreviewStringId.TB_TTip_NextPage: return "下一页";
                    case PreviewStringId.TB_TTip_Open: return "打开文档";
                    case PreviewStringId.TB_TTip_PageSetup: return "页面调整";
                    case PreviewStringId.TB_TTip_Parameters: return "参数";
                    case PreviewStringId.TB_TTip_PreviousPage: return "上一页";
                    case PreviewStringId.TB_TTip_Print: return "打印";
                    case PreviewStringId.TB_TTip_PrintDirect: return "快速打印";
                    case PreviewStringId.TB_TTip_Save: return "保存文档";
                    case PreviewStringId.TB_TTip_Scale: return "比例";
                    case PreviewStringId.TB_TTip_Search: return "搜索";
                    case PreviewStringId.TB_TTip_Send: return "通过电子邮件发送...";
                    case PreviewStringId.TB_TTip_Watermark: return "水印";
                    case PreviewStringId.TB_TTip_Zoom: return "缩放";
                    case PreviewStringId.TB_TTip_ZoomIn: return "放大";
                    case PreviewStringId.TB_TTip_ZoomOut: return "缩小";
                    case PreviewStringId.WatermarkTypePicture: return "（图片）";
                    case PreviewStringId.WatermarkTypeText: return "（文本）";
                    case PreviewStringId.WMForm_Direction_BackwardDiagonal: return "后向倾斜";
                    case PreviewStringId.WMForm_Direction_ForwardDiagonal: return "前向倾斜";
                    case PreviewStringId.WMForm_Direction_Horizontal: return "水平的";
                    case PreviewStringId.WMForm_Direction_Vertical: return "垂直的";
                    case PreviewStringId.WMForm_HorzAlign_Center: return "居中";
                    case PreviewStringId.WMForm_HorzAlign_Left: return "左";
                    case PreviewStringId.WMForm_HorzAlign_Right: return "右";
                    case PreviewStringId.WMForm_ImageClip: return "裁剪";
                    case PreviewStringId.WMForm_ImageStretch: return "拉伸";
                    case PreviewStringId.WMForm_ImageZoom: return "缩放";
                    case PreviewStringId.WMForm_PageRangeRgrItem_All: return "全部";
                    case PreviewStringId.WMForm_PageRangeRgrItem_Pages: return "页面";
                    case PreviewStringId.WMForm_PictureDlg_Title: return "选择图片";
                    case PreviewStringId.WMForm_VertAlign_Bottom: return "底部";
                    case PreviewStringId.WMForm_VertAlign_Middle: return "中间";
                    case PreviewStringId.WMForm_VertAlign_Top: return "上部";
                    case PreviewStringId.WMForm_Watermark_Asap: return "尽快";
                    case PreviewStringId.WMForm_Watermark_Confidential: return "机密";
                    case PreviewStringId.WMForm_Watermark_Copy: return "复制";
                    case PreviewStringId.WMForm_Watermark_DoNotCopy: return "不复制";
                    case PreviewStringId.WMForm_Watermark_Draft: return "草图";
                    case PreviewStringId.WMForm_Watermark_Evaluation: return "评价";
                    case PreviewStringId.WMForm_Watermark_Original: return "创新";
                    case PreviewStringId.WMForm_Watermark_Personal: return "个人";
                    case PreviewStringId.WMForm_Watermark_Sample: return "示例";
                    case PreviewStringId.WMForm_Watermark_TopSecret: return "最高机密";
                    case PreviewStringId.WMForm_Watermark_Urgent: return "紧迫";
                    case PreviewStringId.WMForm_ZOrderRgrItem_Behind: return "后方";
                    case PreviewStringId.WMForm_ZOrderRgrItem_InFront: return "前方";
                    default: return base.GetLocalizedString(id);
                }
            }
        }

        public class MyRichEditLocalizer : DevExpress.XtraRichEdit.Localization.XtraRichEditLocalizer
        {
            public override string Language { get { return "简体中文"; } }
            public override string GetLocalizedString(DevExpress.XtraRichEdit.Localization.XtraRichEditStringId id)
            {
                switch (id)
                {
                    case XtraRichEditStringId.MenuCmd_CopySelection: return "复制";
                    case XtraRichEditStringId.MenuCmd_Paste: return "粘贴";
                    case XtraRichEditStringId.MenuCmd_CutSelection: return "剪切";
                    default: return base.GetLocalizedString(id);
                }
            }
        }

        #endregion


    }
}
