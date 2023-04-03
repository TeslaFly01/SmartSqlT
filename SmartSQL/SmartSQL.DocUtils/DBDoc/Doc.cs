using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartSQL.DocUtils.Dtos;
using ZetaLongPaths;

namespace SmartSQL.DocUtils.DBDoc
{
    public abstract class Doc
    {

        /// <summary>
        /// 临时文件的存放目录
        /// </summary>
        public string WorkTmpDir { get; private set; }

        /// <summary>
        /// 文件扩展名
        /// </summary>
        public string Ext { get; set; }

        /// <summary>
        /// 数据库Dto
        /// </summary>
        public DBDto Dto { get; }

        /// <summary>
        /// 扩展名过滤字符串
        /// </summary>
        public string Filter { get; }

        /// <summary>
        /// 构建生成文档
        /// </summary>
        public abstract bool Build(string filePath);

        /// <summary>
        /// 声明委托参数
        /// </summary>
        public class ChangeRefreshProgressArgs : EventArgs
        {
            /// <summary>
            /// 当前导出文档类型
            /// </summary>
            public DocType Type { get; set; }
            /// <summary>
            /// 已生成数量
            /// </summary>
            public int BuildNum { get; set; }
            /// <summary>
            /// 总数量
            /// </summary>
            public int TotalNum { get; set; }
            /// <summary>
            /// 当前生成对象名称
            /// </summary>
            public string BuildName { get; set; }
            /// <summary>
            /// 是否结束
            /// </summary>
            public bool IsEnd { get; set; } = false;
        }

        //声明进度数据委托
        public delegate void ChangeRefreshProgressHandler(object buildNum, EventArgs buildObjectName);
        //声明进度数据委托事件
        public event EventHandler<ChangeRefreshProgressArgs> ChangeRefreshProgressEvent;
        /// <summary>
        /// 当前应用程序的名称 => SmartSQL
        /// </summary>
        private static string ConfigFileName = "SmartSQL";

        /// <summary>
        /// 定义配置存放的路径 => C:\Users\用户名\AppData\Roaming\SmartSQL
        /// </summary>
        public static string AppPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData, Environment.SpecialFolderOption.Create), ConfigFileName);

        /// <summary>
        /// 定义模板文件存放的路径
        /// </summary>
        public static string TplPath = Path.Combine(AppPath, "TplFile");

        public Doc(DBDto dto, string filter)
        {
            #region MyRegion
            this.Dto = dto;
            this.Filter = filter;
            var dbName = dto.DBName;
            if (dbName.Contains(":"))
            {
                dbName = dbName.Replace(":", "");
            }
            this.WorkTmpDir = Path.Combine(AppPath, dto.DBType + "_" + dbName);
            if (ZlpIOHelper.DirectoryExists(this.WorkTmpDir))
            {
                ZlpIOHelper.DeleteDirectory(this.WorkTmpDir, true);
            }
            ZlpIOHelper.CreateDirectory(this.WorkTmpDir);
            this.Ext = this.Filter.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault()?.Trim('*');
            #endregion
        }

        public virtual void OnProgress(ChangeRefreshProgressArgs agrs)
        {
            if (ChangeRefreshProgressEvent != null)
            {
                this.ChangeRefreshProgressEvent(2, agrs);
            }
        }
    }
}
