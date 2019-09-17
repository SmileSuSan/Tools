using K9;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace SC
{
    public class HttpHelper : BaseHttpHelper
    {
        public static string ipAddress { get; set; }//IP地址
        public static string port { get; set; }//端口

        private static bool IsHttp = true;//指示访问方式为 http 还是 https true时为http

        public static string GetHttpType()
        {
            string str_http = IsHttp ? "http" : "https";
            return str_http;
        }

        /// <summary>
        /// 根据服务名称获取URL
        /// </summary>
        /// <returns>接口地址端口</returns>
        public static string GetUrl(ServerName serverName)
        {
            if (string.IsNullOrWhiteSpace(HttpHelper.ipAddress))
            {
                //ipAddress = "jrwldg.cfss.net.cn";//正式
                //ipAddress = "220.231.140.35";//正式
                ipAddress = "jrwlcs.cfss.net.cn";//测试
                port = "8989";
            }
            return string.Format("{0}://{1}:{2}/{3}", GetHttpType(), ipAddress, port, "YinYanServer/basedata");
        }

        /// <summary>
        /// 发送请求返回数据
        /// </summary>
        /// <param name="type">服务名</param>
        /// <param name="method">方法名</param>
        /// <param name="valueData">参数</param>
        /// <returns></returns>
        public static DataSet TransData(ServerName server, string met, string valueData)
        {
            return TransDataBase(met, valueData, GetUrl(server));
        }

        /// <summary>
        /// 发送请求返回数据
        /// </summary>
        /// <param name="type">服务名</param>
        /// <param name="method">方法名</param>
        /// <param name="valueData">参数</param>
        /// <returns></returns>
        public static DataSet TransData(ServerName server, string met, string valueData, string tableName)
        {
            return TransDataBase(met, valueData, tableName, GetUrl(server));
        }
    }
}
