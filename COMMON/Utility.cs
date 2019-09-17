using K9;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SC
{
    public class Utility : BaseUtility
    {
        private static string _LoginID;
        /// <summary>
        /// 记录登录ID
        /// </summary>
        public static string LoginID
        {
            get { return _LoginID; }
            set { _LoginID = value; }
        }

        private static string _LoginPWD;
        /// <summary>
        /// 记录登录密码
        /// </summary>
        public static string LoginPWD
        {
            get { return _LoginPWD; }
            set { _LoginPWD = value; }
        }

        /// <summary>
        /// 获取DataTable指定列的拼接值
        /// </summary>
        /// <param name="dt">数据源</param>
        /// <param name="columnName">获取列名</param>
        /// <param name="strType">是否需要拼接引号</param>
        /// <returns></returns>
        public static string GetColumnValues(DataTable dt, string columnName, bool strType)
        {
            if (GetRowCount(dt) > 0 && dt.Columns.Contains(columnName))
            {
                string columns = ",";
                StringBuilder sb = new StringBuilder(200);
                foreach (DataRow item in dt.Rows)
                {
                    string columnValue = item[columnName].ToString();
                    if (!string.IsNullOrWhiteSpace(columnValue))
                    {
                        string value = strType ? "'" + columnValue + "'," : columnValue + ",";
                        if (!columns.Contains(value))
                        {
                            sb.Append(value);
                            columns += value;
                        }
                    }
                }
                return sb.ToString().Trim(',');
            }
            return "";
        }

        public static int GetRowCount(DataTable dt)
        {
            if (dt == null)
            {
                return -1;
            }
            return dt.Rows.Count;
        }
    }
}
