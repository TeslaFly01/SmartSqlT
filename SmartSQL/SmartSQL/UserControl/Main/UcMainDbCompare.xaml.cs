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
using SqlSugar;
using ComboBox = System.Windows.Controls.ComboBox;
using FontAwesome = FontAwesome.WPF.FontAwesome;
using TabControl = System.Windows.Controls.TabControl;
using TabItem = System.Windows.Controls.TabItem;

namespace SmartSQL.UserControl
{
    /// <summary>
    /// MainContent.xaml 的交互逻辑
    /// </summary>
    public partial class UcMainDbCompare : BaseUserControl
    {
        private static readonly string GROUPICON = "pack://application:,,,/Resources/svg/category.svg";
        private static readonly string TAGICON = "pack://application:,,,/Resources/svg/tag.svg";
        private static readonly string TABLEICON = "pack://application:,,,/Resources/svg/table.svg";
        private static readonly string VIEWICON = "pack://application:,,,/Resources/svg/view.svg";
        private static readonly string PROCICON = "pack://application:,,,/Resources/svg/proc.svg";

        public static readonly DependencyProperty TreeViewLeftDataProperty = DependencyProperty.Register(
            "TreeViewLeftData", typeof(List<TreeNodeItem>), typeof(UcMainDbCompare), new PropertyMetadata(default(List<TreeNodeItem>)));
        /// <summary>
        /// 左侧菜单数据
        /// </summary>
        public List<TreeNodeItem> TreeViewLeftData
        {
            get => (List<TreeNodeItem>)GetValue(TreeViewLeftDataProperty);
            set
            {
                SetValue(TreeViewLeftDataProperty, value);
                OnPropertyChanged(nameof(TreeViewLeftData));
            }
        }

        public static readonly DependencyProperty TreeViewRightDataProperty = DependencyProperty.Register(
            "TreeViewRightData", typeof(List<TreeNodeItem>), typeof(UcMainDbCompare), new PropertyMetadata(default(List<TreeNodeItem>)));
        /// <summary>
        /// 左侧菜单数据
        /// </summary>
        public List<TreeNodeItem> TreeViewRightData
        {
            get => (List<TreeNodeItem>)GetValue(TreeViewRightDataProperty);
            set
            {
                SetValue(TreeViewRightDataProperty, value);
                OnPropertyChanged(nameof(TreeViewRightData));
            }
        }

        public static readonly DependencyProperty DiffInfoDataProperty = DependencyProperty.Register(
            "DiffInfoData", typeof(List<DiffInfoModel>), typeof(UcMainDbCompare), new PropertyMetadata(default(List<DiffInfoModel>)));
        /// <summary>
        /// 差异数据
        /// </summary>
        public List<DiffInfoModel> DiffInfoData
        {
            get => (List<DiffInfoModel>)GetValue(DiffInfoDataProperty);
            set
            {
                SetValue(DiffInfoDataProperty, value);
                OnPropertyChanged(nameof(DiffInfoData));
            }
        }

        public UcMainDbCompare()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void UcMainDbCompare_OnLoaded(object sender, RoutedEventArgs e)
        {
            var sqLiteHelper = SQLiteHelper.GetInstance();
            var connectConfigs = sqLiteHelper.GetList<ConnectConfigs>();
            ComSourceConnect.ItemsSource = null;
            ComSourceConnect.ItemsSource = connectConfigs;
            ComTargetConnect.ItemsSource = null;
            ComTargetConnect.ItemsSource = connectConfigs;
        }

        /// <summary>
        /// 选中源连接加载对应数据库
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComSourceConnect_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            #region MyRegion
            if (!IsLoaded)
            {
                return;
            }
            var selConnectConfig = (ConnectConfigs)((ComboBox)sender).SelectedItem;
            var dbInstance = ExporterFactory.CreateInstance(selConnectConfig.DbType, selConnectConfig.DbMasterConnectString);
            var list = dbInstance.GetDatabases(selConnectConfig.DefaultDatabase);
            ComSourceDatabase.ItemsSource = list;
            ComSourceDatabase.SelectedItem = list.FirstOrDefault(x => x.DbName == selConnectConfig.DefaultDatabase);
            #endregion
        }

        /// <summary>
        /// 选中目标连接加载对应数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComTargetConnect_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            #region MyRegion
            if (!IsLoaded)
            {
                return;
            }
            if (ComSourceConnect.SelectedItem == null)
            {
                Oops.Oh("请先选择源数据库");
                return;
            }
            var sourceConnect = (ConnectConfigs)ComSourceConnect.SelectedItem;
            var selConnectConfig = (ConnectConfigs)((ComboBox)sender).SelectedItem;
            if (selConnectConfig.DbType != sourceConnect.DbType)
            {
                Oops.Oh("请选择和源数据库相同类型数据库");
                return;
            }
            var dbInstance = ExporterFactory.CreateInstance(selConnectConfig.DbType, selConnectConfig.DbMasterConnectString);
            var list = dbInstance.GetDatabases(selConnectConfig.DefaultDatabase);
            ComTargetDatabase.ItemsSource = list;
            ComTargetDatabase.SelectedItem = list.FirstOrDefault(x => x.DbName == selConnectConfig.DefaultDatabase);
            #endregion
        }

        /// <summary>
        /// 开始比较
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCompare_OnClick(object sender, RoutedEventArgs e)
        {
            if (ComSourceDatabase.SelectedItem == null)
            {
                Oops.Oh("请选择源数据库");
                return;
            }
            if (ComTargetDatabase.SelectedItem == null)
            {
                Oops.Oh("请选择目标数据库");
                return;
            }
            BindDiffData();
        }

        /// <summary>
        /// 绑定差异数据
        /// </summary>
        public void BindDiffData()
        {
            #region MyRegion
            var selectSourceConnection = (ConnectConfigs)ComSourceConnect.SelectedItem;
            var selectSourceDataBase = (DataBase)ComSourceDatabase.SelectedItem;
            var selectTargetConnection = (ConnectConfigs)ComTargetConnect.SelectedItem;
            var selectTargetDataBase = (DataBase)ComTargetDatabase.SelectedItem;
            LoadingLine.Visibility = Visibility.Visible;
            BoxCompareData.Visibility=Visibility.Visible;
            Task.Run(() =>
            {
                var diffInfoList = new List<DiffInfoModel>();
                try
                {
                    var sDbType = selectSourceConnection.DbType;
                    var sDbName = selectSourceDataBase.DbName;
                    var tDbType = selectTargetConnection.DbType;
                    var tDbName = selectTargetDataBase.DbName;
                    var dbSourceInstance = ExporterFactory.CreateInstance(sDbType, selectSourceConnection.SelectedDbConnectString(sDbName), sDbName);
                    var sourceModel = dbSourceInstance.Init();
                    var dbTargetInstance = ExporterFactory.CreateInstance(tDbType, selectTargetConnection.SelectedDbConnectString(tDbName), tDbName);
                    var targetModel = dbTargetInstance.Init();
                    foreach (var t in sourceModel.Tables)
                    {
                        diffInfoList.Add(new DiffInfoModel
                        {
                            SourceName = t.Value.DisplayName,
                            SourceRemark = t.Value.Comment,
                            SourceIsExists = true,
                            TargetIsExists = true,
                            TargetForeground = null
                        });
                    }
                    foreach (var table in targetModel.Tables)
                    {
                        var tb = diffInfoList.FirstOrDefault(x => x.SourceName == table.Value.DisplayName);
                        if (tb != null)
                        {
                            tb.TargetName = table.Value.DisplayName;
                            tb.TargetRemark = table.Value.Comment;
                            tb.TargetIsExists = true;
                            tb.TargetForeground = null;
                        }
                        else
                        {
                            diffInfoList.Add(new DiffInfoModel
                            {
                                SourceIsExists = false,
                                SourceForeground = new SolidColorBrush(Colors.Red),
                                TargetName = table.Value.DisplayName,
                                TargetRemark = table.Value.Comment,
                                TargetIsExists = true
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Oops.God($"连接失败 {selectSourceConnection.ConnectName}，原因：" + ex.ToMsg());
                    }));
                }
                finally
                {
                    this.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        DiffInfoData = diffInfoList;
                        LoadingLine.Visibility = Visibility.Collapsed;
                    }));
                }
            });
            #endregion
        }
    }
}
