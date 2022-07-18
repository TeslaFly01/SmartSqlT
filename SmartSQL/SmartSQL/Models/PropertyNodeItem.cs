using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SmartSQL.Annotations;

namespace SmartSQL.Models
{
    public class PropertyNodeItem : NotifyPropertyBase
    {
        public PropertyNodeItem()
        {
            IsChecked = false;
            IsExpanded = false;
            Children = new List<PropertyNodeItem>();
        }
        /// <summary>
        /// 对象ID
        /// </summary>
        public string ObejcetId { get; set; }
        /// <summary>
        /// 父级菜单
        /// </summary>
        public PropertyNodeItem Parent { get; set; }
        /// <summary>
        /// 图标
        /// </summary>
        public string Icon { get; set; }
        /// <summary>
        /// 显示名称（'架构名称' + '.' + '英文名称'）
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// 英文名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 所属架构
        /// </summary>
        public string Schema { get; set; }
        /// <summary>
        /// 类型
        /// </summary>
        public string Type { get; set; } = ObjType.Table;
        /// <summary>
        /// 文字显示颜色
        /// </summary>
        public string TextColor { get; set; } = "Black";
        /// <summary>
        /// 注释
        /// </summary>
        public string Comment { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime ModifyDate { get; set; }
        /// <summary>
        /// 是否展开
        /// </summary>
        private bool _isExpanded = false;
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (value != _isExpanded)
                {
                    //折叠状态改变
                    _isExpanded = value;
                    OnPropertyChanged("IsExpanded");
                }
            }
        }
        /// <summary>
        /// 字体粗细
        /// </summary>
        public string FontWeight { get; set; } = "Normal";
        /// <summary>
        /// 可见模式
        /// </summary>
        public string Visibility { get; set; } = "Visible";
        /// <summary>
        /// 是否选中
        /// </summary>
        private bool? _isChecked = false;
        public bool? IsChecked
        {
            get { return _isChecked; }
            set { this.SetIsChecked(value, true, true); }
        }
        /// <summary>
        /// 子项菜单
        /// </summary>
        public List<PropertyNodeItem> Children { get; set; }

        void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
        {
            if (value == _isChecked)
                return;

            _isChecked = value;

            if (updateChildren && _isChecked.HasValue)
                this.Children.ForEach(c => c.SetIsChecked(_isChecked, true, false));

            if (updateParent && Parent != null)
                Parent.VerifyCheckState();

            this.OnPropertyChanged("IsChecked");
        }

        void VerifyCheckState()
        {
            bool? state = null;
            for (int i = 0; i < this.Children.Count; ++i)
            {
                bool? current = this.Children[i].IsChecked;
                if (i == 0)
                {
                    state = current;
                }
                else if (state != current)
                {
                    state = null;
                    break;
                }
            }
            this.SetIsChecked(state, false, true);
        }
    }

    public static class ObjType
    {
        public const string Group = "Group";

        public const string Type = "Type";

        public const string Table = "Table";

        public const string View = "View";

        public const string Proc = "Proc";
    }
}
