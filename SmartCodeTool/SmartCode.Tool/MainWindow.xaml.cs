using SmartCode.Framework.Exporter;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using HandyControl.Controls;
using HandyControl.Data;
using SmartCode.Framework.PhysicalDataModel;
using SmartCode.Tool.Models;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Configuration;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;
using HandyControlDemo.Window;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using Microsoft.Win32;
using SmartCode.DocUtils;
using SmartCode.DocUtils.Dtos;
using SmartCode.Framework;
using SmartCode.Framework.SqliteModel;
using SmartCode.Framework.Util;
using SmartCode.Tool.Annotations;
using SmartCode.Tool.Helper;
using SmartCode.Tool.UserControl;
using SmartCode.Tool.Views;
using ComboBox = System.Windows.Controls.ComboBox;
using MessageBox = HandyControl.Controls.MessageBox;
using TabControl = System.Windows.Controls.TabControl;
using TabItem = System.Windows.Controls.TabItem;
using TextBox = System.Windows.Controls.TextBox;
using PathF = System.IO.Path;

namespace SmartCode.Tool
{
    public partial class MainWindow : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private static readonly string GROUPICON = "\ue605";
        private static readonly string TABLEICON = "\ue6ac";
        private static readonly string VIEWICON = "\ue601";
        private static readonly string PROCICON = "\ue6d2";
        private string ConnectionString = "";
        private ConnectConfigs SelectendConnection = null;

        public Model dataSource = new Model();
        public List<PropertyNodeItem> itemList = new List<PropertyNodeItem>();

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            "CornerRadius", typeof(int), typeof(MainWindow), new PropertyMetadata(default(int)));

        public int CornerRadius
        {
            get => (int)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static readonly DependencyProperty DBaseProperty = DependencyProperty.Register(
            "DBase", typeof(List<DataBase>), typeof(MainWindow), new PropertyMetadata(default(List<DataBase>)));

        public List<DataBase> DBase
        {
            get => (List<DataBase>)GetValue(DBaseProperty);
            set => SetValue(DBaseProperty, value);
        }

        public static readonly DependencyProperty TreeViewDataProperty = DependencyProperty.Register(
            "TreeViewData", typeof(List<PropertyNodeItem>), typeof(MainWindow), new PropertyMetadata(default(List<PropertyNodeItem>)));
        /// <summary>
        /// 左侧菜单数据
        /// </summary>
        public List<PropertyNodeItem> TreeViewData
        {
            get => (List<PropertyNodeItem>)GetValue(TreeViewDataProperty);
            set
            {
                SetValue(TreeViewDataProperty, value);
                OnPropertyChanged(nameof(TreeViewData));
            }
        }

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
            var connectConfigs = sqLiteHelper.db.Table<ConnectConfigs>().ToList();
            SwitchMenu.ItemsSource = null;
            SwitchMenu.ItemsSource = connectConfigs;
            CbTargetConnect.ItemsSource = connectConfigs;
            var isGroup = sqLiteHelper.GetSys("IsGroup");
            CheckIsGroup.IsChecked = isGroup;
            var isMultipleTab = sqLiteHelper.GetSys("IsMultipleTab");
            CornerRadius = isMultipleTab ? 0 : 10;
            //MainW.Visibility = isMultipleTab ? Visibility.Collapsed : Visibility.Visible;
            //MainTabW.Visibility = isMultipleTab ? Visibility.Visible : Visibility.Collapsed;
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
            ConnectionString = connectConfig.DbMasterConnectString;
            SwitchMenu.Header = connectConfig.ConnectName;
            SelectendConnection = connectConfig;
            try
            {
                var exporter = ExporterFactory.CreateInstance(DBType.SqlServer, ConnectionString);
                var list = exporter.GetDatabases(ConnectionString);
                DBase = list;
                SelectDatabase.ItemsSource = DBase;
                ConnectionString = connectConfig.DbDefaultConnectString;
                HidSelectDatabase.Text = connectConfig.DefaultDatabase;
                SelectDatabase.SelectedItem = list.FirstOrDefault(x => x.DbName == connectConfig.DefaultDatabase);
                //SearchMenu.Text = string.Empty;
                var sqLiteHelper = new SQLiteHelper();
                var connectConfigs = sqLiteHelper.db.Table<ConnectConfigs>().ToList();
                SwitchMenu.ItemsSource = null;
                SwitchMenu.ItemsSource = connectConfigs;
                CbTargetConnect.ItemsSource = connectConfigs;
            }
            catch (Exception ex)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    Growl.Warning(new GrowlInfo { Message = $"连接失败 {connectConfig.ConnectName}，原因：" + ex.Message, ShowDateTime = false, Type = InfoType.Error });
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
            var selectDatabase = SelectDatabase.SelectedItem;
            if (selectDatabase != null)
            {
                ConnectionString = ConnectionString.Replace(HidSelectDatabase.Text, ((DataBase)selectDatabase).DbName);
                SelectDatabase.SelectedItem = DBase.FirstOrDefault(x => x.DbName == ((DataBase)selectDatabase).DbName);
                HidSelectDatabase.Text = ((DataBase)selectDatabase).DbName;
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
                var isGroup = sqLiteHelper.GetSys("IsGroup");
                var currObjects = new List<SObjectDTO>();
                var currGroups = new List<ObjectGroup>();
                var itemParentList = new List<PropertyNodeItem>();
                itemList = new List<PropertyNodeItem>();
                var nodeTable = new PropertyNodeItem
                {
                    ObejcetId = "0",
                    DisplayName = "数据表",
                    Name = "treeTable",
                    Icon = TABLEICON,
                    Type = ObjType.Type
                };
                itemList.Add(nodeTable);
                var nodeView = new PropertyNodeItem
                {
                    ObejcetId = "0",
                    DisplayName = "视图",
                    Name = "treeView",
                    Icon = VIEWICON,
                    Type = ObjType.Type
                };
                itemList.Add(nodeView);
                var nodeProc = new PropertyNodeItem
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
                if (isGroup)
                {
                    currGroups = sqLiteHelper.db.Table<ObjectGroup>().Where(a =>
                        a.ConnectId == selectConnection.ID &&
                        a.DataBaseName == selectDataBase).OrderBy(x => x.OrderFlag).ToList();
                    if (currGroups.Any())
                    {
                        foreach (var group in currGroups)
                        {
                            var itemChildList = new List<PropertyNodeItem>();
                            var nodeGroup = new PropertyNodeItem
                            {
                                ObejcetId = "0",
                                DisplayName = group.GroupName,
                                Name = "treeGroup",
                                Icon = GROUPICON,
                                FontWeight = "Bold",
                                Type = ObjType.Group,
                                IsExpanded = true
                            };
                            var nodeTable1 = new PropertyNodeItem
                            {
                                ObejcetId = "0",
                                DisplayName = "数据表",
                                Name = "treeTable",
                                Icon = TABLEICON,
                                Parent = nodeGroup,
                                Type = ObjType.Type
                            };
                            itemChildList.Add(nodeTable1);
                            var nodeView1 = new PropertyNodeItem
                            {
                                ObejcetId = "0",
                                DisplayName = "视图",
                                Name = "treeView",
                                Icon = VIEWICON,
                                Parent = nodeGroup,
                                Type = ObjType.Type
                            };
                            itemChildList.Add(nodeView1);
                            var nodeProc1 = new PropertyNodeItem
                            {
                                ObejcetId = "0",
                                DisplayName = "存储过程",
                                Name = "treeProc",
                                Icon = PROCICON,
                                Parent = nodeGroup,
                                Type = ObjType.Type
                            };
                            itemChildList.Add(nodeProc1);
                            nodeGroup.Children = itemChildList;
                            itemParentList.Add(nodeGroup);
                        }
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

                var exporter = ExporterFactory.CreateInstance(DBType.SqlServer, ConnectionString);
                Model model = exporter.Export(ConnectionString);
                dataSource = model;
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
                            nodeTable.Children.Add(new PropertyNodeItem()
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
                        if (isGroup)
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
                                        ppGroup.Children.Add(new PropertyNodeItem()
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
                            nodeTable.Children.Add(new PropertyNodeItem()
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
                            nodeView.Children.Add(new PropertyNodeItem()
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
                        if (isGroup)
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
                                        ppGroup.Children.Add(new PropertyNodeItem()
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
                            nodeView.Children.Add(new PropertyNodeItem()
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
                            nodeProc.Children.Add(new PropertyNodeItem()
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
                        if (isGroup)
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
                                        ppGroup.Children.Add(new PropertyNodeItem()
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
                            nodeProc.Children.Add(new PropertyNodeItem()
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
                            nodeTable.Children.Add(new PropertyNodeItem
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
                            nodeView.Children.Add(new PropertyNodeItem()
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
                            nodeProc.Children.Add(new PropertyNodeItem()
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
                    if (isGroup && !isCompare)
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
            itemList = new List<PropertyNodeItem>();
            var searchText = SearchMenu.Text.Trim();
            var nodeTable = new PropertyNodeItem()
            {
                ObejcetId = "0",
                DisplayName = "数据表",
                Name = "treeTable",
                Icon = TABLEICON,
                Type = ObjType.Type,
                IsExpanded = true
            };
            itemList.Add(nodeTable);
            var nodeView = new PropertyNodeItem()
            {
                ObejcetId = "0",
                DisplayName = "视图",
                Name = "treeView",
                Icon = VIEWICON,
                Type = ObjType.Type,
                IsExpanded = true
            };
            itemList.Add(nodeView);
            var nodeProc = new PropertyNodeItem()
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
            var isGroup = sqLiteHelper.GetSys("IsGroup");
            var selectDataBase = HidSelectDatabase.Text;
            var selectConnection = SelectendConnection;
            var currObjects = new List<SObjectDTO>();
            var currGroups = new List<ObjectGroup>();
            var itemParentList = new List<PropertyNodeItem>();
            #region 分组业务处理
            if (isGroup)
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
                    var itemChildList = new List<PropertyNodeItem>();
                    var nodeGroup = new PropertyNodeItem
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
                    var nodeTable1 = new PropertyNodeItem
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
                    var nodeView1 = new PropertyNodeItem
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
                    var nodeProc1 = new PropertyNodeItem
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
                    if (!table.Key.StartsWith(searchText, true, null) && !table.Value.Name.StartsWith(searchText, true, null))
                    {
                        continue;
                    }
                    //是否业务分组
                    if (isGroup)
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
                                    ppGroup.Children.Add(new PropertyNodeItem()
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
                        nodeTable.Children.Add(new PropertyNodeItem()
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
                    if (!view.Key.StartsWith(searchText, true, null) && !view.Value.Name.StartsWith(searchText, true, null))
                    {
                        continue;
                    }
                    //是否业务分组
                    if (isGroup)
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
                                    ppGroup.Children.Add(new PropertyNodeItem()
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
                        nodeView.Children.Add(new PropertyNodeItem()
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
                    if (!proc.Key.StartsWith(searchText, true, null) && !proc.Value.Name.StartsWith(searchText, true, null))
                    {
                        continue;
                    }
                    //是否业务分组
                    if (isGroup)
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
                                    ppGroup.Children.Add(new PropertyNodeItem()
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
                        nodeProc.Children.Add(new PropertyNodeItem()
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

            if (isGroup)
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
            if (!(TreeViewTables.SelectedItem is PropertyNodeItem objects) || objects.Type == ObjType.Group || objects.TextColor.Equals("Red"))
            {
                return;
            }
            var sqLiteHelper = new SQLiteHelper();
            var isMultipleTab = sqLiteHelper.GetSys("IsMultipleTab");
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
                {"Type", "\ue605"}, {"Table", "\ue6ac"}, {"View", "\ue601"}, {"Proc", "\ue6d2"}
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
                var exporter = ExporterFactory.CreateInstance(DBType.SqlServer, ConnectionString);
                var list = exporter.GetDatabases(dataBase.DbMasterConnectString);

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
            if (string.IsNullOrEmpty(ConnectionString))
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
            var exporter = ExporterFactory.CreateInstance(DBType.SqlServer, ConnectionString);
            LoadingLine.Visibility = Visibility.Visible;

            Model model = exporter.Export(targetConnect.DbMasterConnectString.Replace("master", targetData.DbName));
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
            if (string.IsNullOrEmpty(ConnectionString))
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
            if (CheckIsGroup.IsChecked.HasValue && CheckIsGroup.IsChecked.Value)
            {
                MenuBind(false, null);
            }
        }

        /// <summary>
        /// 是否分组显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckIsGroup_OnClick(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }
            bool isChecked = CheckIsGroup.IsChecked == true;
            var sqLiteHelper = new SQLiteHelper();
            var sysSet = sqLiteHelper.db.Table<SystemSet>().First(x => x.Name.Equals("IsGroup"));
            sysSet.Value = isChecked.ToString();
            sqLiteHelper.db.Update(sysSet);
            if (string.IsNullOrEmpty(ConnectionString))
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

        private void MenuSelectedItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(TreeViewTables.SelectedItem is PropertyNodeItem selectedObjects) || selectedObjects.ObejcetId == "0" || selectedObjects.TextColor.Equals("Red"))
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
            if (string.IsNullOrEmpty(ConnectionString))
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
            var connect = new ConnectManage();
            connect.Owner = this;
            connect.ChangeRefreshEvent += SwitchConnect;
            connect.ShowDialog();
        }

        private void ExportDoc_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ConnectionString))
            {
                Growl.Warning(new GrowlInfo { Message = $"请选择数据库", WaitTime = 1, ShowDateTime = false });
                return;
            }
            var selectDatabase = (DataBase)SelectDatabase.SelectedItem;
            ExportDoc exportDoc = new ExportDoc();
            exportDoc.Owner = this;
            exportDoc.ExportType = ExportEnum.All;
            exportDoc.SelectedConnection = SelectendConnection;
            exportDoc.SelectedDataBase = selectDatabase;
            exportDoc.ExportData = TreeViewData;
            exportDoc.ShowDialog();
        }
    }
}
