using K9;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SC
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
            //return;
            //处理系统异常
            Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            #region 映射主入口
            //命名空间加窗体名称
            string fullNamespace = "SC.FrmLogin";
            //程序集名称
            string strassembly = "MAIN";
            //通过反射获取程序集
            Assembly assembly = Assembly.Load(strassembly);
            if (assembly == null)
            {
                return;
            }
            //通过反射得到窗体
            Object obj = null;
            if (args.Length > 0)
            {
                string[] parame = args[0].Replace("@%#&@", "|").Split('|');
                obj = assembly.CreateInstance(fullNamespace, false, BindingFlags.Default, null, parame, null, null);
            }
            else
            {
                obj = assembly.CreateInstance(fullNamespace, true);
            }
            Form frm = null;
            if (obj != null)
            {
                frm = obj as Form;
            }
            #endregion

            Application.Run(frm);
        }
        #region 捕捉全局异常
        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            string str = "";
            Exception error = e.Exception as Exception;
            if (error != null)
            {
                str = string.Format("异常类型：{0}\r\n异常消息：{1}\r\n异常信息：{2}\r\n", error.GetType().Name, error.Message, error.StackTrace);
            }
            else
            {
                str = string.Format("应用程序线程错误:{0}", e);
            }
            LogHelper.Error("应用程序线程错误!", error);
            MessageBox.Show("发生系统错误，请及时联系管理员！" + str, "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string str = "";
            Exception error = e.ExceptionObject as Exception;
            string strDateInfo = "出现应用程序未处理的异常：" + DateTime.Now.ToString() + "\r\n";
            if (error != null)
            {
                str = string.Format(strDateInfo + "Application UnhandledException:{0};\n\r堆栈信息:{1}", error.Message, error.StackTrace);
            }
            else
            {
                str = string.Format("Application UnhandledError:{0}", e);
            }

            LogHelper.Error("Application UnhandledError!", error);
            MessageBox.Show("发生系统错误，请及时联系管理员！", "系统错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        #endregion
    }
    public static class UPDATE
    {
        private static bool isUpdate = false;
        public static bool IsUpdate
        {
            get { return isUpdate; }
        }
    }
}
