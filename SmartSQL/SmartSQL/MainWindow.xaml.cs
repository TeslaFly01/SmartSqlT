using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HandyControl.Controls;
using HandyControl.Data;
using SmartSQL.Framework.PhysicalDataModel;
using SmartSQL.Models;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using SmartSQL.Framework;
using SmartSQL.Framework.SqliteModel;
using SmartSQL.Annotations;
using SmartSQL.DocUtils;
using SmartSQL.Framework.Const;
using SmartSQL.Helper;
using SmartSQL.UserControl;
using SmartSQL.Views;
using ComboBox = System.Windows.Controls.ComboBox;
using TabControl = System.Windows.Controls.TabControl;
using TabItem = System.Windows.Controls.TabItem;

namespace SmartSQL
{
    public partial class MainWindow : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private static readonly string GROUPICON = "pack://application:,,,/Resources/svg/category.svg";
        private static readonly string TABLEICON = "pack://application:,,,/Resources/svg/table.svg";
        private static readonly string VIEWICON = "pack://application:,,,/Resources/svg/view.svg";
        private static readonly string PROCICON = "pack://application:,,,/Resources/svg/proc.svg";
        private ConnectConfigs SelectendConnection = null;

        private Model dataSource = new Model();
        private List<TreeNodeItem> itemList = new List<TreeNodeItem>();

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            "CornerRadius", typeof(int), typeof(MainWindow), new PropertyMetadata(default(int)));

        public int CornerRadius
        {
            get => (int)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        #region TreeViewData
        public static readonly DependencyProperty TreeViewDataProperty = DependencyProperty.Register(
            "TreeViewData", typeof(List<TreeNodeItem>), typeof(MainWindow), new PropertyMetadata(default(List<TreeNodeItem>)));
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

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            LoadConnects();
        }

        public void LoadConnects()
        {
            var sqLiteHelper = new SQLiteHelper();
            var connectConfigs = sqLiteHelper.ToList<ConnectConfigs>();
            SwitchMenu.ItemsSource = null;
            SwitchMenu.ItemsSource = connectConfigs;
            CbTargetConnect.ItemsSource = connectConfigs;
            var leftMenuType = sqLiteHelper.GetSysInt(SysConst.Sys_LeftMenuType);
            TabLeftType.SelectedIndex = leftMenuType - 1;
            var isMultipleTab = sqLiteHelper.GetSysBool(SysConst.Sys_IsMultipleTab);
            CornerRadius = isMultipleTab ? 0 : 10;

            MainConnect.ConnectConfigs = connectConfigs;
            //var selectedConn = sqLiteHelper.GetSysString(SysConst.Sys_SelectedConnection);
            //if (!string.IsNullOrWhiteSpace(selectedConn))
            //{
            //    var connectConfig = sqLiteHelper.FirstOrDefault<ConnectConfigs>(x => x.ConnectName.Equals(selectedConn));
            //    SwitchConnect(connectConfig);
            //}
            MainTabW.DataContext = TabItemData;
            MainTabW.SetBinding(ItemsControl.ItemsSourceProperty, new Binding());
        }

        /// <summary>
        /// 切换数据库连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SwitchMenu_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var connectConfig = (ConnectConfigs)menuItem.DataContext;
            SwitchConnect(connectConfig);
        }

        /// <summary>
        /// 切换连接
        /// </summary>
        /// <param name="connectConfig"></param>
        public void SwitchConnect(ConnectConfigs connectConfig)
        {
            LoadingLine.Visibility = Visibility.Visible;
            SwitchMenu.Header = connectConfig.ConnectName;
            SelectendConnection = connectConfig;
            try
            {
                var dbInstance = ExporterFactory.CreateInstance(connectConfig.DbType, connectConfig.DbMasterConnectString);
                var list = dbInstance.GetDatabases();
                SelectDatabase.ItemsSource = list;
                HidSelectDatabase.Text = connectConfig.DefaultDatabase;
                SelectDatabase.SelectedItem = list.FirstOrDefault(x => x.DbName == connectConfig.DefaultDatabase);

                var sqLiteHelper = new SQLiteHelper();
                sqLiteHelper.SetSysValue(SysConst.Sys_SelectedConnection, connectConfig.ConnectName);
                var connectConfigs = sqLiteHelper.ToList<ConnectConfigs>();
                SwitchMenu.ItemsSource = null;
                SwitchMenu.ItemsSource = connectConfigs;
                CbTargetConnect.ItemsSource = connectConfigs;
            }
            catch (Exception ex)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    Growl.Warning(new GrowlInfo { Message = $"连接失败 {connectConfig.ConnectName}，原因：" + ex.ToMsg(), ShowDateTime = false, Type = InfoType.Error });
                    LoadingLine.Visibility = Visibility.Collapsed;
                }));
            }
        }

        /// <summary>
        /// 选择数据库发生变更
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SelectDatabase_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }
            var selectDatabase = SelectDatabase.SelectedItem;
            if (selectDatabase != null)
            {
                var selectedDbBase = (DataBase)selectDatabase;
                HidSelectDatabase.Text = ((DataBase)selectDatabase).DbName;
                var sqLiteHelper = new SQLiteHelper();
                sqLiteHelper.SetSysValue(SysConst.Sys_SelectedDataBase, selectedDbBase.DbName);
                MenuBind(false, null);
            }
        }

        public void MenuBind(bool isCompare, Model compareData)
        {
            #region MyRegion
            LoadingLine.Visibility = Visibility.Visible;
            NoDataText.Visibility = Visibility.Collapsed;
            /////TreeViewTables.ItemsSource = null;
            var selectDataBase = HidSelectDatabase.Text;
            var selectConnection = SelectendConnection;
            Task.Run(() =>
            {
                var sqLiteHelper = new SQLiteHelper();
                var leftMenuType = sqLiteHelper.GetSysInt(SysConst.Sys_LeftMenuType);
                var curObjects = new List<SObjectDTO>();
                var curGroups = new List<ObjectGroup>();
                var itemParentList = new List<TreeNodeItem>();
                itemList = new List<TreeNodeItem>();
                var nodeTable = new TreeNodeItem
                {
                    ObejcetId = "0",
                    DisplayName = "数据表",
                    Name = "treeTable",
                    Icon = TABLEICON,
                    Type = ObjType.Type
                };
                itemList.Add(nodeTable);
                var nodeView = new TreeNodeItem
                {
                    ObejcetId = "0",
                    DisplayName = "视图",
                    Name = "treeView",
                    Icon = VIEWICON,
                    Type = ObjType.Type
                };
                itemList.Add(nodeView);
                var nodeProc = new TreeNodeItem
                {
                    ObejcetId = "0",
                    DisplayName = "存储过程",
                    Name = "treeProc",
                    Icon = PROCICON,
                    Type = ObjType.Type
                };
                itemList.Add(nodeProc);

                #region 分组业务处理
                //是否业务分组
                if (leftMenuType == LeftMenuType.Group.GetHashCode())
                {
                    curGroups = sqLiteHelper.db.Table<ObjectGroup>().Where(a =>
                        a.ConnectId == selectConnection.ID &&
                        a.DataBaseName == selectDataBase).OrderBy(x => x.OrderFlag).ToList();
                    if (curGroups.Any())
                    {
                        foreach (var group in curGroups)
                        {
                            var itemChildList = new List<TreeNodeItem>();
                            var nodeGroup = new TreeNodeItem
                            {
                                ObejcetId = "0",
                                DisplayName = group.GroupName,
                                Name = "treeGroup",
                                Icon = GROUPICON,
                                FontWeight = "Bold",
                                Type = ObjType.Group,
                                IsExpanded = !(!group.OpenLevel.HasValue || group.OpenLevel == 0)
                            };
                            var nodeTable1 = new TreeNodeItem
                            {
                                ObejcetId = "0",
                                DisplayName = "数据表",
                                Name = "treeTable",
                                Icon = TABLEICON,
                                Parent = nodeGroup,
                                Type = ObjType.Type,
                                IsExpanded = group.OpenLevel == 2
                            };
                            itemChildList.Add(nodeTable1);
                            var nodeView1 = new TreeNodeItem
                            {
                                ObejcetId = "0",
                                DisplayName = "视图",
                                Name = "treeView",
                                Icon = VIEWICON,
                                Parent = nodeGroup,
                                Type = ObjType.Type,
                                IsExpanded = group.OpenLevel == 2
                            };
                            itemChildList.Add(nodeView1);
                            var nodeProc1 = new TreeNodeItem
                            {
                                ObejcetId = "0",
                                DisplayName = "存储过程",
                                Name = "treeProc",
                                Icon = PROCICON,
                                Parent = nodeGroup,
                                Type = ObjType.Type,
                                IsExpanded = group.OpenLevel == 2
                            };
                            itemChildList.Add(nodeProc1);
                            nodeGroup.Children = itemChildList;
                            itemParentList.Add(nodeGroup);
                        }
                    }
                    curObjects = (from a in sqLiteHelper.db.Table<ObjectGroup>()
                                  join b in sqLiteHelper.db.Table<SObjects>() on a.Id equals b.GroupId
                                  where a.ConnectId == selectConnection.ID &&
                                        a.DataBaseName == selectDataBase
                                  select new SObjectDTO
                                  {
                                      GroupName = a.GroupName,
                                      ObjectName = b.ObjectName
                                  }).ToList();
                }
                #endregion
                var model = new Model();
                try
                {
                    var dbInstance = ExporterFactory.CreateInstance(selectConnection.DbType, selectConnection.SelectedDbConnectString(selectDataBase));
                    model = dbInstance.Init();
                    dataSource = model;
                }
                catch (Exception ex)
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Growl.Warning(new GrowlInfo
                        {
                            Message = $"连接失败 {selectConnection.ConnectName}，原因：" + ex.ToMsg(),
                            ShowDateTime = false,
                            Type = InfoType.Error
                        });
                    }));
                }
                var textColor = "#333444";
                #region 数据表
                foreach (var table in model.Tables)
                {
                    if (isCompare)
                    {
                        var tableAny = compareData.Tables.Any(x => x.Key.Equals(table.Key));
                        if (!tableAny)
                        {
                            textColor = "Blue";
                            nodeTable.Children.Add(new TreeNodeItem()
                            {
                                ObejcetId = table.Value.Id,
                                DisplayName = table.Value.DisplayName,
                                Name = table.Value.Name,
                                Schema = table.Value.SchemaName,
                                Comment = table.Value.Comment,
                                CreateDate = table.Value.CreateDate,
                                ModifyDate = table.Value.ModifyDate,
                                TextColor = textColor,
                                Icon = TABLEICON,
                                Type = ObjType.Table
                            });
                        }
                    }
                    else
                    {
                        //是否业务分组
                        if (leftMenuType == LeftMenuType.Group.GetHashCode())
                        {
                            var hasGroup = curObjects.Where(x => x.ObjectName == table.Key).
                                GroupBy(x => x.GroupName).Select(x => x.Key)
                                .ToList();
                            foreach (var group in hasGroup)
                            {
                                var pGroup = itemParentList.FirstOrDefault(x => x.DisplayName == group);
                                if (pGroup != null)
                                {
                                    var ppGroup = pGroup.Children.FirstOrDefault(x => x.DisplayName == "数据表");
                                    if (ppGroup != null)
                                    {
                                        ppGroup.Children.Add(new TreeNodeItem()
                                        {
                                            ObejcetId = table.Value.Id,
                                            DisplayName = table.Value.DisplayName,
                                            Name = table.Value.Name,
                                            Schema = table.Value.SchemaName,
                                            Comment = table.Value.Comment,
                                            CreateDate = table.Value.CreateDate,
                                            ModifyDate = table.Value.ModifyDate,
                                            TextColor = textColor,
                                            Icon = TABLEICON,
                                            Type = ObjType.Table
                                        });
                                    }
                                }
                            }
                        }
                        else
                        {
                            nodeTable.Children.Add(new TreeNodeItem()
                            {
                                ObejcetId = table.Value.Id,
                                DisplayName = table.Value.DisplayName,
                                Name = table.Value.Name,
                                Schema = table.Value.SchemaName,
                                Comment = table.Value.Comment,
                                CreateDate = table.Value.CreateDate,
                                ModifyDate = table.Value.ModifyDate,
                                TextColor = textColor,
                                Icon = TABLEICON,
                                Type = ObjType.Table
                            });
                        }
                    }
                }
                #endregion

                #region 视图
                foreach (var view in model.Views)
                {
                    if (isCompare)
                    {
                        var viewAny = compareData.Views.Any(x => x.Key.Equals(view.Key));
                        if (!viewAny)
                        {
                            textColor = "Blue";
                            nodeView.Children.Add(new TreeNodeItem()
                            {
                                ObejcetId = view.Value.Id,
                                DisplayName = view.Value.DisplayName,
                                Name = view.Value.Name,
                                Schema = view.Value.SchemaName,
                                Comment = view.Value.Comment,
                                CreateDate = view.Value.CreateDate,
                                ModifyDate = view.Value.ModifyDate,
                                TextColor = textColor,
                                Icon = VIEWICON,
                                Type = ObjType.View
                            });
                        }
                    }
                    else
                    {
                        //是否业务分组
                        if (leftMenuType == LeftMenuType.Group.GetHashCode())
                        {
                            var hasGroup = curObjects.Where(x => x.ObjectName == view.Key).
                                GroupBy(x => x.GroupName).Select(x => x.Key)
                                .ToList();
                            foreach (var group in hasGroup)
                            {
                                var pGroup = itemParentList.FirstOrDefault(x => x.DisplayName == group);
                                if (pGroup != null)
                                {
                                    var ppGroup = pGroup.Children.FirstOrDefault(x => x.DisplayName == "视图");
                                    if (ppGroup != null)
                                    {
                                        ppGroup.Children.Add(new TreeNodeItem()
                                        {
                                            ObejcetId = view.Value.Id,
                                            DisplayName = view.Value.DisplayName,
                                            Name = view.Value.Name,
                                            Schema = view.Value.SchemaName,
                                            Comment = view.Value.Comment,
                                            CreateDate = view.Value.CreateDate,
                                            ModifyDate = view.Value.ModifyDate,
                                            TextColor = textColor,
                                            Icon = VIEWICON,
                                            Type = ObjType.View
                                        });
                                    }
                                }
                            }
                        }
                        else
                        {
                            nodeView.Children.Add(new TreeNodeItem()
                            {
                                ObejcetId = view.Value.Id,
                                DisplayName = view.Value.DisplayName,
                                Name = view.Value.Name,
                                Schema = view.Value.SchemaName,
                                Comment = view.Value.Comment,
                                CreateDate = view.Value.CreateDate,
                                ModifyDate = view.Value.ModifyDate,
                                TextColor = textColor,
                                Icon = VIEWICON,
                                Type = ObjType.View
                            });
                        }
                    }
                }
                #endregion

                #region 存储过程
                foreach (var proc in model.Procedures)
                {
                    if (isCompare)
                    {
                        var procAny = compareData.Procedures.Any(x => x.Key.Equals(proc.Key));
                        if (!procAny)
                        {
                            textColor = "Blue";
                            nodeProc.Children.Add(new TreeNodeItem()
                            {
                                ObejcetId = proc.Value.Id,
                                DisplayName = proc.Value.DisplayName,
                                Name = proc.Value.Name,
                                Schema = proc.Value.SchemaName,
                                Comment = proc.Value.Comment,
                                CreateDate = proc.Value.CreateDate,
                                ModifyDate = proc.Value.ModifyDate,
                                TextColor = textColor,
                                Icon = PROCICON,
                                Type = ObjType.Proc
                            });
                        }
                    }
                    else
                    {
                        //是否业务分组
                        if (leftMenuType == LeftMenuType.Group.GetHashCode())
                        {
                            var hasGroup = curObjects.Where(x => x.ObjectName == proc.Key).GroupBy(x => x.GroupName)
                                .Select(x => x.Key)
                                .ToList();
                            foreach (var group in hasGroup)
                            {
                                var pGroup = itemParentList.FirstOrDefault(x => x.DisplayName == group);
                                if (pGroup != null)
                                {
                                    var ppGroup = pGroup.Children.FirstOrDefault(x => x.DisplayName == "存储过程");
                                    if (ppGroup != null)
                                    {
                                        ppGroup.Children.Add(new TreeNodeItem()
                                        {
                                            ObejcetId = proc.Value.Id,
                                            DisplayName = proc.Value.DisplayName,
                                            Name = proc.Value.Name,
                                            Schema = proc.Value.SchemaName,
                                            Comment = proc.Value.Comment,
                                            CreateDate = proc.Value.CreateDate,
                                            ModifyDate = proc.Value.ModifyDate,
                                            TextColor = textColor,
                                            Icon = PROCICON,
                                            Type = ObjType.Proc
                                        });
                                    }
                                }
                            }
                        }
                        else
                        {
                            nodeProc.Children.Add(new TreeNodeItem()
                            {
                                ObejcetId = proc.Value.Id,
                                DisplayName = proc.Value.DisplayName,
                                Name = proc.Value.Name,
                                Schema = proc.Value.SchemaName,
                                Comment = proc.Value.Comment,
                                CreateDate = proc.Value.CreateDate,
                                ModifyDate = proc.Value.ModifyDate,
                                TextColor = textColor,
                                Icon = PROCICON,
                                Type = ObjType.Proc
                            });
                        }
                    }
                }
                #endregion

                #region 数据比较
                if (isCompare)
                {
                    foreach (var compareTable in compareData.Tables)
                    {
                        if (!model.Tables.Any(x => x.Key.Equals(compareTable.Key)))
                        {
                            textColor = "Red";
                            nodeTable.Children.Add(new TreeNodeItem
                            {
                                ObejcetId = compareTable.Value.Id,
                                DisplayName = compareTable.Value.DisplayName,
                                Name = compareTable.Value.Name,
                                Schema = compareTable.Value.SchemaName,
                                TextColor = textColor,
                                Icon = TABLEICON,
                                Type = ObjType.Table
                            });
                        }
                    }
                    foreach (var compareView in compareData.Views)
                    {
                        if (!model.Views.Any(x => x.Key.Equals(compareView.Key)))
                        {
                            textColor = "Red";
                            nodeView.Children.Add(new TreeNodeItem()
                            {
                                ObejcetId = compareView.Value.Id,
                                DisplayName = compareView.Value.DisplayName,
                                Name = compareView.Value.Name,
                                Schema = compareView.Value.SchemaName,
                                TextColor = textColor,
                                Icon = VIEWICON,
                                Type = ObjType.View
                            });
                        }
                    }
                    foreach (var compareProc in compareData.Procedures)
                    {
                        if (!model.Procedures.Any(x => x.Key.Equals(compareProc.Key)))
                        {
                            textColor = "Red";
                            nodeProc.Children.Add(new TreeNodeItem()
                            {
                                ObejcetId = compareProc.Value.Id,
                                DisplayName = compareProc.Value.DisplayName,
                                Name = compareProc.Value.Name,
                                Schema = compareProc.Value.SchemaName,
                                TextColor = textColor,
                                Icon = VIEWICON,
                                Type = ObjType.View
                            });
                        }
                    }
                }
                #endregion

                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    LoadingLine.Visibility = Visibility.Hidden;
                    //编写获取数据并显示在界面的代码
                    //是否业务分组
                    if (leftMenuType == LeftMenuType.Group.GetHashCode() && !isCompare)
                    {
                        if (!itemParentList.Any())
                        {
                            NoDataAreaText.TipText = "暂无分组，请先建分组";
                            NoDataText.Visibility = Visibility.Visible;
                        }
                        itemParentList.ForEach(group =>
                        {
                            group.Children.ForEach(obj =>
                            {
                                if (!obj.Children.Any())
                                {
                                    obj.Visibility = nameof(Visibility.Collapsed);
                                }
                                obj.DisplayName += $"（{obj.Children.Count}）";
                            });
                        });
                        TreeViewData = itemParentList;
                        SearchMenu.Text = string.Empty;
                    }
                    else
                    {
                        if (!itemList.Any(x => x.Children.Count > 0))
                        {
                            NoDataAreaText.TipText = "暂无数据";
                            if (isCompare)
                            {
                                NoDataAreaText.TipText = "暂无差异数据";
                            }
                            NoDataText.Visibility = Visibility.Visible;
                        }
                        itemList.ForEach(obj =>
                        {
                            if (!obj.Children.Any())
                            {
                                obj.Visibility = nameof(Visibility.Collapsed);
                            }
                            obj.DisplayName += $"（{obj.Children.Count}）";
                        });
                        TreeViewData = itemList;
                        SearchMenu.Text = string.Empty;
                    }
                }));
            });
            #endregion
        }

        /// <summary>
        /// 菜单类型变更事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabLeftType_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var sqLiteHelper = new SQLiteHelper();
            var selectedItem = (TabItem)((TabControl)sender).SelectedItem;
            if (selectedItem.Name == "TabAllData")
            {
                sqLiteHelper.SetSysValue(SysConst.Sys_LeftMenuType, "1");
            }
            else if (selectedItem.Name == "TabGroupData")
            {
                sqLiteHelper.SetSysValue(SysConst.Sys_LeftMenuType, "2");
            }
            else
            {
                sqLiteHelper.SetSysValue(SysConst.Sys_LeftMenuType, "3");
            }
            if (SelectendConnection == null)
            {
                return;
            }
            if (!string.IsNullOrEmpty(SearchMenu.Text))
            {
                SearchMenuBind();
            }
            else
            {
                MenuBind(false, null);
            }
        }

        /// <summary>
        /// 左侧菜单动态实时搜索
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchMenu_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            SearchMenuBind();
        }

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
                DisplayName = "数据表",
                Name = "treeTable",
                Icon = TABLEICON,
                Type = ObjType.Type,
                IsExpanded = true
            };
            itemList.Add(nodeTable);
            var nodeView = new TreeNodeItem()
            {
                ObejcetId = "0",
                DisplayName = "视图",
                Name = "treeView",
                Icon = VIEWICON,
                Type = ObjType.Type,
                IsExpanded = true
            };
            itemList.Add(nodeView);
            var nodeProc = new TreeNodeItem()
            {
                ObejcetId = "0",
                DisplayName = "存储过程",
                Name = "treeProc",
                Icon = PROCICON,
                Type = ObjType.Type,
                IsExpanded = true
            };
            itemList.Add(nodeProc);
            var sqLiteHelper = new SQLiteHelper();
            var leftMenuType = sqLiteHelper.GetSysInt(SysConst.Sys_LeftMenuType);
            var isLikeSearch = sqLiteHelper.GetSysBool(SysConst.Sys_IsLikeSearch);
            var selectDataBase = HidSelectDatabase.Text;
            var selectConnection = SelectendConnection;
            var currObjects = new List<SObjectDTO>();
            var currGroups = new List<ObjectGroup>();
            var itemParentList = new List<TreeNodeItem>();
            #region 分组业务处理
            if (leftMenuType == LeftMenuType.Group.GetHashCode())
            {
                currGroups = sqLiteHelper.db.Table<ObjectGroup>().Where(a =>
                    a.ConnectId == selectConnection.ID &&
                    a.DataBaseName == selectDataBase).OrderBy(x => x.OrderFlag).ToList();
                if (!currGroups.Any())
                {
                    NoDataAreaText.TipText = "暂无分组，请先建分组";
                    NoDataText.Visibility = Visibility.Visible;
                    return;
                }
                foreach (var group in currGroups)
                {
                    var itemChildList = new List<TreeNodeItem>();
                    var nodeGroup = new TreeNodeItem
                    {
                        ObejcetId = "0",
                        DisplayName = group.GroupName,
                        Name = "treeTable",
                        Icon = GROUPICON,
                        Type = ObjType.Group,
                        IsExpanded = true,
                        FontWeight = "Bold",
                        Children = itemChildList
                    };
                    var nodeTable1 = new TreeNodeItem
                    {
                        ObejcetId = "0",
                        DisplayName = "数据表",
                        Name = "treeTable",
                        Icon = TABLEICON,
                        Type = ObjType.Type,
                        IsExpanded = true,
                        Parent = nodeGroup
                    };
                    itemChildList.Add(nodeTable1);
                    var nodeView1 = new TreeNodeItem
                    {
                        ObejcetId = "0",
                        DisplayName = "视图",
                        Name = "treeView",
                        Icon = VIEWICON,
                        Type = ObjType.Type,
                        IsExpanded = true,
                        Parent = nodeGroup
                    };
                    itemChildList.Add(nodeView1);
                    var nodeProc1 = new TreeNodeItem
                    {
                        ObejcetId = "0",
                        DisplayName = "存储过程",
                        Name = "treeProc",
                        Icon = PROCICON,
                        Type = ObjType.Type,
                        IsExpanded = true,
                        Parent = nodeGroup
                    };
                    itemChildList.Add(nodeProc1);
                    itemParentList.Add(nodeGroup);
                }
                currObjects = (from a in sqLiteHelper.db.Table<ObjectGroup>()
                               join b in sqLiteHelper.db.Table<SObjects>() on a.Id equals b.GroupId
                               where a.ConnectId == selectConnection.ID &&
                                     a.DataBaseName == selectDataBase
                               select new SObjectDTO
                               {
                                   GroupName = a.GroupName,
                                   ObjectName = b.ObjectName
                               }).ToList();
            }
            #endregion

            #region 数据表
            if (dataSource.Tables != null)
            {
                foreach (var table in dataSource.Tables)
                {
                    var isStartWith = !table.Key.ToLower().StartsWith(searchText, true, null) &&
                                     !table.Value.Name.ToLower().StartsWith(searchText, true, null);
                    var isContains = !table.Key.ToLower().Contains(searchText) && !table.Key.ToLower().Contains(searchText);
                    var isSearchMode = isLikeSearch ? isContains : isStartWith;
                    if (isSearchMode)
                    {
                        continue;
                    }
                    //是否业务分组
                    if (leftMenuType == LeftMenuType.Group.GetHashCode())
                    {
                        var hasGroup = currObjects.Where(x => x.ObjectName == table.Key).
                            GroupBy(x => x.GroupName).Select(x => x.Key)
                            .ToList();
                        foreach (var group in hasGroup)
                        {
                            var pGroup = itemParentList.FirstOrDefault(x => x.DisplayName == group);
                            if (pGroup != null)
                            {
                                var ppGroup = pGroup.Children.FirstOrDefault(x => x.DisplayName == "数据表");
                                if (ppGroup != null)
                                {
                                    ppGroup.Children.Add(new TreeNodeItem()
                                    {
                                        ObejcetId = table.Value.Id,
                                        DisplayName = table.Value.DisplayName,
                                        Name = table.Value.Name,
                                        Schema = table.Value.SchemaName,
                                        Comment = table.Value.Comment,
                                        CreateDate = table.Value.CreateDate,
                                        ModifyDate = table.Value.ModifyDate,
                                        Icon = TABLEICON,
                                        Type = ObjType.Table
                                    });
                                }
                            }
                        }
                    }
                    else
                    {
                        nodeTable.Children.Add(new TreeNodeItem()
                        {
                            ObejcetId = table.Value.Id,
                            DisplayName = table.Value.DisplayName,
                            Name = table.Value.Name,
                            Schema = table.Value.SchemaName,
                            Comment = table.Value.Comment,
                            CreateDate = table.Value.CreateDate,
                            ModifyDate = table.Value.ModifyDate,
                            Icon = TABLEICON,
                            Type = ObjType.Table
                        });
                    }
                }
            }
            #endregion

            #region 视图
            if (dataSource.Views != null)
            {
                foreach (var view in dataSource.Views)
                {
                    var isStartWith = !view.Key.ToLower().StartsWith(searchText, true, null) && !view.Value.Name.ToLower().StartsWith(searchText, true, null);
                    var isContains = !view.Key.ToLower().Contains(searchText) && !view.Key.ToLower().Contains(searchText);
                    var isSearchMode = isLikeSearch ? isContains : isStartWith;
                    if (isSearchMode)
                    {
                        continue;
                    }
                    //是否业务分组
                    if (leftMenuType == LeftMenuType.Group.GetHashCode())
                    {
                        var hasGroup = currObjects.Where(x => x.ObjectName == view.Key).
                            GroupBy(x => x.GroupName).Select(x => x.Key)
                            .ToList();
                        foreach (var group in hasGroup)
                        {
                            var pGroup = itemParentList.FirstOrDefault(x => x.DisplayName == group);
                            if (pGroup != null)
                            {
                                var ppGroup = pGroup.Children.FirstOrDefault(x => x.DisplayName == "视图");
                                if (ppGroup != null)
                                {
                                    ppGroup.Children.Add(new TreeNodeItem()
                                    {
                                        ObejcetId = view.Value.Id,
                                        DisplayName = view.Value.DisplayName,
                                        Name = view.Value.Name,
                                        Schema = view.Value.SchemaName,
                                        Comment = view.Value.Comment,
                                        CreateDate = view.Value.CreateDate,
                                        ModifyDate = view.Value.ModifyDate,
                                        Icon = VIEWICON,
                                        Type = ObjType.View
                                    });
                                }
                            }
                        }
                    }
                    else
                    {
                        nodeView.Children.Add(new TreeNodeItem()
                        {
                            ObejcetId = view.Value.Id,
                            DisplayName = view.Value.DisplayName,
                            Name = view.Value.Name,
                            Schema = view.Value.SchemaName,
                            Comment = view.Value.Comment,
                            CreateDate = view.Value.CreateDate,
                            ModifyDate = view.Value.ModifyDate,
                            Icon = VIEWICON,
                            Type = ObjType.View
                        });
                    }
                }
            }
            #endregion

            #region 存储过程
            if (dataSource.Procedures != null)
            {
                foreach (var proc in dataSource.Procedures)
                {
                    var isStartWith = !proc.Key.ToLower().StartsWith(searchText, true, null) && !proc.Value.Name.ToLower().StartsWith(searchText, true, null);
                    var isContains = !proc.Key.ToLower().Contains(searchText) && !proc.Key.ToLower().Contains(searchText);
                    var isSearchMode = isLikeSearch ? isContains : isStartWith;
                    if (isSearchMode)
                    {
                        continue;
                    }
                    //是否业务分组
                    if (leftMenuType == LeftMenuType.Group.GetHashCode())
                    {
                        var hasGroup = currObjects.Where(x => x.ObjectName == proc.Key).GroupBy(x => x.GroupName)
                            .Select(x => x.Key)
                            .ToList();
                        foreach (var group in hasGroup)
                        {
                            var pGroup = itemParentList.FirstOrDefault(x => x.DisplayName == group);
                            if (pGroup != null)
                            {
                                var ppGroup = pGroup.Children.FirstOrDefault(x => x.DisplayName == "存储过程");
                                if (ppGroup != null)
                                {
                                    ppGroup.Children.Add(new TreeNodeItem()
                                    {
                                        ObejcetId = proc.Value.Id,
                                        DisplayName = proc.Value.DisplayName,
                                        Name = proc.Value.Name,
                                        Schema = proc.Value.SchemaName,
                                        Comment = proc.Value.Comment,
                                        CreateDate = proc.Value.CreateDate,
                                        ModifyDate = proc.Value.ModifyDate,
                                        Icon = PROCICON,
                                        Type = ObjType.Proc
                                    });
                                }
                            }
                        }
                    }
                    else
                    {
                        nodeProc.Children.Add(new TreeNodeItem()
                        {
                            ObejcetId = proc.Value.Id,
                            DisplayName = proc.Value.DisplayName,
                            Name = proc.Value.Name,
                            Schema = proc.Value.SchemaName,
                            Comment = proc.Value.Comment,
                            CreateDate = proc.Value.CreateDate,
                            ModifyDate = proc.Value.ModifyDate,
                            Icon = PROCICON,
                            Type = ObjType.Proc
                        });
                    }
                }
            }
            #endregion

            if (leftMenuType == LeftMenuType.Group.GetHashCode())
            {
                itemParentList.ForEach(group =>
                {
                    if (!group.Children.First(x => x.Name.Equals("treeTable")).Children.Any() && !group.Children.First(x => x.Name.Equals("treeView")).Children.Any() && !group.Children.First(x => x.Name.Equals("treeProc")).Children.Any())
                    {
                        group.Visibility = nameof(Visibility.Collapsed);
                    }
                    group.Children.ForEach(obj =>
                    {
                        if (!obj.Children.Any())
                        {
                            obj.Visibility = nameof(Visibility.Collapsed);
                        }
                        obj.DisplayName = $"{obj.DisplayName}({obj.Children.Count})";
                    });
                });
                if (itemParentList.All(x => x.Visibility != nameof(Visibility.Visible)))
                {
                    NoDataAreaText.TipText = "暂无数据";
                    NoDataText.Visibility = Visibility.Visible;
                }
                TreeViewData = itemParentList;
            }
            else
            {
                itemList.ForEach(obj =>
                {
                    if (!obj.Children.Any())
                    {
                        obj.Visibility = nameof(Visibility.Collapsed);
                    }
                    obj.DisplayName = $"{obj.DisplayName}({obj.Children.Count})";
                });
                if (itemList.All(x => x.Visibility != nameof(Visibility.Visible)))
                {
                    NoDataAreaText.TipText = "暂无数据";
                    NoDataText.Visibility = Visibility.Visible;
                }
                TreeViewData = itemList;
            }
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
            var selectDatabase = (DataBase)SelectDatabase.SelectedItem;
            if (!(TreeViewTables.SelectedItem is TreeNodeItem objects) || objects.Type == ObjType.Group || objects.TextColor.Equals("Red"))
            {
                return;
            }
            var sqLiteHelper = new SQLiteHelper();
            var isMultipleTab = sqLiteHelper.GetSysBool(SysConst.Sys_IsMultipleTab);
            if (!isMultipleTab)
            {
                if (TabItemData.Any())
                {
                    TabItemData.Clear();
                }
                CornerRadius = 10;
                MainW.Visibility = Visibility.Visible;
                MainTabW.Visibility = Visibility.Collapsed;
                MainW.ObjChangeRefreshEvent += Group_ChangeRefreshEvent;
                MainW.SelectedConnection = SelectendConnection;
                MainW.SelectedDataBase = selectDatabase;
                MainW.SelectedObject = objects;
                MainW.LoadPage(TreeViewData);
                return;
            }
            CornerRadius = 0;
            MainW.Visibility = Visibility.Collapsed;
            MainTabW.Visibility = Visibility.Visible;
            var curItem = TabItemData.FirstOrDefault(x => x.DisplayName == objects.DisplayName);
            if (curItem != null)
            {
                MainTabW.SelectedItem = curItem;
                return;
            }

            var dic = new Dictionary<string, string>
            {
                {"Type", "pack://application:,,,/Resources/svg/category.svg"},
                {"Table", "pack://application:,,,/Resources/svg/table.svg"},
                {"View", "pack://application:,,,/Resources/svg/view.svg"},
                {"Proc", "pack://application:,,,/Resources/svg/proc.svg"}
            };
            var mainW = new MainW
            {
                SelectedConnection = SelectendConnection,
                SelectedDataBase = selectDatabase,
                SelectedObject = objects,
            };
            mainW.LoadPage(TreeViewData);
            var tabItem = new MainTabWModel
            {
                DisplayName = objects.DisplayName,
                Icon = dic[objects.Type],
                MainW = mainW
            };
            TabItemData.Insert(0, tabItem);
            MainTabW.SelectedItem = TabItemData.First();
            #endregion
        }

        /// <summary>
        /// 切换目标连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CbTargetConnect_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBoxItem = (ComboBox)sender;
            var dataBase = (ConnectConfigs)comboBoxItem.SelectedItem;
            if (dataBase == null)
            {
                return;
            }
            Task.Run(() =>
            {
                var dbInstance = ExporterFactory.CreateInstance(dataBase.DbType, dataBase.DbMasterConnectString);
                var list = dbInstance.GetDatabases();
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    CbTargetDatabase.ItemsSource = list;
                    CbTargetDatabase.DisplayMemberPath = "DbName";
                    list.ForEach(x =>
                    {
                        if (!(SelectDatabase.SelectedItem is DataBase selectDatabase)) return;
                        if (x.DbName == selectDatabase.DbName)
                        {
                            CbTargetDatabase.SelectedItem = x;
                        }
                    });
                }));
            });
        }

        /// <summary>
        /// 数据库比较
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCompare_OnClick(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            if (SelectendConnection == null)
            {
                Growl.Warning(new GrowlInfo { Message = $"请选择源数据库", WaitTime = 1, ShowDateTime = false });
                return;
            }
            var targetConnect = (ConnectConfigs)CbTargetConnect.SelectedItem;
            var targetData = (DataBase)CbTargetDatabase.SelectedItem;
            if (targetConnect == null || targetData == null || string.IsNullOrEmpty(targetData.DbName))
            {
                Growl.Warning(new GrowlInfo { Message = $"请选择目标数据库", WaitTime = 1, ShowDateTime = false });
                return;
            }
            var dbInstance = ExporterFactory.CreateInstance(targetConnect.DbType, targetConnect.SelectedDbConnectString(targetData.DbName));
            LoadingLine.Visibility = Visibility.Visible;

            var model = dbInstance.Init();
            MenuBind(true, model);

            #endregion
        }

        /// <summary>
        /// 关于SmartSQL
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuAbout_OnClick(object sender, RoutedEventArgs e)
        {
            var about = new AboutWindow();
            about.Owner = this;
            about.ShowDialog();
        }

        /// <summary>
        /// 分组管理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuGroup_OnClick(object sender, RoutedEventArgs e)
        {
            if (SelectendConnection == null)
            {
                Growl.Warning(new GrowlInfo { Message = $"请选择连接", WaitTime = 1, ShowDateTime = false });
                return;
            }
            var selectDatabase = (DataBase)SelectDatabase.SelectedItem;
            var group = new GroupManage();
            group.Connection = SelectendConnection;
            group.SelectedDataBase = selectDatabase.DbName;
            group.Owner = this;
            group.ChangeRefreshEvent += Group_ChangeRefreshEvent;
            group.ShowDialog();
        }

        /// <summary>
        /// 子窗体刷新左侧菜单
        /// </summary>
        public void Group_ChangeRefreshEvent()
        {
            if (TabGroupData.IsSelected)
            {
                MenuBind(false, null);
            }
        }

        private void MenuSelectedItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(TreeViewTables.SelectedItem is TreeNodeItem selectedObjects) || selectedObjects.ObejcetId == "0" || selectedObjects.TextColor.Equals("Red"))
            {
                return;
            }
            Clipboard.SetDataObject(selectedObjects.Name);
        }

        /// <summary>
        /// 刷新菜单列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnFresh_OnClick(object sender, RoutedEventArgs e)
        {
            if (SelectendConnection == null)
            {
                return;
            }
            var searchText = SearchMenu.Text.Trim();
            if (!string.IsNullOrEmpty(searchText))
            {
                SearchMenuBind();
                return;
            }
            MenuBind(false, null);
        }

        /// <summary>
        /// 全局设置
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuSetting_OnClick(object sender, RoutedEventArgs e)
        {
            var set = new SettingWindow();
            set.Owner = this;
            set.ShowDialog();
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
        /// 新建连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddConnect_OnClick(object sender, RoutedEventArgs e)
        {
            var connect = new ConnectManageExt();
            connect.Owner = this;
            connect.ChangeRefreshEvent += SwitchConnect;
            connect.ShowDialog();
        }

        /// <summary>
        /// 导出文档
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExportDoc_OnClick(object sender, RoutedEventArgs e)
        {
            var selectDatabase = (DataBase)SelectDatabase.SelectedItem;
            if (SelectendConnection == null || selectDatabase == null)
            {
                Growl.Warning(new GrowlInfo { Message = $"请选择数据库", WaitTime = 1, ShowDateTime = false });
                return;
            }
            var exportDoc = new ExportDocExt();
            exportDoc.Owner = this;
            exportDoc.SelectedConnection = SelectendConnection;
            exportDoc.SelectedDataBase = selectDatabase;
            exportDoc.ShowDialog();
        }

        /// <summary>
        /// 导入备注
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImportMark_OnClick(object sender, RoutedEventArgs e)
        {
            var selectDatabase = (DataBase)SelectDatabase.SelectedItem;
            if (SelectendConnection == null || selectDatabase == null)
            {
                Growl.Warning(new GrowlInfo { Message = $"请选择数据库", WaitTime = 1, ShowDateTime = false });
                return;
            }
            var importMark = new ImportMark();
            importMark.Owner = this;
            importMark.SelectedConnection = SelectendConnection;
            importMark.SelectedDataBase = selectDatabase;
            importMark.ShowDialog();
        }

        private void MainTabW_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }
            if (e.Source is HandyControl.Controls.TabControl)
            {
                MainTabW.ShowCloseButton = MainTabW.Items.Count > 1;
                MainTabW.ShowContextMenu = MainTabW.Items.Count > 1;
            }
        }
    }
}
