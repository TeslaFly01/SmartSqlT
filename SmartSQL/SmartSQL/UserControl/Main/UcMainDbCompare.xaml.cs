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

        public UcMainDbCompare()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void UcMainDbCompare_OnLoaded(object sender, RoutedEventArgs e)
        {
            var sqLiteHelper = SQLiteHelper.GetInstance();
            var connectConfigs = sqLiteHelper.ToList<ConnectConfigs>();
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
            if (selConnectConfig.DbType == DbType.PostgreSQL)
            {
                ComSourceDatabase.SelectedItem = list.FirstOrDefault(x => x.DbName.EndsWith("public"));
                return;
            }
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
            if (selConnectConfig.DbType == DbType.PostgreSQL)
            {
                ComTargetDatabase.SelectedItem = list.FirstOrDefault(x => x.DbName.EndsWith("public"));
                return;
            }
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
            BindLeftMenu();
            BindRightMenu();
        }


        private static readonly string GROUPICON = "pack://application:,,,/Resources/svg/category.svg";
        private static readonly string TAGICON = "pack://application:,,,/Resources/svg/tag.svg";
        private static readonly string TABLEICON = "pack://application:,,,/Resources/svg/table.svg";
        private static readonly string VIEWICON = "pack://application:,,,/Resources/svg/view.svg";
        private static readonly string PROCICON = "pack://application:,,,/Resources/svg/proc.svg";

        public void BindLeftMenu()
        {
            #region MyRegion
            var selectConnection = (ConnectConfigs)ComSourceConnect.SelectedItem;
            var selectDataBase = (DataBase)ComSourceDatabase.SelectedItem;
            var menuData = TreeViewLeftData;
            Task.Run(() =>
            {
                var itemList = new List<TreeNodeItem>();
                var nodeTable = new TreeNodeItem
                {
                    ObejcetId = "0",
                    DisplayName = "表",
                    Name = "treeTable",
                    Icon = TABLEICON,
                    IsExpanded = true,
                    Type = ObjType.Type
                };
                itemList.Add(nodeTable);

                var model = new Model();
                try
                {
                    var dbInstance = ExporterFactory.CreateInstance(selectConnection.DbType, selectConnection.SelectedDbConnectString(selectDataBase.DbName), selectDataBase.DbName);
                    model = dbInstance.Init();
                    //menuData = model;
                }
                catch (Exception ex)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Oops.God($"连接失败 {selectConnection.ConnectName}，原因：" + ex.ToMsg());
                    }));
                }
                var textColor = "#4f5d79";
                foreach (var t in model.Tables)
                {
                    var tableItem = new TreeNodeItem
                    {
                        ObejcetId = t.Value.Id,
                        DisplayName = t.Value.DisplayName,
                        Name = t.Value.Name,
                        Schema = t.Value.SchemaName,
                        Comment = t.Value.Comment,
                        CreateDate = t.Value.CreateDate,
                        ModifyDate = t.Value.ModifyDate,
                        TextColor = textColor,
                        Icon = TABLEICON,
                        Type = ObjType.Table
                    };
                    nodeTable.Children.Add(tableItem);
                }

                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    itemList.ForEach(obj =>
                    {
                        if (!obj.Children.Any())
                        {
                            obj.Visibility = nameof(Visibility.Collapsed);
                        }
                        obj.DisplayName += $"（{obj.Children.Count}）";
                    });
                    TreeViewLeftData = itemList;
                }));
            });
            #endregion
        }


        public void BindRightMenu()
        {
            #region MyRegion
            var selectConnection = (ConnectConfigs)ComTargetConnect.SelectedItem;
            var selectDataBase = (DataBase)ComTargetDatabase.SelectedItem;
            var menuData = TreeViewLeftData;
            Task.Run(() =>
            {
                var itemList = new List<TreeNodeItem>();
                var nodeTable = new TreeNodeItem
                {
                    ObejcetId = "0",
                    DisplayName = "表",
                    Name = "treeTable",
                    Icon = TABLEICON,
                    IsExpanded = true,
                    Type = ObjType.Type
                };
                itemList.Add(nodeTable);

                var model = new Model();
                try
                {
                    var dbInstance = ExporterFactory.CreateInstance(selectConnection.DbType, selectConnection.SelectedDbConnectString(selectDataBase.DbName), selectDataBase.DbName);
                    model = dbInstance.Init();
                    //menuData = model;
                }
                catch (Exception ex)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Oops.God($"连接失败 {selectConnection.ConnectName}，原因：" + ex.ToMsg());
                    }));
                }
                var textColor = "#4f5d79";
                foreach (var t in model.Tables)
                {
                    var tableItem = new TreeNodeItem
                    {
                        ObejcetId = t.Value.Id,
                        DisplayName = t.Value.DisplayName,
                        Name = t.Value.Name,
                        Schema = t.Value.SchemaName,
                        Comment = t.Value.Comment,
                        CreateDate = t.Value.CreateDate,
                        ModifyDate = t.Value.ModifyDate,
                        TextColor = textColor,
                        Icon = TABLEICON,
                        Type = ObjType.Table
                    };
                    nodeTable.Children.Add(tableItem);
                }

                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    itemList.ForEach(obj =>
                    {
                        if (!obj.Children.Any())
                        {
                            obj.Visibility = nameof(Visibility.Collapsed);
                        }
                        obj.DisplayName += $"（{obj.Children.Count}）";
                    });
                    TreeViewRightData = itemList;
                }));
            });
            #endregion
        }

        private void TreeViewSourceTables_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var selData = (TreeNodeItem)((TreeView)sender).SelectedItem;
            TreeViewRightData.ForEach(x =>
            {
                if (selData.Name == x.Name)
                {
                    x.IsChecked = true;
                }
            });
            TreeViewTargetTables.ItemsSource = null;
            TreeViewTargetTables.ItemsSource = TreeViewRightData;
        }
    }
}
