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

        #region MyRegion
        private readonly static string CategoryApiUrl = "https://apiv.gitee.io/smartapi/categoryApi.json";

        private readonly static string SiteApiUrl = "https://apiv.gitee.io/smartapi/siteApi.json";

        #region 圆角度数
        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
"CornerRadius", typeof(int), typeof(UcMainSite), new PropertyMetadata(default(int)));

        /// <summary>
        /// 选项卡圆角度数
        /// </summary>
        public int CornerRadius
        {
            get => (int)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }
        #endregion

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
        #endregion

        public UcMainSite()
        {
            InitializeComponent();
            CornerRadius = 10;
            DataContext = this;
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var sqLiteHelper = new SQLiteHelper();
            var isMultipleTab = sqLiteHelper.GetSysBool(SysConst.Sys_IsMultipleTab);
            CornerRadius = isMultipleTab ? 0 : 10;
            await GetSiteInfo();
        }

        private async Task GetSiteInfo()
        {
            #region MyRegion
            await Task.Run(() =>
                {
                    var client = new RestClient(CategoryApiUrl);
                    var result = client.Execute(new RestRequest());
                    if (result.StatusCode == HttpStatusCode.OK)
                    {
                        var categoryList = JsonSerializer.Deserialize<List<CategoryApi>>(result.Content);
                        client = new RestClient(SiteApiUrl);
                        result = client.Execute(new RestRequest());
                        if (result.StatusCode == HttpStatusCode.OK)
                        {
                            var siteList = JsonSerializer.Deserialize<List<SiteApi>>(result.Content);
                            categoryList.ForEach(x =>
                            {
                                int initType = 0;
                                x.count = siteList.Count(t => t.category == x.categoryName);
                                if (x.type.Any())
                                {
                                    x.type.ForEach(t =>
                                    {
                                        if (initType == 0)
                                        {
                                            x.SelectedType = t;
                                        }
                                        t.sites = siteList.Where(s => s.category == x.categoryName && s.type == t.typeName).ToList();
                                        initType++;
                                    });
                                    x.type = x.type.Where(t => t.sites.Count > 0).ToList();
                                    return;
                                }
                                x.sites = siteList.Where(s => s.category == x.categoryName).ToList();
                            });
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                CategoryList = categoryList.Where(x => x.isEnable).ToList();
                            }));
                        }
                    }
                });
            #endregion
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
            //选中项
            var selectedItem = (CategoryApi)((System.Windows.Controls.ListBox)sender).SelectedItem;
            var itemToScrollIntoView = CategoryItems.ItemContainerGenerator.ContainerFromItem(selectedItem) as FrameworkElement;
            var itemPosition = itemToScrollIntoView.TransformToAncestor(CategoryItems).Transform(new Point(0, 0));
            // 滚动到item位置
            ScrollViewBox.ScrollToVerticalOffset(itemPosition.Y);
            ScrollViewBox.ScrollToHorizontalOffset(itemPosition.X);
        }
    }
}
