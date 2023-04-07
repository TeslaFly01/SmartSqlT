using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SmartSQL.Models
{
    /// <summary>
    /// 差异信息
    /// </summary>
    public class DiffInfoModel : NotifyPropertyBase
    {
        /// <summary>
        /// 源对象：名称
        /// </summary>
        public string SourceName { get; set; }
        /// <summary>
        /// 源对象：备注
        /// </summary>
        public string SourceRemark { get; set; }
        /// <summary>
        /// 源对象：是否存在
        /// </summary>
        public bool SourceIsExists { get; set; }
        /// <summary>
        /// 源对象：前景色
        /// </summary>
        public SolidColorBrush SourceForeground { get; set; }
        /// <summary>
        /// 目标对象：名称
        /// </summary>
        public string TargetName { get; set; }
        /// <summary>
        /// 目标对象：备注
        /// </summary>
        public string TargetRemark { get; set; }
        /// <summary>
        /// 目标对象：是否存在
        /// </summary>
        public bool TargetIsExists { get; set; }
        /// <summary>
        /// 目标对象：前景色
        /// </summary>
        public SolidColorBrush TargetForeground { get; set; }


    }
}
