using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using System.IO;
using SmartSQL.Views.Category;
using System.Data.Entity.Core.Objects;
using System.Data.Entity;
using Org.BouncyCastle.Math.EC.Rfc7748;
using Org.BouncyCastle.Crypto.Agreement;
using NLog;
using SmartSQL.ViewModels;
using System.Windows.Threading;
using System.ComponentModel;

namespace SmartSQL.UserControl
{
    /// <summary>
    /// MainContent.xaml 的交互逻辑
    /// </summary>
    public partial class UcMainRedis : BaseUserControl
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly Dictionary<string, string> IconDic = new Dictionary<string, string>
        {
            { "Type", SysConst.Sys_GROUPICON },
            { "Table", SysConst.Sys_TABLEICON },
            { "View", SysConst.Sys_VIEWICON },
            { "Proc", SysConst.Sys_PROCICON },
            { "SQL", SysConst.Sys_SQLICON }
        };

        private List<TreeNodeItem> itemList = new List<TreeNodeItem>();

        #region Fields
        public static readonly DependencyProperty SelectedConnectionProperty = DependencyProperty.Register(
            "SelectedConnection", typeof(ConnectConfigs), typeof(UcMainRedis), new PropertyMetadata(default(ConnectConfigs)));

        public static readonly DependencyProperty MenuDataProperty = DependencyProperty.Register(
            "MenuData", typeof(Model), typeof(UcMainRedis), new PropertyMetadata(default(Model)));

        public static readonly DependencyProperty TreeViewDataProperty = DependencyProperty.Register(
            "TreeViewData", typeof(List<TreeNodeItem>), typeof(UcMainRedis), new PropertyMetadata(default(List<TreeNodeItem>)));

        /// <summary>
        /// 菜单源数据
        /// </summary>
        public ConnectConfigs SelectedConnection
        {
            get => (ConnectConfigs)GetValue(SelectedConnectionProperty);
            set => SetValue(SelectedConnectionProperty, value);
        }

        /// <summary>
        /// 菜单源数据
        /// </summary>
        public Model MenuData
        {
            get => (Model)GetValue(MenuDataProperty);
            set => SetValue(MenuDataProperty, value);
        }

        /// <summary>
        /// 左侧菜单数据
        /// </summary>
        public List<TreeNodeItem> TreeViewData
        {
            get => (List<TreeNodeItem>)GetValue(TreeViewDataProperty);
            set
            {
                SetValue(TreeViewDataProperty, value);
                OnPropertyChanged(nameof(TreeViewData));
            }
        }
        #endregion

        public ObservableCollection<MainTabWModel> TabItemData = new ObservableCollection<MainTabWModel>();

        public UcMainRedis()
        {
            InitializeComponent();
            DataContext = this;
        }

        /// <summary>
        /// 页面初始化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UcMainRedis_OnLoaded(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            MainTabW.Visibility = Visibility.Collapsed;
            TabItemData.Clear();
            MainTabW.DataContext = TabItemData;
            MainTabW.SetBinding(ItemsControl.ItemsSourceProperty, new Binding());

            LoadingLine.Visibility = Visibility.Visible;
            var connectConfig = SelectedConnection;
            var list = new List<DataBase>();
            Task.Run(() =>
            {
                try
                {
                    var dbInstance = ExporterFactory.CreateInstance(connectConfig.DbType, connectConfig.DbMasterConnectString);
                    list = dbInstance.GetDatabases(connectConfig.DefaultDatabase);
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        ComSelectDatabase.ItemsSource = list;
                        HidSelectDatabase.Text = connectConfig.DefaultDatabase;
                        DataBase selectedDataBase = list.FirstOrDefault(x => x.DbName == connectConfig.DefaultDatabase);
                        ComSelectDatabase.SelectedItem=selectedDataBase;



                        MainTabW.Visibility = Visibility.Visible;
                        if (TabItemData.Count > 0)
                        {
                            var curItem = TabItemData.FirstOrDefault();
                            TabItemData.Remove(curItem);
                        }
                        var mainW = new UcRedisInfo
                        {
                            SelectedConnection = SelectedConnection
                        };
                        var tabItem = new MainTabWModel
                        {
                            DisplayName = SelectedConnection.ConnectName,
                            Icon = IconDic["Type"],
                            MainW = mainW
                        };
                        TabItemData.Insert(0, tabItem);
                        MainTabW.SelectedItem = TabItemData.First();



                    }));
                }
                catch (Exception ex)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Oops.God($"连接失败 {connectConfig.ConnectName}，原因：" + ex.ToMsg());
                        LoadingLine.Visibility = Visibility.Collapsed;
                    }));
                }
            });
            #endregion
        }

        /// <summary>
        /// 选择数据库发生变更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComSelectDatabase_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            #region MyRegion     
            if (!IsLoaded)
            {
                return;
            }
            var selectDatabase = ComSelectDatabase.SelectedItem;
            if (selectDatabase != null)
            {
                var selectedDbBase = (DataBase)selectDatabase;
                HidSelectDatabase.Text = ((DataBase)selectDatabase).DbName;
                var sqLiteHelper = new SQLiteHelper();
                sqLiteHelper.SetSysValue(SysConst.Sys_SelectedDataBase, selectedDbBase.DbName);
                MenuBind();
            }
            #endregion
        }

        public void MenuBind()
        {
            #region MyRegion
            LoadingLine.Visibility = Visibility.Visible;
            NoDataText.Visibility = Visibility.Collapsed;
            var selectDataBase = HidSelectDatabase.Text;
            var selectConnection = SelectedConnection;
            var menuData = new Model();
            Task.Run(() =>
            {
                var sqLiteHelper = new SQLiteHelper();
                var isContainsObjName = sqLiteHelper.GetSysBool(SysConst.Sys_IsContainsObjName);

                var itemParentList = new List<TreeNodeItem>();
                itemList = new List<TreeNodeItem>();

                var model = new Model();
                try
                {
                    var dbInstance = ExporterFactory.CreateInstance(selectConnection.DbType, selectConnection.SelectedDbConnectString(selectDataBase), selectDataBase);
                    model = dbInstance.Init();
                    menuData = model;
                }
                catch (Exception ex)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Oops.God($"连接失败 {selectConnection.ConnectName}，原因：" + ex.ToMsg());
                    }));
                }
                #region 表
                var listKey = new List<string>();
                foreach (var table in model.Tables)
                {
                    var key = table.Key;
                    string[] parts = key.Split(':');
                    var nodeTable = new TreeNodeItem();
                    AddNode(itemList, nodeTable, parts, 0);
                }
                #endregion


                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    #region MyRegion
                    LoadingLine.Visibility = Visibility.Hidden;
                    if (!itemList.Any())
                    {
                        NoDataAreaText.TipText = "暂无数据";
                        NoDataText.Visibility = Visibility.Visible;
                    }
                    itemList.ForEach(obj =>
                    {
                        obj.Icon = SysConst.Sys_FLODERICON;
                        if (!obj.Children.Any())
                        {
                            obj.Icon = SysConst.Sys_KEYICON;
                            //obj.Visibility = nameof(Visibility.Collapsed);
                        }
                        //obj.DisplayName += $"（{obj.Children.Count}）";
                        obj.ChildrenCount = obj.Children.Count;
                    });
                    TreeViewData = itemList;
                    SearchMenu.Text = string.Empty;

                    MenuData = menuData;
                    #endregion
                }));
            });
            #endregion
        }

        /// <summary>
        /// Key树形菜单
        /// </summary>
        /// <param name="treeNodes"></param>
        /// <param name="node"></param>
        /// <param name="parts"></param>
        /// <param name="index"></param>
        static void AddNode(List<TreeNodeItem> treeNodes, TreeNodeItem node, string[] parts, int index)
        {
            #region MyRegion
            if (index >= parts.Length)
                return;
            string currentValue = parts[index];
            if (index == 0)
            {
                var fnode = treeNodes.FirstOrDefault(x => x.Name == currentValue && x.Children.Count == 0);
                var cnode = treeNodes.FirstOrDefault(x => x.Name == currentValue && x.Children.Count > 0);
                if ((fnode == null && parts.Length==1) || (cnode == null && parts.Length >0))
                {
                    node.Name =  currentValue;
                    node.DisplayName = currentValue;
                    treeNodes.Add(node);
                    AddNode(treeNodes, node, parts, index + 1);
                }
                else
                {
                    cnode.Type = ObjType.Group;
                    AddNode(treeNodes, cnode, parts, index + 1);
                }
            }
            else
            {
                if (currentValue == parts[parts.Length -1])
                {
                    var dd = string.Join(":", parts);
                    var childNode = new TreeNodeItem
                    {
                        Name =  dd,
                        DisplayName= dd,
                        Icon = SysConst.Sys_KEYICON
                    };
                    node.Children.Add(childNode);
                }
                else
                {
                    TreeNodeItem childNode = node.Children.FirstOrDefault(c => c.DisplayName == currentValue);
                    if (childNode == null)
                    {
                        childNode = new TreeNodeItem
                        {
                            Name =  currentValue,
                            DisplayName= currentValue,
                            Type = ObjType.Group,
                            Icon = SysConst.Sys_FLODERICON
                        };
                        node.Children.Add(childNode);
                    }
                    AddNode(treeNodes, childNode, parts, index + 1);
                }
            }
            #endregion
        }

        /// <summary>
        /// 刷新菜单列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnFresh_OnClick(object sender, RoutedEventArgs e)
        {
            if (SelectedConnection == null)
            {
                return;
            }
            var searchText = SearchMenu.Text.Trim();
            if (!string.IsNullOrEmpty(searchText))
            {
                SearchMenuBind();
                return;
            }
            MenuBind();
        }

        /// <summary>
        /// 新建查询
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnQuery_Click(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            var selectConnection = SelectedConnection;
            var selectDatabase = (DataBase)ComSelectDatabase.SelectedItem;
            if (selectConnection == null || selectDatabase == null)
            {
                Oops.Oh("请选择数据库");
                return;
            }
            #endregion
        }

        ///// <summary>
        ///// 左侧菜单动态实时搜索
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void SearchMenu_OnTextChanged(object sender, TextChangedEventArgs e)
        //{
        //    if (SelectedConnection == null)
        //    {
        //        return;
        //    }
        //    SearchMenuBind();
        //}

        /// <summary>
        /// 搜索菜单绑定
        /// </summary>
        private void SearchMenuBind()
        {
            #region MyRegion
            NoDataText.Visibility = Visibility.Collapsed;
            itemList = new List<TreeNodeItem>();
            var searchText = SearchMenu.Text.ToLower().Trim();
            var nodeTable = new TreeNodeItem()
            {
                ObejcetId = "0",
                DisplayName = "表",
                Name = "treeTable",
                Icon = SysConst.Sys_TABLEICON,
                Type = ObjType.Type,
                IsExpanded = true,
                IsShowCount = Visibility.Visible
            };
            itemList.Add(nodeTable);
            var sqLiteHelper = new SQLiteHelper();
            var leftMenuType = sqLiteHelper.GetSysInt(SysConst.Sys_LeftMenuType);
            var isLikeSearch = sqLiteHelper.GetSysBool(SysConst.Sys_IsLikeSearch);
            var isContainsObjName = sqLiteHelper.GetSysBool(SysConst.Sys_IsContainsObjName);
            var selectDataBase = HidSelectDatabase.Text;
            var selectConnection = SelectedConnection;
            var itemParentList = new List<TreeNodeItem>();

            #region 表
            if (MenuData.Tables != null)
            {
                foreach (var table in MenuData.Tables)
                {
                    var isStartWith = !table.Key.ToLower().StartsWith(searchText, true, null) &&
                                     !table.Value.Name.ToLower().StartsWith(searchText, true, null);
                    var isContains = !table.Key.ToLower().Contains(searchText) && !table.Value.Name.ToLower().Contains(searchText);
                    var isSearchMode = isLikeSearch ? isContains : isStartWith;
                    if (isSearchMode)
                    {
                        continue;
                    }
                    var isShowComment = !isContainsObjName
                        ? Visibility.Collapsed
                        : Visibility.Visible;
                    var tableItem = new TreeNodeItem()
                    {
                        ObejcetId = table.Value.Id,
                        DisplayName = table.Value.DisplayName,
                        Name = table.Value.Name,
                        Schema = table.Value.SchemaName,
                        Comment = table.Value.Comment,
                        CreateDate = table.Value.CreateDate,
                        ModifyDate = table.Value.ModifyDate,
                        Icon = SysConst.Sys_TABLEICON,
                        Type = ObjType.Table,
                        IsShowComment = isShowComment
                    };
                    nodeTable.Children.Add(tableItem);
                }
            }
            #endregion

            itemList.ForEach(obj =>
            {
                if (!obj.Children.Any())
                {
                    obj.Visibility = nameof(Visibility.Collapsed);
                }
                //obj.DisplayName = $"{obj.DisplayName}({obj.Children.Count})";
                obj.ChildrenCount = obj.Children.Count;
            });
            if (itemList.All(x => x.Visibility != nameof(Visibility.Visible)))
            {
                NoDataAreaText.TipText = "暂无数据";
                NoDataText.Visibility = Visibility.Visible;
            }
            TreeViewData = itemList;

            #endregion
        }

        /// <summary>
        /// 选中表加载主内容对应数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectedTable_OnClick(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            var selectDatabase = (DataBase)ComSelectDatabase.SelectedItem;
            if (!(TreeViewTables.SelectedItem is TreeNodeItem objects) || objects.Type == ObjType.Group)
            {
                return;
            }
            MainTabW.Visibility = Visibility.Visible;
            if (TabItemData.Count > 0)
            {
                var curItem = TabItemData.FirstOrDefault();
                TabItemData.Remove(curItem);
            }
            var mainW = new UcRedisFrom
            {
                SelectedConnection = SelectedConnection,
                SelectedDataBase = selectDatabase,
                SelectedObject = objects
            };
            var tabItem = new MainTabWModel
            {
                DisplayName = objects.DisplayName,
                Icon = IconDic[objects.Type],
                MainW = mainW
            };
            TabItemData.Insert(0, tabItem);
            MainTabW.SelectedItem = TabItemData.First();
            #endregion
        }

        /// <summary>
        /// 子窗体刷新左侧菜单
        /// </summary>
        public void ChangeRefreshMenuEvent()
        {
            //if (TabGroupData.IsSelected || TabTagData.IsSelected)
            //{
            //    MenuBind();
            //}
        }

        /// <summary>
        /// 禁止水平滚动条自动滚动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EventSetter_OnHandler(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;
        }

        /// <summary>
        /// 复制名称
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuCopyName_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(TreeViewTables.SelectedItem is TreeNodeItem selectedObjects) || selectedObjects.ObejcetId == "0")
            {
                return;
            }
            Clipboard.SetDataObject(selectedObjects.Name);
            Oops.Success("复制成功");
        }


        private void MainTabW_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            #region MyRegion
            if (!IsLoaded)
            {
                return;
            }
            MainTabW.Visibility = Visibility.Visible;
            if (MainTabW.Items.Count == 0)
            {
                MainTabW.Visibility = Visibility.Collapsed;
            }
            #endregion
        }

        /// <summary>
        /// 右键菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EventSetter_OnHandler(object sender, MouseButtonEventArgs e)
        {
            #region MyRegion
            if (VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject) is TreeViewItem treeViewItem)
            {
                var selectedNode = treeViewItem.Header as TreeNodeItem;
                if (selectedNode == null)
                {
                    return;
                }
                treeViewItem.Focus();
                e.Handled = true;
            }
            #endregion
        }

        private DependencyObject VisualUpwardSearch<T>(DependencyObject source)
        {
            while (source != null && source.GetType() != typeof(T))
            {
                source = VisualTreeHelper.GetParent(source);
            }
            return source;
        }
    }
}
