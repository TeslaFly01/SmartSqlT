using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SmartSQL.Framework.PhysicalDataModel;
using SmartSQL.Models;
using System.Runtime.CompilerServices;
using SmartSQL.Framework;
using SmartSQL.Framework.SqliteModel;
using SmartSQL.Annotations;
using SmartSQL.DocUtils;
using SmartSQL.Framework.Const;
using SmartSQL.Helper;
using SmartSQL.UserControl;
using SmartSQL.Views;
using ComboBox = System.Windows.Controls.ComboBox;
using FontAwesome = FontAwesome.WPF.FontAwesome;
using TabControl = System.Windows.Controls.TabControl;
using TabItem = System.Windows.Controls.TabItem;

namespace SmartSQL.UserControl
{
    /// <summary>
    /// MainContent.xaml 的交互逻辑
    /// </summary>
    public partial class UcMD5 : BaseUserControl
    {
        private static readonly string GROUPICON = "pack://application:,,,/Resources/svg/category.svg";
        private static readonly string TABLEICON = "pack://application:,,,/Resources/svg/table.svg";
        private static readonly string VIEWICON = "pack://application:,,,/Resources/svg/view.svg";
        private static readonly string PROCICON = "pack://application:,,,/Resources/svg/proc.svg";

        private List<TreeNodeItem> itemList = new List<TreeNodeItem>();


        public static readonly DependencyProperty ToolMenuListProperty = DependencyProperty.Register(
            "ToolMenuList", typeof(List<TreeNodeItem>), typeof(UcToolMenu), new PropertyMetadata(default(List<TreeNodeItem>)));


        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            "CornerRadius", typeof(int), typeof(UcToolMenu), new PropertyMetadata(default(int)));


        /// <summary>
        /// 左侧菜单数据
        /// </summary>
        public List<TreeNodeItem> ToolMenuList
        {
            get => (List<TreeNodeItem>)GetValue(ToolMenuListProperty);
            set
            {
                SetValue(ToolMenuListProperty, value);
                OnPropertyChanged(nameof(ToolMenuList));
            }
        }

        public ObservableCollection<MainTabWModel> TabItemData = new ObservableCollection<MainTabWModel>();

        public UcMD5()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void BaseUserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }
    }
}
