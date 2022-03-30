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
            this.Dto = dto;
            this.Filter = filter;
            this.WorkTmpDir = Path.Combine(AppPath, dto.DBType + "_" + dto.DBName);
            if (ZlpIOHelper.DirectoryExists(this.WorkTmpDir))
            {
                ZlpIOHelper.DeleteDirectory(this.WorkTmpDir, true);
            }
            ZlpIOHelper.CreateDirectory(this.WorkTmpDir);
            this.Ext = this.Filter.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault()?.Trim('*');
        }

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
        public abstract void Build(string filePath);
    }
}
