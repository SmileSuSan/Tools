using DevExpress.LookAndFeel;
using DevExpress.Utils;
using DevExpress.XtraBars;
using DevExpress.XtraBars.Helpers;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraTab;
using K9;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace SC
{
    public partial class FrmMain : BaseFrm
    {
        #region 系统无操作自动关闭声明变量
        [StructLayout(LayoutKind.Sequential)]
        struct LASTINPUTINFO
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cbSize;
            [MarshalAs(UnmanagedType.U4)]
            public uint dwTime;
        }

        [DllImport("user32.dll")]
        static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        long GetLastInputTime()
        {
            LASTINPUTINFO vLastInputInfo = new LASTINPUTINFO();
            vLastInputInfo.cbSize = Marshal.SizeOf(vLastInputInfo);
            if (!GetLastInputInfo(ref vLastInputInfo)) return 0;
            return Convert.ToInt64(Environment.TickCount - (long)vLastInputInfo.dwTime);
        }

        bool flg = true;
        long timeNum = 0;
        #endregion

        public FrmMain()
        {
            InitializeComponent();
            SkinHelper.InitSkinGalleryDropDown(this.skinGallery);
        }

        #region 全局变量

        bool isRelogin = false; //注销标识
        /// <summary>
        /// 强制注销标识
        /// </summary>
        int isForcedOff = 0;
        /// <summary>
        /// 是否提示过注销信息标识
        /// </summary>
        bool isPromptMessage = false;

        private Thread thread = null;

        string noticeMes = string.Empty; //公告信息

        SQLiteBusiness sh = new SQLiteBusiness(); //本地数据库操作实例

        DataTable dtAuthority = new DataTable();//权限列表

        private SharpZip zip = new SharpZip(); //实例化解压类

        int nodeI = 0; //用于菜单搜索

        DataTable dtRecordTime = new DataTable();//消息
        DataTable dtNotice = new DataTable();//公告
        DataTable dtPro = new DataTable();//问题件

        public bool isMsgCount = false;//公告
        public bool isProMsgCount = false;//问题件
        private string localTimeByMsg = string.Empty;
        private string localTimeByProMsg = string.Empty;

        private delegate void CrossThreadOperationControl();//公告LOGN委托
        private delegate void CrossThreadOperationControlPro();//问题件LOGN委托

        FrmAllExcelExport frmExcel = new FrmAllExcelExport();

        #endregion


        /// <summary>
        /// 加载事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmMain_Load(object sender, EventArgs e)
        {
            ShowWait();
            dsMsg = new DataSet();
            #region 设置皮肤
            string skinName = SC.Properties.Settings.Default.UserSkinName;
            if (!string.IsNullOrWhiteSpace(skinName))
            {
                UserLookAndFeel.Default.SetSkinStyle(string.Format("{0}", skinName));
            }
            #endregion

            #region 开启电子称线程
            try
            {
                Utility.GlobalSetSerialPort();
            }
            catch (Exception ex)
            {
                LogHelper.Error("链接电子称错误", ex);
            }
            #endregion

            #region 初始加载设定

            //菜单加载控制方法
            InitMenu();

            //获取账户余额
            GetCurMoney();

            //设置底部状态栏显示信息
            StatusBarInfo();

            //显示时间
            SetServerTime();

            //OpenNoticeFrm();///查询公告

            //打开导航页
            OpenGuideFrm();
            #endregion

            CloseWait();

            #region 更新日志
            //string strMes = GetUpdateLog();
            //if (!string.IsNullOrWhiteSpace(strMes))
            //{
            //    FrmUpdateLog log = new FrmUpdateLog(strMes);
            //    log.ShowDialog();
            //}
            #endregion

            InitColumn();
            timer4.Start();

        }

        /// <summary>
        /// 初始化表格
        /// </summary>
        private void InitColumn()
        {
            if (dtRecordTime == null || dtRecordTime.Columns.Count <= 0)
            {
                dtRecordTime = new DataTable();
                dtRecordTime.Columns.Add("NOTICE_NAME");
                dtRecordTime.Columns.Add("NOTICE_DATETIME");
                if (sh.ValidateIsInt("TAB_NOTICE_LOCAL") <= 0)
                {
                    sh.AddNewTable(dtRecordTime, "TAB_NOTICE_LOCAL");
                }
            }
        }

        //登录默认打开未查看消息窗体
        private void OpenNoticeFrm()
        {
            //命名空间加窗体名称
            string fullNamespace = "K9.FrmNoticeNoSee";
            //程序集名称
            string strassembly = "SERVICE";
            //通过反射获取程序集
            Assembly assembly = this.LoadAssembly(strassembly);
            //判断窗体是否打开，如果已经打开则将窗体激活
            if (!ContainMDIChild(fullNamespace))
            {
                //通过反射得到窗体
                Object obj = assembly.CreateInstance(fullNamespace, true);
                if (obj != null)
                {
                    Form frm = obj as Form;
                    frm.Owner = this;
                    frm.ShowDialog();
                }
            }
        }


        /// <summary>
        /// 登录默认打开导航向导
        /// </summary>
        private void OpenGuideFrm()
        {
            if (dtAuthority == null || dtAuthority.Rows.Count == 0)
            {
                return;
            }
            DataRow[] aRow = dtAuthority.Select("A_NAME='首页导航'");
            if (aRow.Length > 0)
            {
                //命名空间加窗体名称
                string fullNamespace = "K9.FrmNavigation";
                //程序集名称
                string strassembly = "REPORT";
                //通过反射获取程序集
                Assembly assembly = this.LoadAssembly(strassembly);
                //判断窗体是否打开，如果已经打开则将窗体激活
                if (!ContainMDIChild(fullNamespace))
                {
                    //通过反射得到窗体
                    Object obj = assembly.CreateInstance(fullNamespace, true);
                    if (obj != null)
                    {
                        Form frm = obj as Form;
                        //frm.Text = frmName;
                        frm.MdiParent = this;
                        frm.ControlBox = false;
                        frm.WindowState = FormWindowState.Maximized;
                        //frm.FormClosed += new FormClosedEventHandler(MDIFromClose);
                        //frm.Load += new EventHandler(MDIFromLoad);
                        //FrmName = frm.Name;
                        //如需要特别处理则，在Base里面添加自己窗体特别属性的状态，然后在对应事件里面判断窗体名称来区别调用
                        SetMenuStatus(frm.Name, "HideAll");//菜单按钮状态
                        frm.Show();
                    }
                }
            }
        }

        /// <summary>
        /// 选择皮肤
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void skinGallery_GalleryItemClick(object sender, DevExpress.XtraBars.Ribbon.GalleryItemClickEventArgs e)
        {
            SC.Properties.Settings.Default.UserSkinName = e.Item.Tag.ToString();
            SC.Properties.Settings.Default.Save();
        }

        /// <summary>
        /// 初始化系统菜单方法.
        /// </summary>
        private void InitMenu()
        {
            try
            {

                string where = "";
                if (BaseInfoCommon.CurrentUser.EMPLOYEE_TYPE.Contains("承包区"))
                {
                    where = " AND A_DEFINE19 >=5 ";
                }
                else if (BaseInfoCommon.CurrentUser.EMPLOYEE_TYPE.Contains("业务员"))
                {
                    where = " AND A_DEFINE19 >=6 ";
                }
                else
                {
                    switch (BaseInfoCommon.CurrentUser.TYPE)
                    {
                        case "总部":
                            where = " AND A_DEFINE19 >=0 ";
                            break;
                        case "财务中心":
                            where = " AND A_DEFINE19 >=1 ";
                            break;
                        case "分拨中心":
                            where = " AND A_DEFINE19 >=2 ";
                            break;
                        case "一级网点":
                            where = " AND A_DEFINE19 >=3 ";
                            break;
                        case "二级网点":
                            where = " AND A_DEFINE19 >=4 ";
                            break;
                        default:
                            where = " AND A_DEFINE19 >=6 ";
                            break;
                    }
                }
                if (BaseInfoCommon.CurrentUser.EMPLOYEE_CODE == "ztdadmin" || BaseInfoCommon.CurrentUser.EMPLOYEE_CODE == "k9admin")
                {
                    dtAuthority = sh.GetSqliteData("T_BASE_AUTHORITY");
                }
                else
                {
                    dtAuthority = sh.GetSqliteData("T_BASE_AUTHORITY", where);
                }
                if (Utility.GetRowCount(dtAuthority) > 0)
                {

                    #region 分类菜单数据并排序
                    //Page菜单
                    DataRow[] drPageMenu = dtAuthority.Select("A_LEVEL=1", "A_ORDERNUM");
                    //Group菜单

                    DataRow[] drGroupMenu = dtAuthority.Select("A_LEVEL=2", "A_ORDERNUM");
                    //子菜单

                    DataRow[] drChildMenu = dtAuthority.Select("A_LEVEL=3", "A_ORDERNUM");
                    #endregion

                    #region 根据权限数据加载页面菜单
                    //创建Page                   
                    foreach (DataRow row in drPageMenu)
                    {
                        BarSubItem subItem = new BarSubItem();
                        subItem.Name = row["A_ID"].ToString();    //ID
                        subItem.Caption = row["A_NAME"].ToString();  //显示文本                      
                        this.bar2.AddItem(subItem);
                    }

                    for (int i = 0; i < this.bar2.Manager.Items.Count; i++)
                    {
                        foreach (DataRow row in drGroupMenu)
                        {
                            if ((this.bar2.Manager.Items[i] is BarSubItem) && this.bar2.Manager.Items[i].Name == row["A_PARAENTID"].ToString())
                            {
                                //首页不在菜单显示
                                if (row["A_NAME"].ToString() == "首页导航")
                                {
                                    continue;
                                }

                                DataRow[] arrTemp = dtAuthority.Select(" A_PARAENTID='" + row["A_ID"].ToString() + "'");
                                if (arrTemp.Length > 0)
                                {
                                    BarSubItem subItem = new BarSubItem();
                                    subItem.Name = row["A_ID"].ToString();    //ID
                                    subItem.Caption = row["A_NAME"].ToString();  //显示文本                               
                                    BarSubItem tempSubItem = (BarSubItem)this.bar2.Manager.Items[i];
                                    tempSubItem.AddItem(subItem).BeginGroup = row["A_DEFINE0"].ToString() == "1";
                                }
                                else
                                {
                                    BarButtonItem item = new BarButtonItem();
                                    item.Name = row["A_KEY"].ToString();        //Name
                                    item.Id = Convert.ToInt32(row["A_ID"]);     //ID
                                    item.Caption = row["A_NAME"].ToString();    //显示文本
                                    item.Tag = row["A_PATH"].ToString();
                                    BarSubItem tempSubItem = (BarSubItem)this.bar2.Manager.Items[i];
                                    tempSubItem.AddItem(item).BeginGroup = row["A_DEFINE0"].ToString() == "1";
                                }
                            }
                        }
                    }

                    for (int i = 0; i < this.bar2.Manager.Items.Count; i++)
                    {
                        foreach (DataRow row in drChildMenu)
                        {
                            if ((this.bar2.Manager.Items[i] is BarSubItem) && this.bar2.Manager.Items[i].Name == row["A_PARAENTID"].ToString())
                            {
                                BarButtonItem item = new BarButtonItem();
                                item.Name = row["A_KEY"].ToString();        //Name
                                item.Id = Convert.ToInt32(row["A_ID"]);     //ID
                                item.Caption = row["A_NAME"].ToString();    //显示文本
                                item.Tag = row["A_PATH"].ToString();
                                BarSubItem tempSubItem = (BarSubItem)this.bar2.Manager.Items[i];
                                tempSubItem.AddItem(item).BeginGroup = row["A_DEFINE0"].ToString() == "1";
                            }
                        }
                    }
                    #endregion
                }
            }

            catch (Exception ex)
            {
                LogHelper.Error("系统主界面窗体加载菜单事件报错：", ex);

            }
        }

        /// <summary>
        /// 获取账户余额
        /// </summary>
        private void GetCurMoney()
        {
            if (BaseInfoCommon.CurrentUser.EMPLOYEE_TYPE == "承包区")
            {
                Dictionary<string, string> dis = new Dictionary<string, string>();
                dis.Add("CENTER_NAME", BaseInfoCommon.CurrentUser.OWNER_SITE);
                dis.Add("SITE_NAME", BaseInfoCommon.CurrentUser.EMPLOYEE_NAME);
                string strJson = JsonConvertHelper.DicToTableJson(dis, "TAB_BALANCE_ACCOUNT_EMP_QRY");
                DataSet ds = HttpHelper.TransData(ServerName.K9, "qryTAB_BALANCE_ACCOUNT_EMP", strJson);
                if (Utility.IsTranOK(ds))
                {
                    if (Utility.GetRowCount(ds.Tables[0]) > 0)
                    {
                        this.barbtnAccount.Caption = "【承包区余额：" + ds.Tables[0].Rows[0]["CUR_MONEY"].ToString() + "】";
                    }
                }
            }
            else
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                if (BaseInfoCommon.CurrentUser.OWNER_SITE == "总部")
                {
                    return;
                }
                else
                {
                    dic.Add("CENTER_NAME", BaseInfoCommon.CurrentUser.SUPERIOR_FINANCE_CENTER);
                }

                dic.Add("SITE_NAME", BaseInfoCommon.CurrentUser.OWNER_SITE);
                dic.Add("BL_OPEN", "1");//字段启动
                dic.Add("qrySql", " START_DATE<=sysdate");
                string json = JsonConvertHelper.DicToTableJson(dic, "TAB_BALANCE_ACCOUNT_QRY");
                DataSet ds = HttpHelper.TransData(ServerName.K9, "qryTAB_BALANCE_ACCOUNT", json);


                Dictionary<string, string> diss = new Dictionary<string, string>();
                diss.Add("SQL", "SELECT A.CUR_MONEY FROM TAB_BALANCE_ACCOUNT_HK   A WHERE A.CENTER_NAME = '" + BaseInfoCommon.CurrentUser.SUPERIOR_FINANCE_CENTER + "' AND A.SITE_NAME = '" + BaseInfoCommon.CurrentUser.OWNER_SITE + "'");
                string sTRjjSON = JsonConvertHelper.DicToTableJson(diss, "BALANCE_QRY");
                DataSet dss = HttpHelper.TransData(ServerName.K9, "qryBALANCE_INTERFACE", sTRjjSON);

                if (Utility.IsTranOK(ds) && Utility.IsTranOK(dss))
                {
                    if (Utility.GetRowCount(ds.Tables[0]) > 0 && Utility.GetRowCount(dss.Tables[0]) > 0)
                    {
                        this.barbtnAccount.Caption = "【预付款余额：" + ds.Tables[0].Rows[0]["CUR_MONEY"].ToString() + "" + "    货款余额：" + dss.Tables[0].Rows[0]["CUR_MONEY"].ToString() + "】";
                    }
                }
            }
        }

        /// <summary>
        /// 设置登录信息
        /// </summary>
        private void StatusBarInfo()
        {
            string ipInfo = HttpHelper.ipAddress;
            if (!ipInfo.Contains(".cn"))
            {
                string[] ips = ipInfo.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                if (ips.Length > 0)
                {
                    ipInfo = "连接地址：" + ips[0] + ".*.*." + ips[ips.Length - 1];
                }
            }
            this.barUserSite.Caption = string.Format("登录人：{0}    网点：{1}    所属财务中心：{2}", BaseInfoCommon.CurrentUser.EMPLOYEE_NAME, BaseInfoCommon.CurrentUser.OWNER_SITE, BaseInfoCommon.CurrentUser.SUPERIOR_FINANCE_CENTER);
            this.barIP.Caption = ipInfo;
        }

        /// <summary>
        /// 设置左下角显示时间
        /// </summary>
        private void SetServerTime()
        {
            this.barDateTime.Caption = "系统时间：" + BaseInfoCommon.CurrentUser.DTSERVERDATE.ToString("yyyy年MM月dd日 HH时mm分ss秒");
            thread = new Thread(new ThreadStart(TimeMethod));
            thread.IsBackground = true;
            thread.Start();
            this.timer1.Interval = 1000;
            this.timer1.Start();
        }

        /// <summary>
        /// 间隔一秒
        /// </summary>
        void TimeMethod()
        {
            while (true)
            {
                this.barDateTime.Caption = "系统时间：" + BaseInfoCommon.CurrentUser.DTSERVERDATE.ToString("yyyy年MM月dd日 HH时mm分ss秒");
                Thread.CurrentThread.Join(1000);//阻止设定时间1秒
            }
        }

        /// <summary>
        /// 打开窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barManager1_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if ("BarButtonItem".Equals(e.Item.GetType().Name))
                {
                    if (e.Item.Tag == null)
                    {
                        return;
                    }
                    //命名空间加窗体名称
                    string fullNamespace = e.Item.Name;
                    //程序集名称
                    string strassembly = e.Item.Tag.ToString();
                    //通过反射获取程序集
                    Assembly assembly = this.LoadAssembly(strassembly);
                    if (assembly == null)
                    {
                        XtraMessageBox.Show("系统文件缺失或被破坏,请检查升级！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                    Istool = true;
                    //判断窗体是否打开，如果已经打开则将窗体激活
                    if (!ContainMDIChild(fullNamespace) && fullNamespace != "GROUP")
                    {
                        ShowWait();
                        //通过反射得到窗体
                        Object obj = assembly.CreateInstance(fullNamespace, true);
                        if (obj != null)
                        {
                            Form frm = obj as Form;
                            frm.Text = e.Item.Caption;
                            frm.MdiParent = this;
                            frm.ControlBox = false;
                            frm.WindowState = FormWindowState.Maximized;
                            frm.FormClosed += new FormClosedEventHandler(MDIFromClose);
                            frm.Load += new EventHandler(MDIFromLoad);
                            FrmName = frm.Name;
                            SetMenuStatus(frm.Name, MenuStatus.Init.ToString());//菜单按钮状态
                            frm.Show();
                        }
                        else
                        {
                            LogHelper.Error(string.Format("PC端主页面菜单点击事件-反射窗体{0}错误日志记录:", fullNamespace));
                            XtraMessageBox.Show("系统文件缺失或被破坏,请检查升级！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        foreach (Form frm in this.MdiChildren)
                        {
                            if (frm.GetType().ToString() == fullNamespace)
                            {
                                //如需要特别处理则，在Base里面添加自己窗体特别属性的状态，然后在对应事件里面判断窗体名称来区别调用
                                SetMenuStatus(frm.Name, MenuStatus.Activate.ToString());//菜单按钮状态
                                frm.Activate();
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                LogHelper.Error("系统主界面窗体点击事件报错：", ex);
            }
        }

        /// <summary>
        /// 加载程序集方法.
        /// </summary>
        /// <param name="assemblyName">程序集名称.</param>
        /// <returns>程序集实体.</returns>
        private Assembly LoadAssembly(string assemblyName)
        {
            Assembly assembly = null;
            try
            {
                assembly = Assembly.Load(assemblyName);

            }
            catch (Exception ex)
            {
                LogHelper.Error(string.Format("加载程序集:{0}错误日志记录:", assemblyName), ex);
                // XtraMessageBox.Show("系统文件缺失或被破坏,请检查升级！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //ShowMessage("系统文件缺失或被破坏,请检查升级！", MessageType.OkAndTips);
            }

            return assembly;
        }

        /// <summary>
        /// 检查子窗体是否激活存在.
        /// </summary>
        /// <param name="ChildTypeString">窗体名称.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        internal bool ContainMDIChild(string ChildTypeString)
        {
            Form myMDIChild = null;
            foreach (Form f in this.MdiChildren)
            {
                if (f.GetType().ToString().ToUpper() == ChildTypeString.ToUpper())
                {
                    myMDIChild = f;
                    break;
                }
            }

            if (myMDIChild != null)
            {
                return true;
            }
            else
                return false;
        }

        #region 子窗体输出Grid字段列文本

        /// <summary>
        /// 保存Grid字段方法
        /// </summary>
        /// <param name="frm">The FRM.</param>
        private void SaveGridColumn(Form frm)
        {

            string FileName = string.Empty;
            //存储目录不存在则创建
            string ColumnFile = AppDomain.CurrentDomain.BaseDirectory.ToString() + @"GridColumn";

            if (!DirFile.IsExistDirectory(ColumnFile))
            {
                DirFile.CreateDirectory(ColumnFile);
            }
            try
            {
                List<GridControl> list = GetGridConrol(frm.Controls);
                if (list.Count > 0)
                {
                    foreach (GridControl ctl in list)
                    {
                        if (ctl != null)
                        {
                            FileName = ColumnFile + "\\" + frm.Name + "-" + ctl.Name + ".xml";
                            OptionsLayoutBase olb = new OptionsLayoutBase();
                            ctl.DefaultView.SaveLayoutToXml(FileName, olb);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("窗体报错Grid字段出错：", ex);
            }
        }

        /// <summary>
        /// 递归循环取到grid
        /// </summary>
        /// <param name="contrls"></param>
        /// <returns></returns>
        private List<GridControl> GetGridConrol(System.Windows.Forms.Control.ControlCollection contrls)
        {
            List<GridControl> list = new List<GridControl>();
            foreach (Control ctl in contrls)
            {
                if (ctl is GroupControl || ctl is PanelControl || ctl is XtraTabControl || ctl is XtraScrollableControl || ctl is XtraTabPage)
                {
                    foreach (GridControl con in GetGridConrol(ctl.Controls))
                    {
                        list.Add(con);
                    }
                }
                if (ctl.ProductName.Equals("DevExpress.XtraGrid"))
                {
                    if (ctl.Width > 500)
                    {
                        list.Add((DevExpress.XtraGrid.GridControl)ctl);
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// 子窗体关闭事件.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="FormClosedEventArgs"/> instance containing the event data.</param>
        private void MDIFromClose(object sender, FormClosedEventArgs e)
        {
            Istool = true;
            SaveGridColumn(((Form)sender));
        }

        /// <summary>
        /// 子窗体加载事件.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MDIFromLoad(object sender, EventArgs e)
        {
            LoadColumn(((Form)sender));
            CloseWait();
        }

        /// <summary>
        /// 加载列名顺序以及宽度
        /// </summary>
        /// <param name="FileName">模块名称</param>
        /// <param name="frmName">窗体名称</param>
        /// <param name="gridView">GridView</param>
        public void LoadColumn(Form frm)
        {
            string Gpath = string.Empty;
            //存储目录不存在则创建
            string ColumnFile = AppDomain.CurrentDomain.BaseDirectory.ToString() + @"GridColumn";

            try
            {
                List<GridControl> list = GetGridConrol(frm.Controls);
                if (list.Count > 0)
                {
                    foreach (GridControl gc in list)
                    {
                        if (gc != null)
                        {
                            if (gc.Views.Count > 0)
                            {

                                Gpath = ColumnFile + "\\" + frm.Name + "-" + gc.Name + ".xml";

                                gc.Views[0].RestoreLayoutFromXml(Gpath);

                                continue;
                            }

                            Gpath = ColumnFile + "\\" + frm.Name + "-" + gc.Name + ".xml";

                            //判断文件是否存在
                            if (File.Exists(Gpath))
                            {
                                XmlDocument objXmlDoc = new XmlDocument();

                                objXmlDoc.Load(Gpath);

                                string tNodeCaption;

                                XmlNodeList node = objXmlDoc.SelectSingleNode("//property[@name='Columns']").ChildNodes;

                                DataTable dt = new DataTable();
                                dt.Columns.Add("VisibleIndex");
                                dt.Columns.Add("Width");
                                dt.Columns.Add("Name");


                                foreach (XmlNode nodeChiled in node)
                                {
                                    DataRow dr = dt.NewRow();
                                    foreach (XmlNode nodeChileds in nodeChiled.ChildNodes)
                                    {
                                        tNodeCaption = nodeChileds.Attributes["name"].Value;
                                        switch (tNodeCaption)
                                        {
                                            case "VisibleIndex":
                                                dr["VisibleIndex"] = nodeChileds.InnerText;
                                                break;
                                            case "Width":
                                                dr["Width"] = nodeChileds.InnerText;
                                                break;
                                            case "Name":
                                                dr["Name"] = nodeChileds.InnerText;
                                                break;
                                        }

                                    }
                                    dt.Rows.Add(dr);
                                }

                                if (gc.Views.Count > 0)
                                {
                                    GridView gridView = (GridView)gc.Views[0];

                                    foreach (DataRow row in dt.Rows)
                                    {
                                        if (gridView.Columns[row["Name"].ToString()] != null && gridView.Columns[row["Name"].ToString()].VisibleIndex >= 0)
                                        {
                                            if (!string.IsNullOrWhiteSpace(row["VisibleIndex"].ToString()))
                                            {
                                                gridView.Columns[row["Name"].ToString()].VisibleIndex = Convert.ToInt32(row["VisibleIndex"]);      //设置列名下标
                                                if (!string.IsNullOrWhiteSpace(row["Width"].ToString()))
                                                {
                                                    gridView.Columns[row["Name"].ToString()].Width = Convert.ToInt32(row["Width"]);    //设置列名宽度
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("窗体报错Grid字段出错：", ex);
            }
        }
        #endregion

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barUpdatePwd_ItemClick(object sender, ItemClickEventArgs e)
        {
            FrmMainUpdatePwd updatePwd = new FrmMainUpdatePwd();
            updatePwd.Owner = this;
            updatePwd.ShowDialog();
            if (!updatePwd.isSuccess)
            {
                return;
            }
        }

        /// <summary>
        /// 注销
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barLoginOut_ItemClick(object sender, ItemClickEventArgs e)
        {
            LoginOutByWho(0);
        }

        /// <summary>
        /// 注销登出
        /// </summary>
        /// <param name="isType">0:本机操作\1：系统检测\2总部强制</param>
        private void LoginOutByWho(int isType)
        {
            isForcedOff = isType;
            isRelogin = true;
            this.Close();
        }

        /// <summary>
        /// 退出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barClose_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 主窗体关闭中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (isRelogin)
            {
                if (isForcedOff > 0)
                {
                    if (isForcedOff > 1)
                    {
                        //弹窗提示
                        XtraMessageBox.Show("检查到您的系统未更新，已由总部IT强制下线，请重新登陆系统！", "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;//注销 重新登录
                    }
                    else
                    {
                        isPromptMessage = true;
                        //弹窗确认
                        if (XtraMessageBox.Show("网络已断开，请检查网络，是否重新登录？", "系统提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                        {
                            return;//注销 重新登录
                        }
                    }
                    isForcedOff = 0;
                }
                else
                {
                    if (timeNum == 1800)
                    {
                        timeNum = 0;
                        //FrmLock lk = new FrmLock();
                        //this.Enabled = false;
                        //timer2.Enabled = false;
                        //lk.ShowDialog();
                        this.Enabled = true;
                        timer2.Enabled = true;
                        //取消退出操作
                        e.Cancel = true;
                        return;
                    }
                    //弹窗确认
                    if (XtraMessageBox.Show("确定要注销当前用户？", "系统提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                    {
                        return;//注销 重新登录
                    }
                }
                isRelogin = false;
                //取消退出操作
                e.Cancel = true;
            }
            else
            {
                //弹窗确认
                DialogResult result = XtraMessageBox.Show("确定退出？", "系统提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (result == DialogResult.OK)
                {
                    //防止MDI子窗体关闭事件的处理 影响到父窗体,主动设置Cancel为false
                    e.Cancel = false;
                }
                else
                {
                    //取消退出操作
                    e.Cancel = true;
                }
            }
        }

        /// <summary>
        /// 主窗体关闭
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FrmMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            Dictionary<string, string> doc = new Dictionary<string, string>();
            //添加登录日志
            doc.Add("GUID", Utility.LoginID);
            doc.Add("CLOSE_TIME", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            string jsons = JsonConvertHelper.DicToTableJson(doc, "T_BASE_LOGIN_LOG_UPT");
            HttpHelper.TransData(ServerName.K9, "saveT_BASE_LOGIN_LOG", jsons);

            CloseWait();
            Clean();//清理资源
            if (isRelogin)
            {
                //重新登录
                ReLogin();
            }
            else
            {
                //退出程序, 关闭程序主入口窗体(login)
                FrmLogin loginFrm = GetLoginForm();
                loginFrm.Close();
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        private void Clean()
        {
            timer1.Stop();

        }

        /// <summary>
        /// 显示重新登录窗体
        /// </summary>
        private void ReLogin()
        {
            FrmLogin login = GetLoginForm();
            login.ClearPwd();
            login.Show();
            login.Activate();
        }

        /// <summary>
        /// 获取程序中的登录窗体
        /// </summary>
        /// <returns></returns>
        private FrmLogin GetLoginForm()
        {
            FrmLogin loginFrm = null;
            if (Application.OpenForms["FrmLogin"] != null)
            {
                loginFrm = (FrmLogin)Application.OpenForms["FrmLogin"];
            }
            else
            {
                loginFrm = new FrmLogin();
            }
            return loginFrm;
        }

        #region 窗体按钮菜单控制方法

        /// <summary>
        /// 控制窗体按钮菜单
        /// </summary>
        /// <param name="dr">The dr.</param>
        /// <param name="drAUTHORITYREL">The dr authorityrel.</param>
        public void ShowBtnVisible(DataRow dr)
        {
            if (dr["A_DEFINE1"] == null || "0".Equals(dr["A_DEFINE1"].ToString())) //新增
            {
                this.tsbtnAdd.Enabled = false;
            }
            if (dr["A_DEFINE2"] == null || "0".Equals(dr["A_DEFINE2"].ToString())) //修改
            {
                this.tsbtnUpdate.Enabled = false;
            }
            if (dr["A_DEFINE3"] == null || "0".Equals(dr["A_DEFINE3"].ToString())) //删除
            {
                this.tsbtnDelete.Enabled = false;
            }
            if (dr["A_DEFINE4"] == null || "0".Equals(dr["A_DEFINE4"].ToString())) //查询
            {
                this.tsbtnSerach.Enabled = false;
            }
            if (dr["A_DEFINE15"] == null || "0".Equals(dr["A_DEFINE15"].ToString())) //取消
            {
                this.tsbRevocation.Enabled = false;
            }
            if (dr["A_DEFINE11"] == null || "0".Equals(dr["A_DEFINE11"].ToString())) //保存
            {
                this.tsbtnSave.Enabled = false;
            }
            if (dr["A_DEFINE5"] != null && "1".Equals(dr["A_DEFINE5"].ToString())) //导入
            {
                this.tsbtnImport.Enabled = true;
            }
            if (dr["A_DEFINE6"] != null && "1".Equals(dr["A_DEFINE6"].ToString())) //导出
            {
                this.tsbtnExport.Enabled = true;
            }
            if (dr["A_DEFINE7"] != null && "1".Equals(dr["A_DEFINE7"].ToString())) //打印
            {
                this.tsbtnPrint.Enabled = true;
            }
            if (dr["A_DEFINE16"] != null && "1".Equals(dr["A_DEFINE16"].ToString())) //关闭
            {
                this.tsbClose.Enabled = true;
            }
        }

        /// <summary>
        /// 根据菜单状态判断按钮实体变化事件.
        /// </summary>
        /// <param name="Status">The status.</param>
        /// <param name="OldMenuStatus">The old menu status.</param>
        /// <returns>MainMenuMode.</returns>
        private MainMenuMode CheckMenuStatus(string Status, MainMenuMode OldMenuStatus)
        {

            MainMenuMode mb = new MainMenuMode();
            if (Istool == false)
            {
                return null;
            }

            //点击新增后按钮菜单状态
            if (MenuStatus.Add.ToString().Equals(Status))
            {
                mb.Add = false;
                mb.Delete = false;
                mb.Modify = false;
                mb.Search = true;
                mb.Save = true;
                mb.Cancel = true;
            }
            else if (MenuStatus.All.ToString().Equals(Status)) //显示所有按钮
            {
                mb.Add = true;
                mb.Delete = true;
                mb.Modify = true;
                mb.Search = true;
                mb.Save = true;
                mb.Cancel = true;

            }
            else if ("HideAll".Equals(Status)) //隐藏所有按钮
            {
                mb.Add = false;
                mb.Delete = false;
                mb.Modify = false;
                mb.Search = false;
                mb.Save = false;
                mb.Cancel = false;

            }
            else if (MenuStatus.Modify.ToString().Equals(Status)) //点击编辑后按钮菜单状态
            {
                mb.Add = false;
                mb.Delete = false;
                mb.Modify = false;
                mb.Search = true;
                mb.Save = true;
                mb.Cancel = true;

            }
            else if (MenuStatus.Delete.ToString().Equals(Status)) //点击删除后按钮菜单状态
            {
                mb.Add = false;
                mb.Delete = true;
                mb.Modify = false;
                mb.Search = true;
                mb.Save = true;
                mb.Cancel = true;

            }
            else if (MenuStatus.Save.ToString().Equals(Status)) //点击保存后按钮菜单状态
            {
                mb.Add = true;
                mb.Delete = true;
                mb.Modify = true;
                mb.Search = true;
                mb.Save = true;
                mb.Cancel = false;

            }
            else if (MenuStatus.Cancel.ToString().Equals(Status)) //点击取消后按钮菜单状态
            {
                mb.Add = true;
                mb.Delete = true;
                mb.Modify = true;
                mb.Search = true;
                mb.Save = true;
                mb.Cancel = false;

            }
            else if (MenuStatus.Search.ToString().Equals(Status)) //点击查询后按钮菜单状态
            {
                mb.Add = true;
                mb.Delete = true;
                mb.Modify = true;
                mb.Search = true;
                mb.Save = true;
                mb.Cancel = false;

            }
            else if (MenuStatus.Init.ToString().Equals(Status))//打开窗体初始化状态
            {

                mb.Add = true;
                mb.Delete = true;
                mb.Modify = true;
                mb.Search = true;
                mb.Save = true;
                mb.Cancel = false;

            }
            else if ((MenuStatus.Activate.ToString().Equals(Status)))//窗体激活状态
            {
                if (OldMenuStatus != null)
                {
                    mb.Add = OldMenuStatus.Add;
                    mb.Delete = OldMenuStatus.Delete;
                    mb.Modify = OldMenuStatus.Modify;
                    mb.Search = OldMenuStatus.Search;
                    mb.Save = OldMenuStatus.Save;
                    mb.Cancel = OldMenuStatus.Cancel;


                }
                else
                {
                    mb.Add = true;
                    mb.Delete = true;
                    mb.Modify = true;
                    mb.Search = true;
                    mb.Save = true;
                    mb.Cancel = false;
                }

            }
            else
            {
                return null;
            }
            return mb;
        }

        /// <summary>
        /// 设置主窗体菜单状态
        /// </summary>
        /// <param name="Status">需要设置菜单状态</param>
        public override void SetMenuStatus(string FrmName, string Status)
        {
            if (MenuMode.MainMenu.ContainsKey(FrmName))//如果存在状态数据
            {
                if (CheckMenuStatus(Status, MenuMode.MainMenu[FrmName]) == null)
                {
                    return;
                }
                else
                {
                    MenuMode.MainMenu[FrmName] = CheckMenuStatus(Status, MenuMode.MainMenu[FrmName]);
                }
            }
            else//不存在
            {
                MenuMode.MainMenu.Add(FrmName, CheckMenuStatus(Status, null));
            }

            MainMenuMode mb = MenuMode.MainMenu[FrmName];
            this.tsbtnAdd.Enabled = mb.Add;
            this.tsbtnUpdate.Enabled = mb.Modify;
            this.tsbtnSerach.Enabled = mb.Search;
            this.tsbtnDelete.Enabled = mb.Delete;
            this.tsbtnSave.Enabled = mb.Save;
            this.tsbRevocation.Enabled = mb.Cancel;

            Form frm = this.ActiveMdiChild;

            if (frm != null)
            {
                //获取MDI子窗体名称
                string frmFullName = string.Format("{0}.{1}", frm.GetType().Namespace, frm.Name);

                if (this.dtAuthority != null && this.dtAuthority.Rows.Count > 0)
                {
                    DataRow[] rows = this.dtAuthority.Select(string.Format("A_KEY='{0}'", frmFullName));
                    if (rows != null && rows.Length > 0)
                    {
                        ShowBtnVisible(rows[0]);
                    }
                }
            }
        }

        /// <summary>
        /// 子窗体激活事件.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void FrmMainNew_MdiChildActivate(object sender, EventArgs e)
        {
            try
            {
                Istool = true;
                //获取当前激活的MDI子窗体
                Form frm = this.ActiveMdiChild;

                //先将所有按钮禁用
                bar1.Manager.Items["tsbtnAdd"].Enabled = false;
                bar1.Manager.Items["tsbtnUpdate"].Enabled = false;
                bar1.Manager.Items["tsbtnDelete"].Enabled = false;
                bar1.Manager.Items["tsbtnSerach"].Enabled = false;
                bar1.Manager.Items["tsbtnSave"].Enabled = false;
                bar1.Manager.Items["tsbtnImport"].Enabled = false;
                bar1.Manager.Items["tsbtnExport"].Enabled = false;
                bar1.Manager.Items["tsbtnPrint"].Enabled = false;
                bar1.Manager.Items["tsbRevocation"].Enabled = false;
                bar1.Manager.Items["tsbClose"].Enabled = false;

                if (frm == null)
                {
                    return;
                }

                if (string.IsNullOrWhiteSpace(FrmName) || !FrmName.Contains(frm.Name))
                {
                    FrmName = frm.Name;
                }

                //如需要特别处理则，在Base里面添加自己窗体特别属性的状态，然后在对应事件里面判断窗体名称来区别调用
                SetMenuStatus(FrmName, MenuStatus.Activate.ToString());//更新菜单状态
            }
            catch (Exception ex)
            {
                LogHelper.Error("系统主界面激活子窗体事件报错：", ex);
            }
        }

        /// <summary>
        /// 菜单按钮触发主窗体对应方法
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ToolStripItemClickedEventArgs"/> instance containing the event data.</param>
        private void ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                Istool = true;
                if (this.ActiveMdiChild != null)
                {
                    switch (e.Item.Name)
                    {
                        case "tsbtnAdd":
                            ((BaseFrm)this.ActiveMdiChild).Add();
                            SetMenuStatus(FrmName, MenuStatus.Add.ToString());
                            break;
                        case "tsbtnUpdate":
                            ((BaseFrm)this.ActiveMdiChild).Modify();
                            SetMenuStatus(FrmName, MenuStatus.Modify.ToString());
                            break;
                        case "tsbtnDelete":
                            ((BaseFrm)this.ActiveMdiChild).Delete();
                            SetMenuStatus(FrmName, MenuStatus.Delete.ToString());
                            break;
                        case "tsbtnSerach":
                            ((BaseFrm)this.ActiveMdiChild).Serach();
                            SetMenuStatus(FrmName, MenuStatus.Search.ToString());
                            break;
                        case "tsbtnSave":
                            ((BaseFrm)this.ActiveMdiChild).Save();
                            SetMenuStatus(FrmName, MenuStatus.Save.ToString());
                            break;
                        case "tsbtnImport":
                            ((BaseFrm)this.ActiveMdiChild).Import();
                            break;
                        case "tsbtnExport":
                            ((BaseFrm)this.ActiveMdiChild).Export();
                            break;
                        case "tsbtnPrint":
                            ((BaseFrm)this.ActiveMdiChild).Print();
                            break;
                        case "tsbRevocation":
                            ((BaseFrm)this.ActiveMdiChild).Revocation();
                            SetMenuStatus(FrmName, MenuStatus.Cancel.ToString());
                            break;
                        case "tsbClose":
                            ((BaseFrm)this.ActiveMdiChild).threadAbort();
                            ((BaseFrm)this.ActiveMdiChild).Exit();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error("执行MDI子窗体按钮操作出错：", ex);
                XtraMessageBox.Show(ex.Message, "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 打开计算器
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsbCalc_ItemClick(object sender, ItemClickEventArgs e)
        {
            System.Diagnostics.Process.Start("calc.exe");
        }
        #endregion

        /// <summary>
        /// 获取预付款余额
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barbtnAccount_ItemClick(object sender, ItemClickEventArgs e)
        {
            GetCurMoney();
        }

        /// <summary>
        /// 导出Excel
        /// </summary>
        /// <param name="ds"></param>
        public override void ExportExcel(DataSet ds)
        {
            frmExcel.Show();
            frmExcel.ExportAll(ds);
            frmExcel.Activate();
        }

        /// <summary>
        /// 获取更新日志
        /// </summary>
        /// <returns></returns>
        public string GetUpdateLog()
        {
            SQLiteBusiness sh1 = new SQLiteBusiness(string.Format(@"Data Source={0}SysInfo.db", AppDomain.CurrentDomain.BaseDirectory.ToString()));
            DataTable logDt = sh1.GetSqliteData("UPDATETABLE");
            sh1.DelTable("UPDATETABLE");
            if (logDt != null && logDt.Rows.Count > 0)
            {
                string mes = "";
                foreach (DataRow row in logDt.Rows)
                {
                    if (!string.IsNullOrWhiteSpace(row["UPDATE_LOG"].ToString()))
                    {
                        mes += row["UPDATE_LOG"].ToString() + "\r\n\r\n";
                    }
                }
                return mes;
            }
            return "";
        }

        private DateTime m_LastClick = System.DateTime.Now;

        private void xtraTabbedMdiManager1_MouseDown(object sender, MouseEventArgs e)
        {
            //if (e.Button == MouseButtons.Left)
            //{
            //    DateTime dt = DateTime.Now;
            //    TimeSpan span = dt.Subtract(m_LastClick);
            //    if (span.TotalMilliseconds < 300)  //如果两次点击的时间间隔小于300毫秒，则认为是双击
            //    {
            //        if (this.MdiChildren.Length > 0)
            //        {
            //            MessageBox.Show("a");
            //        }
            //        m_LastClick = dt.AddMinutes(-1);
            //    }
            //    else
            //        m_LastClick = dt;
            //}
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            BaseInfoCommon.CurrentUser.DTSERVERDATE = BaseInfoCommon.CurrentUser.DTSERVERDATE.AddSeconds(1);
        }

        DataSet dsMsg = new DataSet();
        bool isMsg = false;
        private void timer4_Tick(object sender, EventArgs e)
        {
            dsMsg.AcceptChanges();
            isMsg = false;
            timer4.Interval = 180000;
            if (!BaseInfoCommon.CurrentUser.TYPE.Contains("中心"))
            {
                GetMessageExcBoxs();
                GetNotice();
                //GetLogOutNotice();
            }
            else
            {
                GetNotice();
                //GetLogOutNotice();
            }
        }

        private string GetOldTime(string noticeName)
        {
            DataTable dt = sh.GetSqliteData("TAB_NOTICE_LOCAL", "AND NOTICE_NAME ='" + noticeName + "'");
            if (dt != null && Utility.GetRowCount(dt) > 0)
            {
                DateTime time = new DateTime();
                if (DateTime.TryParse(dt.Rows[0]["NOTICE_DATETIME"].ToString(), out time))
                {
                    return time.ToString("yyyy-MM-dd HH:mm:ss");
                }
            }
            return string.Empty;
        }

        private string localTimeByExce = string.Empty;
        //投诉
        public void GetMessageExcBoxs()
        {
            try
            {
                string localTime = GetOldTime("投诉");
                localTimeByExce = localTime.Equals("") ? DateTime.Now.AddDays(-3).ToString("yyyy-MM-dd HH:mm:ss") : localTime;

                DataTable dt_Souser = new DataTable();
                dt_Souser.Columns.Add("EXCEPTIONID");
                dt_Souser.Columns.Add("EXCEPTIONSITE");
                dt_Souser.Columns.Add("BILL_CODE");
                dt_Souser.Columns.Add("EXCEPTIONSITE_SIDE");
                dt_Souser.Columns.Add("QUESTION");
                dt_Souser.Columns.Add("REGISTER_DATE");
                dt_Souser.Columns.Add("BIAO");
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(@" SELECT  T.BILL_CODE,T.QUESTION,T.REGISTER_DATE,T.EXCEPTIONID,T.EXCEPTIONSITE,T.EXCEPTIONSITE_SIDE
                                   FROM TAB_EXCEPTION T WHERE T.EXCEPTION_DATE>=TO_DATE('{0}','yyyy-mm-dd hh24:mi:ss') AND T.EXCEPTIONSITE_SIDE = '{1}'
                   ", localTimeByExce, BaseInfoCommon.CurrentUser.OWNER_SITE);
                sb.AppendFormat(@" AND NOT EXISTS (SELECT /*+INDEX(S,TAB_NOTICE_SEE_P)*/1
                                          FROM TAB_NOTICE_SEE S
                                          WHERE S.GUID = T.EXCEPTIONID
                                          AND S.SITE_NAME = '{0}'
                                          AND S.EMPLOYEE_NAME= '{1}')", BaseInfoCommon.CurrentUser.OWNER_SITE, BaseInfoCommon.CurrentUser.EMPLOYEE_NAME);
                Dictionary<string, string> dic = new Dictionary<string, string>();
                dic.Add("SQL", sb.ToString());
                string strJson = JsonConvertHelper.DicToTableJson(dic, "BILL_QRY");
                DataSet ds = HttpHelper.TransData(ServerName.K9, "qryBILL_INTERFACE", strJson);
                if (Utility.IsTranOK(ds) && Utility.GetRowCount(ds.Tables[0]) > 0)
                {
                    DataTable dt_1 = ds.Tables[0].Copy();
                    dt_1.Columns.Add("BIAO");
                    foreach (DataRow dr in dt_1.Rows)
                    {
                        dr["BIAO"] = "3";
                        dt_Souser.ImportRow(dr);
                    }
                }
                if (dt_Souser != null && Utility.GetRowCount(dt_Souser) > 0)
                {
                    dt_Souser.TableName = "投诉";
                    //GetMessageExcBoxsTang(dt_Souser);

                    if (dsMsg.Tables.Contains("投诉"))
                    {
                        dsMsg.Tables["投诉"].Merge(dt_Souser);
                    }
                    else
                    {
                        dsMsg.Tables.Add(dt_Souser.Copy());
                    }
                    isMsg = true;
                }

                AddLocalData("投诉", dtRecordTime);
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message);
                XtraMessageBox.Show(ex.Message, "系统提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 获取当前是否有公告
        /// </summary>
        private void GetNotice()
        {
            DataTable dt_SouserOne = new DataTable();
            dt_SouserOne.Columns.Add("NOTICE_DATETIME");
            dt_SouserOne.Columns.Add("TITLE");
            dt_SouserOne.Columns.Add("BL_MUST_SEE");
            dt_SouserOne.Columns.Add("GUID");
            dt_SouserOne.Columns.Add("NOTICE_NAME");
            dt_SouserOne.Columns.Add("SEND_SITE");
            dt_SouserOne.Columns.Add("BL_MUST_REPLY");
            dt_SouserOne.Columns.Add("BIAO");
            string localTime = GetOldTime("公告");
            localTimeByMsg = localTime.Equals("") ? DateTime.Now.AddDays(-3).ToString("yyyy-MM-dd HH:mm:ss") : localTime;
            Dictionary<string, string> dic = new Dictionary<string, string>();
            StringBuilder sql = new StringBuilder();
            sql.Append(@"SELECT /*+INDEX(T,TAB_NOTICE_I3)*/
                   T.NOTICE_DATETIME,
                   T.TITLE,
                   T.REC_SITE,
                   T.CONTENT,
                   T.SEND_SITE,
                   T.GUID,
                   T.PERSON,
                   T.ALLOW_RESTORE,
                   T.CONTENT1,
                   T.CONTENT2,
                   T.BL_MUST_SEE,
                   T.BL_MUST_REPLY,
                   T.SPECIFY_PERSON,
                   T.FILE_PATH,
                   (CASE WHEN T.FILE_PATH IS NULL THEN 0 ELSE 1 END) AS BL_FILE,
                   T.NOTICE_NAME,
                   T.FILE_NAME
                   FROM TAB_NOTICE T WHERE 1=1 ");
            sql.AppendFormat(@" AND T.NOTICE_DATETIME >=to_date('{0}','yyyy-mm-dd hh24:mi:ss')
               ", localTimeByMsg);
            //全体网点、指定网点、上属网点查询
            sql.AppendFormat(" AND (T.REC_SITE='全体' OR T.REC_SITE LIKE '%;{0};%' OR T.REC_SITE LIKE '%;{1};%') ", BaseInfoCommon.CurrentUser.OWNER_SITE, BaseInfoCommon.CurrentUser.SUPERIOR_SITE);
            sql.AppendFormat(" AND T.NOTICE_NAME='公告' ");

            sql.AppendFormat(@" AND NOT EXISTS (SELECT /*+INDEX(S,TAB_NOTICE_SEE_P)*/1
                                          FROM TAB_NOTICE_SEE S
                                          WHERE S.GUID = T.GUID
                                          AND S.SITE_NAME = '{0}'
                                          AND S.EMPLOYEE_NAME= '{1}')", BaseInfoCommon.CurrentUser.OWNER_SITE, BaseInfoCommon.CurrentUser.EMPLOYEE_NAME);
            dic.Add("SQL", sql.ToString());
            string strJson = JsonConvertHelper.DicToTableJson(dic, "BILL_QRY");
            DataSet ds = HttpHelper.TransData(ServerName.K9, "qryBILL_INTERFACE", strJson);
            if (Utility.IsTranOK(ds))
            {
                if (Utility.GetRowCount(ds.Tables[0]) > 0)
                {
                    DataTable dt_1 = ds.Tables[0].Copy();
                    dt_1.Columns.Add("BIAO");
                    foreach (DataRow dr in dt_1.Rows)
                    {
                        dr["BIAO"] = "4";
                        dt_SouserOne.ImportRow(dr);
                    }
                    if (dt_SouserOne != null && Utility.GetRowCount(dt_SouserOne) > 0)
                    {
                        dt_SouserOne.TableName = "公告";
                        //GetMessageBoxsNotion(dt_SouserOne);

                        if (dsMsg.Tables.Contains("公告"))
                        {
                            dsMsg.Tables["公告"].Merge(dt_SouserOne);
                        }
                        else
                        {
                            dsMsg.Tables.Add(dt_SouserOne.Copy());
                        }
                        isMsg = true;
                    }
                }

                AddLocalData("公告", dtRecordTime);
            }
        }

        public void AddLocalData(string noticeName, DataTable dt)
        {
            int i = sh.DelTableData("TAB_NOTICE_LOCAL", "AND NOTICE_NAME='" + noticeName + "'");
            AddRow(dt, noticeName);
            sh.TableInsert(dt, "TAB_NOTICE_LOCAL");
            dt.Rows.Clear();
        }
        private void AddRow(DataTable dt, string noticeName)
        {
            DataRow row = dt.NewRow();
            row["NOTICE_NAME"] = noticeName;
            row["NOTICE_DATETIME"] = BaseInfoCommon.CurrentUser.DTSERVERDATE.ToString("yyyy-MM-dd HH:mm:ss");
            dt.Rows.Add(row);
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if (flg)
            {
                timeNum = GetLastInputTime() / 1000;
            }
            else
            {
                timeNum++;
            }

            if (timeNum == 1800)//30分钟无任何操作自动注销系统
            {
                //注销
                LoginOutByWho(0);
            }
        }

        string deptName = string.Empty;
        DataRow row = null;

       }
}
