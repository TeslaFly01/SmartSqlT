using System;
using System.Collections.Generic;
using System.Linq;
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
using HandyControl.Data;
using SmartCode.Framework.PhysicalDataModel;
using SmartCode.Tool.Models;
using SmartCode.Tool.Views;
using UserControlE = System.Windows.Controls.UserControl;

namespace SmartCode.Tool.UserControl
{
    /// <summary>
    /// MainW.xaml 的交互逻辑
    /// </summary>
    public partial class MainW : BaseUserControl
    {
        #region Filds
        public event ObjChangeRefreshHandler ObjChangeRefreshEvent;
        public static readonly DependencyProperty SelectedObjectProperty = DependencyProperty.Register(
            "SelectedObject", typeof(PropertyNodeItem), typeof(MainW), new PropertyMetadata(default(PropertyNodeItem)));

        public static readonly DependencyProperty SelectedDataBaseProperty = DependencyProperty.Register(
            "SelectedDataBase", typeof(DataBase), typeof(MainW), new PropertyMetadata(default(DataBase)));

        public static readonly DependencyProperty SelectedConnectionProperty = DependencyProperty.Register(
            "SelectedConnection", typeof(DataBasesConfig), typeof(MainW), new PropertyMetadata(default(DataBasesConfig)));

        public static readonly DependencyProperty MainTitleProperty = DependencyProperty.Register(
            "MainTitle", typeof(string), typeof(MainW), new PropertyMetadata(default(string)));

        /// <summary>
        /// 当前选中对象
        /// </summary>
        public PropertyNodeItem SelectedObject
        {
            get => (PropertyNodeItem)GetValue(SelectedObjectProperty);
            set => SetValue(SelectedObjectProperty, value);
        }
        /// <summary>
        /// 当前选中数据库
        /// </summary>
        public DataBase SelectedDataBase
        {
            get => (DataBase)GetValue(SelectedDataBaseProperty);
            set => SetValue(SelectedDataBaseProperty, value);
        }
        /// <summary>
        /// 当前数据连接
        /// </summary>
        public DataBasesConfig SelectedConnection
        {
            get => (DataBasesConfig)GetValue(SelectedConnectionProperty);
            set => SetValue(SelectedConnectionProperty, value);
        }
        public string MainTitle
        {
            get => (string)GetValue(MainTitleProperty);
            set => SetValue(MainTitleProperty, value);
        }
        #endregion

        public MainW()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void LoadPage(List<PropertyNodeItem> objectsViewData)
        {
            MainTitle = SelectedObject.DisplayName;
            if (SelectedObject.Type == ObjType.Type)
            {
                MainColumns.Visibility = Visibility.Collapsed;
                MainObjects.Visibility = Visibility.Visible;
                MainObjects.SelectedConnection = SelectedConnection;
                MainObjects.SelectedDataBase = SelectedDataBase;
                MainObjects.SelectedObject = SelectedObject;
                MainObjects.ObjectsViewData = objectsViewData;
                MainObjects.LoadPageData();
            }
            else
            {
                MainColumns.Visibility = Visibility.Visible;
                MainObjects.Visibility = Visibility.Collapsed;
                MainColumns.ObjChangeRefreshEvent += ObjChangeRefreshEvent;
                MainColumns.SelectedConnection = SelectedConnection;
                MainColumns.SelectedDataBase = SelectedDataBase;
                MainColumns.SelectedObject = SelectedObject;
                MainColumns.LoadPageData();
            }
        }
    }
}
