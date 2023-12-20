using SmartSQL.Annotations;
using SmartSQL.Framework;
using SmartSQL.Framework.SqliteModel;
using SmartSQL.Helper;
using SmartSQL.Views.Category;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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
using SmartSQL.Views;
using RestSharp;
using System.Net;
using System.Text.Json;
using SmartSQL.Models.Api;
using SmartSQL.Framework.Const;
using System.Windows.Forms;
using HandyControl.Controls;
using System.Diagnostics;
using System.Web.UI;

namespace SmartSQL.UserControl
{
    /// <summary>
    /// TagObjects.xaml 的交互逻辑
    /// </summary>
    public partial class UcMainSite : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region 分类列表
        public static readonly DependencyProperty CategoryListProperty = DependencyProperty.Register(
"CategoryList", typeof(List<CategoryApi>), typeof(UcMainSite), new PropertyMetadata(default(List<CategoryApi>)));

        /// <summary>
        /// 分类列表
        /// </summary>
        public List<CategoryApi> CategoryList
        {
            get => (List<CategoryApi>)GetValue(CategoryListProperty);
            set => SetValue(CategoryListProperty, value);
        }
        #endregion

        #region 站点列表
        public static readonly DependencyProperty SiteListProperty = DependencyProperty.Register(
"SiteList", typeof(List<SiteApi>), typeof(UcMainSite), new PropertyMetadata(default(List<SiteApi>)));

        /// <summary>
        /// 站点列表
        /// </summary>
        public List<SiteApi> SiteList
        {
            get => (List<SiteApi>)GetValue(SiteListProperty);
            set => SetValue(SiteListProperty, value);
        }
        #endregion

        public UcMainSite()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var sqLiteHelper = new SQLiteHelper();
            CategoryList = App.SiteInfo;
            NoMenuText.Visibility = CategoryList.Any() ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        /// 浏览器打开站点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Card_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var site = (SiteApi)((Card)sender).DataContext;
            Process.Start(site.url);
        }

        /// <summary>
        /// 选中左侧菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }
            //选中项
            var selectedItem = (CategoryApi)((System.Windows.Controls.ListBox)sender).SelectedItem;
            if (selectedItem == null)
            {
                return;
            }
            var itemToScrollIntoView = CategoryItems.ItemContainerGenerator.ContainerFromItem(selectedItem) as FrameworkElement;
            if (itemToScrollIntoView == null)
            {
                return;
            }
            var itemPosition = itemToScrollIntoView.TransformToAncestor(CategoryItems).Transform(new Point(0, 0));
            // 滚动到item位置
            ScrollViewBox.ScrollToVerticalOffset(itemPosition.Y);
            ScrollViewBox.ScrollToHorizontalOffset(itemPosition.X);
        }
    }
}
