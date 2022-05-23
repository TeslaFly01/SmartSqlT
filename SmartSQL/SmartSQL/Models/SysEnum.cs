using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartSQL.Models
{
    /// <summary>
    /// 导出文档类型
    /// </summary>
    public enum ExportEnum
    {
        /// <summary>
        /// 全部导出
        /// </summary>
        All = 0,
        /// <summary>
        /// 部分导出
        /// </summary>
        Partial = 1
    }

    /// <summary>
    /// 显示类型
    /// </summary>
    public enum ShowType
    {
        /// <summary>
        /// 文字
        /// </summary>
        Txt = 0,
        /// <summary>
        /// 图片
        /// </summary>
        Img = 1,
    }

    /// <summary>
    /// 左侧菜单类型
    /// </summary>
    public enum LeftMenuType
    {
        /// <summary>
        /// 全部
        /// </summary>
        All = 1,
        /// <summary>
        /// 分组
        /// </summary>
        Group = 2,
        /// <summary>
        /// 收藏
        /// </summary>
        Fav = 3
    }
}
