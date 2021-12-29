using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;

namespace SmartCode.DocUtils.Dtos
{
    /// <summary>
    /// 数据库Dto
    /// </summary>
    public class DBDto
    {
        public DBDto() { }

        public DBDto(string dbName, object tag = null)
        {
            this.DBName = dbName;
            this.Tag = tag;
        }

        /// <summary>
        /// 数据库名称
        /// </summary>
        public string DBName { get; set; }

        /// <summary>
        /// 数据库类型
        /// </summary>
        public string DBType { get; set; }


        private List<TableDto> _Tables = null;
        /// <summary>
        /// 表结构信息
        /// </summary>
        public List<TableDto> Tables
        {
            get
            {
                if (_Tables == null)
                {
                    return new List<TableDto>();
                }
                else
                {
                    _Tables.ForEach(t =>
                    {
                        t.Comment = FilterIllegalDir(t.Comment);
                    });
                    return _Tables;
                }
            }
            set
            {
                _Tables = value;
            }
        }

        /// <summary>
        /// 数据库视图
        /// </summary>
        public Dictionary<string,string> Views { get; set; }

        /// <summary>
        /// 数据库存储过程
        /// </summary>
        public Dictionary<string, string> Procs { get; set; }

        /// <summary>
        /// 其他一些参数数据，用法如 winform 控件的 Tag属性
        /// </summary>
        public object Tag { get; set; }


        /// <summary>
        /// 处理非法字符路径
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private string FilterIllegalDir(string str)
        {
            if (str.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                str = string.Join(" ", str.Split(Path.GetInvalidFileNameChars()));
            }
            return str;
        }

    }
}
