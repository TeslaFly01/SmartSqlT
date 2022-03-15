using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCode.Tool.Models
{
    public enum ExportEnum
    {
        All = 0,
        Partial = 1
    }

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
