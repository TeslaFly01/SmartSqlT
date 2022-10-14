using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartSQL.Framework.PhysicalDataModel;
using SmartSQL.UserControl;

namespace SmartSQL.Models
{
    public class MainTabWModel
    {
        /// <summary>
        /// 选项卡名称
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// 选项卡图标
        /// </summary>
        public string Icon { get; set; }
        /// <summary>
        /// 选项卡内容控件
        /// </summary>
        public UcMainW MainW { get; set; }
    }
}
