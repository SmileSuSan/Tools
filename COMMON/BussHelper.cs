using K9;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SC
{
    public class BussHelper : BaseBussHelper
    {

        /// <summary>
        /// http下载数据
        /// </summary>
        /// <returns>返回获取值</returns>
        public static bool HttpDownLoad(string file, string savePath)
        {
            string url = string.Format("{0}://{1}:8070/download/{2}", HttpHelper.GetHttpType(), HttpHelper.ipAddress, file);

            return HttpDownLoad(file, savePath, url);
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="localFilePath">本地文件路径</param>
        /// <param name="destFileName">服务端存储文件名</param>
        /// <param name="destFilePath">服务端存储文件路径</param>
        /// <returns></returns>
        public static string UploadFile(string serverPath, string localFilePath, string destFileName, string destFilePath)
        {
            string url = string.Format("{0}/{1}DestFileName={2}&FilePath={3}&FileKey=mzWBbloM5ASNrYc6h8PBgg==", serverPath, "K9FileDataInterface/upload?", destFileName, destFilePath.Replace("\\", "/"));

            return BaseUploadFile(serverPath, localFilePath, destFileName, destFilePath, url);
        }

        /// <summary>
        /// 上传DLL文件
        /// </summary>
        /// <param name="localFilePath">本地文件路径</param>
        /// <param name="destFileName">服务端存储文件名</param>
        /// <param name="destFilePath">服务端存储文件路径</param>
        /// <returns></returns>
        public static string UploadFileDLL(string serverPath, string localFilePath, string destFileName, string destFilePath)
        {
            string url = string.Format("{0}/{1}DestFileName={2}&FilePath={3}&FileKey=mzWBbloM5ASNrYc6h8PBgg==&FileType=DLL", serverPath, "K9FileDataInterface/upload?", destFileName, destFilePath.Replace("\\", "/"));
            return UploadFileDLL(serverPath, localFilePath, destFileName, destFilePath, url);
        }
    }
}
