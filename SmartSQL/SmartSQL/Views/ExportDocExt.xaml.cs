using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using HandyControl.Controls;
using HandyControl.Data;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using SmartSQL.Annotations;
using SmartSQL.DocUtils;
using SmartSQL.DocUtils.Dtos;
using SmartSQL.Framework;
using SmartSQL.Framework.Const;
using SmartSQL.Framework.Exporter;
using SmartSQL.Framework.PhysicalDataModel;
using SmartSQL.Framework.SqliteModel;
using SmartSQL.Framework.Util;
using SmartSQL.Models;
using SqlSugar;
using DbType = SqlSugar.DbType;

namespace SmartSQL.Views
{
    /// <summary>
    /// ExportDoc.xaml 的交互逻辑
    /// </summary>
    public partial class ExportDocExt : INotifyPropertyChanged
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
        #region DependencyProperty
        public static readonly DependencyProperty SelectedConnectionProperty = DependencyProperty.Register(
            "SelectedConnection", typeof(ConnectConfigs), typeof(ExportDocExt), new PropertyMetadata(default(ConnectConfigs)));
        /// <summary>
        /// 当前连接
        /// </summary>
        public ConnectConfigs SelectedConnection
        {
            get => (ConnectConfigs)GetValue(SelectedConnectionProperty);
            set => SetValue(SelectedConnectionProperty, value);
        }

        public static readonly DependencyProperty SelectedDataBaseProperty = DependencyProperty.Register(
            "SelectedDataBase", typeof(DataBase), typeof(ExportDocExt), new PropertyMetadata(default(DataBase)));
        /// <summary>
        /// 当前数据库
        /// </summary>
        public DataBase SelectedDataBase
        {
            get => (DataBase)GetValue(SelectedDataBaseProperty);
            set => SetValue(SelectedDataBaseProperty, value);
        }

        public static readonly DependencyProperty TreeViewDataProperty = DependencyProperty.Register(
            "TreeViewData", typeof(List<TreeNodeItem>), typeof(ExportDocExt), new PropertyMetadata(default(List<TreeNodeItem>)));
        /// <summary>
        /// 树形对象菜单
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

        public static readonly DependencyProperty ExportDataProperty = DependencyProperty.Register(
            "ExportData", typeof(List<TreeNodeItem>), typeof(ExportDocExt), new PropertyMetadata(default(List<TreeNodeItem>)));
        /// <summary>
        /// 导出目标数据
        /// </summary>
        public List<TreeNodeItem> ExportData
        {
            get => (List<TreeNodeItem>)GetValue(ExportDataProperty);
            set
            {
                SetValue(ExportDataProperty, value);
                OnPropertyChanged(nameof(ExportData));
            }
        }
        private Model dataSource = new Model();
        private List<TreeNodeItem> itemList = new List<TreeNodeItem>();
        #endregion

        public ExportDocExt()
        {
            InitializeComponent();
            DataContext = this;
            TxtPath.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }

        private void ExportDoc_OnLoaded(object sender, RoutedEventArgs e)
        {
            Title = $"{SelectedDataBase.DbName} - {Title}";
            TxtFileName.Text = SelectedDataBase.DbName + "数据库设计文档";
            var dbInstance = ExporterFactory.CreateInstance(SelectedConnection.DbType, SelectedConnection.DbMasterConnectString);
            var list = dbInstance.GetDatabases();
            SelectDatabase.ItemsSource = list;
            HidSelectDatabase.Text = SelectedDataBase.DbName;
            SelectDatabase.SelectedItem = list.FirstOrDefault(x => x.DbName == SelectedDataBase.DbName);
            MenuBind();
        }

        private void BtnLookPath_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            CommonFileDialogResult result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                TxtPath.Text = dialog.FileName;
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
                HidSelectDatabase.Text = ((DataBase)selectDatabase).DbName;
                MenuBind();
            }
        }

        /// <summary>
        /// 实时搜索表、视图、存储过程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchMenu_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            SearchMenuBind();
        }

        /// <summary>
        /// 菜单绑定
        /// </summary>
        public void MenuBind()
        {
            #region MyRegion
            LoadingLine.Visibility = Visibility.Visible;
            NoDataText.Visibility = Visibility.Collapsed;
            /////TreeViewTables.ItemsSource = null;
            var selectDataBase = HidSelectDatabase.Text;
            var selectConnection = SelectedConnection;
            var selectData = ExportData;
            Task.Run(() =>
            {
                var sqLiteHelper = new SQLiteHelper();
                var leftMenuType = 1; // sqLiteHelper.GetSysInt(SysConst.Sys_LeftMenuType);
                var curObjects = new List<SObjectDTO>();
                var curGroups = new List<ObjectGroup>();
                var itemParentList = new List<TreeNodeItem>();
                var itemList = new List<TreeNodeItem>();
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
                    var isChecked = selectData != null && selectData.Any(x => x.DisplayName.Equals(table.Value.DisplayName));
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
                                        Parent = ppGroup,
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
                            Parent = nodeTable,
                            DisplayName = table.Value.DisplayName,
                            Name = table.Value.Name,
                            Schema = table.Value.SchemaName,
                            Comment = table.Value.Comment,
                            CreateDate = table.Value.CreateDate,
                            ModifyDate = table.Value.ModifyDate,
                            TextColor = textColor,
                            Icon = TABLEICON,
                            Type = ObjType.Table,
                            IsChecked = isChecked
                        });
                    }

                }
                #endregion

                #region 视图
                foreach (var view in model.Views)
                {
                    var isChecked = selectData != null && selectData.Any(x => x.DisplayName.Equals(view.Value.DisplayName));
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
                                        Parent = ppGroup,
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
                            Parent = nodeView,
                            DisplayName = view.Value.DisplayName,
                            Name = view.Value.Name,
                            Schema = view.Value.SchemaName,
                            Comment = view.Value.Comment,
                            CreateDate = view.Value.CreateDate,
                            ModifyDate = view.Value.ModifyDate,
                            TextColor = textColor,
                            Icon = VIEWICON,
                            Type = ObjType.View,
                            IsChecked = isChecked
                        });
                    }
                }
                #endregion

                #region 存储过程
                foreach (var proc in model.Procedures)
                {
                    var isChecked = selectData != null && selectData.Any(x => x.DisplayName.Equals(proc.Value.DisplayName));
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
                                        Parent = ppGroup,
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
                            Parent = nodeProc,
                            DisplayName = proc.Value.DisplayName,
                            Name = proc.Value.Name,
                            Schema = proc.Value.SchemaName,
                            Comment = proc.Value.Comment,
                            CreateDate = proc.Value.CreateDate,
                            ModifyDate = proc.Value.ModifyDate,
                            TextColor = textColor,
                            Icon = PROCICON,
                            Type = ObjType.Proc,
                            IsChecked = isChecked
                        });
                    }
                }
                #endregion

                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    LoadingLine.Visibility = Visibility.Hidden;
                    //编写获取数据并显示在界面的代码
                    //是否业务分组
                    if (leftMenuType == LeftMenuType.Group.GetHashCode())
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
            var selectConnection = SelectedConnection;
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
                                        Parent = ppGroup,
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
                            Parent = nodeTable,
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
                                        Parent = ppGroup,
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
                            Parent = nodeView,
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
                                        Parent = ppGroup,
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
                            Parent = nodeProc,
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
        /// 导出数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnExport_OnClick(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            var selectedConnection = SelectedConnection;
            var selectedDatabase = SelectedDataBase;
            var exportData = TreeViewData;
            var floderPath = TxtPath.Text;
            var doctype = DocumentType();
            if (string.IsNullOrEmpty(doctype))
            {
                Growl.WarningGlobal(new GrowlInfo { Message = $"请选择输出文档类型.", WaitTime = 1, ShowDateTime = false });
                return;
            }
            var checkAny = exportData.Count(x => x.Type == "Type" && x.IsChecked == false);
            if (checkAny == 3)
            {
                Growl.WarningGlobal(new GrowlInfo { Message = $"请选择需要导出的对象.", WaitTime = 1, ShowDateTime = false });
                return;
            }
            if (string.IsNullOrEmpty(TxtFileName.Text))
            {
                TxtFileName.Text = $"{SelectedDataBase.DbName}数据库设计文档";
            }
            //文件扩展名
            var fileNameE = LblFileExtend.Content;
            //文件名
            var fileName = TxtFileName.Text.Trim() + fileNameE;
            LoadingG.Visibility = Visibility.Visible;
            var dbDto = new DBDto(selectedDatabase.DbName);
            Task.Factory.StartNew(() =>
            {
                dbDto.DBType = selectedConnection.DbType.ToString();

                dbDto.Tables = Trans2Table(exportData, selectedConnection, selectedDatabase);
                dbDto.Procs = Trans2Dictionary(exportData, selectedConnection, selectedDatabase, "Proc");
                dbDto.Views = Trans2Dictionary(exportData, selectedConnection, selectedDatabase, "View");

                //判断文档路径是否存在
                if (!Directory.Exists(floderPath))
                {
                    Directory.CreateDirectory(floderPath);
                }
                var filePath = Path.Combine(floderPath, fileName);
                var doc = DocFactory.CreateInstance((DocType)(Enum.Parse(typeof(DocType), doctype)), dbDto);
                try
                {
                    var bulResult = doc.Build(filePath);
                    Dispatcher.Invoke(() =>
                    {
                        LoadingG.Visibility = Visibility.Collapsed;
                        if (bulResult)
                        {
                            Growl.SuccessGlobal("导出成功.");
                        }
                        else
                        {
                            Growl.WarningGlobal($"导出失败.");
                        }
                    });
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        LoadingG.Visibility = Visibility.Collapsed;
                        Growl.WarningGlobal($"导出失败，原因：{ex.ToMsg()}");
                    });
                }
            }, TaskCreationOptions.LongRunning);
            #endregion
        }

        private List<ViewProDto> Trans2Dictionary(List<TreeNodeItem> treeViewData, ConnectConfigs selectedConnection, DataBase selectedDatabase, string type)
        {
            #region MyRegion
            var selectedConnectionString = selectedConnection.SelectedDbConnectString(selectedDatabase.DbName);
            var exporter = ExporterFactory.CreateInstance(selectedConnection.DbType, selectedConnectionString);
            var objectType = type == "View" ? DbObjectType.View : DbObjectType.Proc;
            var viewPro = new List<ViewProDto>();
            foreach (var group in treeViewData)
            {
                if (group.Name.Equals("treeTable"))
                {
                    continue;
                }
                if (group.IsChecked == false)
                {
                    continue;
                }
                if (group.Type == "Type")
                {
                    foreach (var item in group.Children)
                    {
                        if (item.IsChecked == false)
                        {
                            continue;
                        }
                        if (item.Type == type)
                        {
                            var script = exporter.GetScriptInfoById(item.ObejcetId, objectType);
                            viewPro.Add(new ViewProDto
                            {
                                ObjectName = item.DisplayName,
                                Comment = item.Comment,
                                Script = script
                            });
                        }
                    }
                }
                else
                {
                    if (group.Type == type)
                    {
                        var script = exporter.GetScriptInfoById(group.ObejcetId, objectType);
                        viewPro.Add(new ViewProDto
                        {
                            ObjectName = group.DisplayName,
                            Comment = group.Comment,
                            Script = script
                        });
                    }
                }
            }
            return viewPro;
            #endregion
        }

        private List<TableDto> Trans2Table(List<TreeNodeItem> treeViewData, ConnectConfigs selectedConnection, DataBase selectedDatabase)
        {
            #region MyRegion
            var selectedConnectionString = selectedConnection.SelectedDbConnectString(selectedDatabase.DbName);
            var tables = new List<TableDto>();
            var groupNo = 1;
            foreach (var group in treeViewData)
            {
                if (group.Type == "Type" && group.Name.Equals("treeTable"))
                {
                    int orderNo = 1;
                    foreach (var node in group.Children)
                    {
                        if (node.IsChecked == false)
                        {
                            continue;
                        }
                        TableDto tbDto = new TableDto();
                        tbDto.TableOrder = orderNo.ToString();
                        tbDto.TableName = node.Name;
                        tbDto.Comment = node.Comment.FilterIllegalDir();
                        tbDto.DBType = nameof(DbType.SqlServer);

                        var lst_col_dto = new List<ColumnDto>();
                        var dbInstance = ExporterFactory.CreateInstance(selectedConnection.DbType,
                            selectedConnectionString);
                        var columns = dbInstance.GetColumnInfoById(node.ObejcetId);
                        var columnIndex = 1;
                        foreach (var col in columns)
                        {
                            var colDto = new ColumnDto();
                            colDto.ColumnOrder = columnIndex.ToString();
                            colDto.ColumnName = col.Value.Name;
                            // 数据类型
                            colDto.ColumnTypeName = col.Value.DataType;
                            // 长度
                            colDto.Length = col.Value.Length;
                            // 小数位
                            //colDto.Scale = "";//(col.Scale.HasValue ? col.Scale.Value.ToString() : "");
                            // 主键
                            colDto.IsPK = (col.Value.IsPrimaryKey ? "√" : "");
                            // 自增
                            colDto.IsIdentity = (col.Value.IsIdentity ? "√" : "");
                            // 允许空
                            colDto.CanNull = (col.Value.IsNullable ? "√" : "");
                            // 默认值
                            colDto.DefaultVal = (!string.IsNullOrWhiteSpace(col.Value.DefaultValue) ? col.Value.DefaultValue : "");
                            // 列注释（说明）
                            colDto.Comment = col.Value.Comment.FilterIllegalDir();

                            lst_col_dto.Add(colDto);
                            columnIndex++;
                        }
                        tbDto.Columns = lst_col_dto;
                        tables.Add(tbDto);
                        orderNo++;
                    }
                }
                if (group.Type == "Table")
                {
                    TableDto tbDto = new TableDto();
                    tbDto.TableOrder = groupNo.ToString();
                    tbDto.TableName = group.Name;
                    tbDto.Comment = group.Comment.FilterIllegalDir();
                    tbDto.DBType = selectedConnection.DbType.ToString();

                    var lst_col_dto = new List<ColumnDto>();
                    var dbInstance = ExporterFactory.CreateInstance(selectedConnection.DbType,
                        selectedConnectionString);
                    var columns = dbInstance.GetColumnInfoById(group.ObejcetId);
                    var columnIndex = 1;
                    foreach (var col in columns)
                    {
                        ColumnDto colDto = new ColumnDto();
                        colDto.ColumnOrder = columnIndex.ToString();
                        colDto.ColumnName = col.Value.Name;
                        // 数据类型
                        colDto.ColumnTypeName = col.Value.DataType;
                        // 长度
                        colDto.Length = col.Value.Length;
                        // 小数位
                        //colDto.Scale = "";//(col.Scale.HasValue ? col.Scale.Value.ToString() : "");
                        // 主键
                        colDto.IsPK = (col.Value.IsPrimaryKey ? "√" : "");
                        // 自增
                        colDto.IsIdentity = (col.Value.IsIdentity ? "√" : "");
                        // 允许空
                        colDto.CanNull = (col.Value.IsNullable ? "√" : "");
                        // 默认值
                        colDto.DefaultVal = (!string.IsNullOrWhiteSpace(col.Value.DefaultValue) ? col.Value.DefaultValue : "");
                        // 列注释（说明）
                        colDto.Comment = col.Value.Comment.FilterIllegalDir();
                        lst_col_dto.Add(colDto);
                        columnIndex++;
                    }
                    tbDto.Columns = lst_col_dto;
                    tables.Add(tbDto);
                    groupNo++;
                }
            }
            return tables;
            #endregion
        }

        /// <summary>
        /// 取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCancel_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 对应文档提示信息
        /// </summary>
        private static Dictionary<string, string> _docTypeTipMsg = new Dictionary<string, string>
        {
            {"chm",""},
            {"excel","Excel文档类型仅支持导出数据表"},
            {"word","Word文档类型仅支持导出数据表"},
            {"pdf",""},
            {"html","Html文档类型仅支持导出数据表"},
            {"xml",""},
            {"markdown",""}
        };

        /// <summary>
        /// 导出文档类型单选
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Toggle_OnChecked(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }
            GridTipMsg.Visibility = Visibility.Collapsed;
            var button = (ToggleButton)sender;
            foreach (ToggleButton toggle in ToggleWarpPanel.Children)
            {
                if (toggle.Name != button.Name)
                {
                    toggle.IsChecked = false;
                }
            }
            var tipMsg = _docTypeTipMsg[button.Name.ToLower()];
            if (!string.IsNullOrEmpty(tipMsg))
            {
                GridTipMsg.Visibility = Visibility.Visible;
                TextDocTipMsg.Text = tipMsg;
            }
            var docType = (DocType)(Enum.Parse(typeof(DocType), button.Content.ToString().ToLower()));
            var fileExtend = FileExtend(docType);
            LblFileExtend.Content = "." + fileExtend;
        }

        private string DocumentType()
        {
            var type = string.Empty;
            foreach (ToggleButton button in ToggleWarpPanel.Children)
            {
                if (button.IsChecked == true)
                {
                    type = button.Content.ToString().ToLower();
                }
            }
            return type;
        }

        private string FileExtend(DocType docType)
        {
            switch (docType)
            {
                case DocType.word: return "docx";
                case DocType.chm: return "chm";
                case DocType.excel: return "xlsx";
                case DocType.html: return "html";
                case DocType.markdown: return "md";
                case DocType.pdf: return "pdf";
                case DocType.xml: return "xml";
                default: return "chm";
            }
        }

        private void EventSetter_OnHandler(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;
        }
    }
}
