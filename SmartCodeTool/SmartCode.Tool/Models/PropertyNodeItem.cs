using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SmartCode.Tool.Annotations;

namespace SmartCode.Tool.Models
{
    public class PropertyNodeItem : NotifyPropertyBase
    {
        public PropertyNodeItem()
        {
            IsSelected = false;
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
        /// 显示名称
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// 英文名称
        /// </summary>
        public string Name { get; set; }
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
        public bool IsExpanded { get; set; } = false;
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
        private bool? isSelected = false;
        public bool? IsSelected
        {
            get
            {
                return isSelected;
            }
            set
            {
                isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }
        /// <summary>
        /// 子项菜单
        /// </summary>
        public List<PropertyNodeItem> Children { get; set; }

        /// <summary>
        /// 设置选中
        /// </summary>
        /// <param name="value"></param>
        /// <param name="checkedChildren"></param>
        /// <param name="checkedParent"></param>
        private void SetIsChecked(bool? value, bool checkedChildren, bool checkedParent)
        {
            if (isSelected == value) return;
            isSelected = value;
            //选中和取消子类
            if (checkedChildren && value.HasValue && Children != null)
                Children.ForEach(ch => ch.SetIsChecked(value, true, false));

            //选中和取消父类
            if (checkedParent && this.Parent != null)
                this.Parent.CheckParentCheckState();

            //通知更改
            this.SetProperty(x => x.IsSelected);
        }

        /// <summary>
        /// 检查父类是否选 中
        /// 如果父类的子类中有一个和第一个子类的状态不一样父类ischecked为null
        /// </summary>
        private void CheckParentCheckState()
        {
            List<PropertyNodeItem> checkedItems = new List<PropertyNodeItem>();
            string checkedNames = string.Empty;
            bool? _currentState = this.IsSelected;
            bool? _firstState = null;
            for (int i = 0; i < this.Children.Count(); i++)
            {
                bool? childrenState = this.Children[i].IsSelected;
                if (i == 0)
                {
                    _firstState = childrenState;
                }
                else if (_firstState != childrenState)
                {
                    _firstState = null;
                }
            }
            if (_firstState != null) _currentState = _firstState;
            SetIsChecked(_firstState, false, true);
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
