using SmartSQL.Annotations;
using SmartSQL.Framework;
using SmartSQL.Framework.SqliteModel;
using SmartSQL.Models;
using SmartSQL.Views.Category;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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
using HandyControl.Controls;
using HandyControl.Data;
using Microsoft.WindowsAPICodePack.Dialogs;
using SmartSQL.DocUtils;
using SmartSQL.DocUtils.Dtos;
using SmartSQL.Framework.Const;
using SmartSQL.Framework.PhysicalDataModel;
using SmartSQL.Helper;
using SmartSQL.Views;
using Path = System.Windows.Shapes.Path;

namespace SmartSQL.UserControl.GenCodes
{
    /// <summary>
    /// TagObjects.xaml 的交互逻辑
    /// </summary>
    public partial class UcGenCode : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string fileExt = ".chm";
        private static readonly string GROUPICON = "pack://application:,,,/Resources/svg/category.svg";
        private static readonly string TABLEICON = "pack://application:,,,/Resources/svg/table.svg";
        private static readonly string VIEWICON = "pack://application:,,,/Resources/svg/view.svg";
        private static readonly string PROCICON = "pack://application:,,,/Resources/svg/proc.svg";
        #region PropertyFiled
        public static readonly DependencyProperty SelectedConnectionProperty = DependencyProperty.Register(
            "SelectedConnection", typeof(ConnectConfigs), typeof(UcGenCode), new PropertyMetadata(default(ConnectConfigs)));
        /// <summary>
        /// 当前选中连接
        /// </summary>
        public ConnectConfigs SelectedConnection
        {
            get => (ConnectConfigs)GetValue(SelectedConnectionProperty);
            set => SetValue(SelectedConnectionProperty, value);
        }

        public static readonly DependencyProperty SelectedDataBaseProperty = DependencyProperty.Register(
            "SelectedDataBase", typeof(DataBase), typeof(UcGenCode), new PropertyMetadata(default(DataBase)));
        /// <summary>
        /// 当前数据库
        /// </summary>
        public DataBase SelectedDataBase
        {
            get => (DataBase)GetValue(SelectedDataBaseProperty);
            set => SetValue(SelectedDataBaseProperty, value);
        }

        public static readonly DependencyProperty TreeViewDataProperty = DependencyProperty.Register(
            "TreeViewData", typeof(List<TreeNodeItem>), typeof(UcGenCode), new PropertyMetadata(default(List<TreeNodeItem>)));
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
            "ExportData", typeof(List<TreeNodeItem>), typeof(UcGenCode), new PropertyMetadata(default(List<TreeNodeItem>)));
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

        /// <summary>
        /// 菜单数据
        /// </summary>
        public static readonly DependencyProperty MenuDataProperty = DependencyProperty.Register(
            "MenuData", typeof(Model), typeof(UcGenCode), new PropertyMetadata(default(Model)));
        /// <summary>
        /// 菜单数据
        /// </summary>
        public Model MenuData
        {
            get => (Model)GetValue(MenuDataProperty);
            set
            {
                SetValue(MenuDataProperty, value);
                OnPropertyChanged(nameof(MenuData));
            }
        }

        private List<TreeNodeItem> itemList = new List<TreeNodeItem>();
        #endregion

        public UcGenCode()
        {
            InitializeComponent();
            DataContext = this;
            TxtPath.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            //Title = $"{SelectedDataBase.DbName} - {Title}";
            var fName = SelectedDataBase.DbName;
            if (fName.Contains(":"))
            {
                fName = fName.Replace(":", "_");
            }
            TxtFileName.Text = fName + "数据库设计文档";
            var dbInstance = ExporterFactory.CreateInstance(SelectedConnection.DbType, SelectedConnection.DbMasterConnectString, SelectedDataBase.DbName);
            var list = dbInstance.GetDatabases(SelectedDataBase.DbName);
            SelectDatabase.ItemsSource = list;
            HidSelectDatabase.Text = SelectedDataBase.DbName;
            SelectDatabase.SelectedItem = list.FirstOrDefault(x => x.DbName == SelectedDataBase.DbName);
            MenuBind();
            #endregion
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
            #region MyRegion
            if (!IsLoaded)
            {
                return;
            }
            var selectDatabase = SelectDatabase.SelectedItem;
            if (selectDatabase != null)
            {
                HidSelectDatabase.Text = ((DataBase)selectDatabase).DbName;
                MenuBind(true);
            }
            #endregion
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
        public void MenuBind(bool isSelectDb = false)
        {
            #region MyRegion
            LoadingLine.Visibility = Visibility.Visible;
            NoDataText.Visibility = Visibility.Collapsed;
            /////TreeViewTables.ItemsSource = null;
            var selectDataBase = SelectedDataBase;
            var selectDataBaseText = HidSelectDatabase.Text;
            var selectConnection = SelectedConnection;
            var selectData = ExportData;
            var menuData = MenuData;
            Task.Run(() =>
            {
                var sqLiteHelper = new SQLiteHelper();
                var leftMenuType = 1; // sqLiteHelper.GetSysInt(SysConst.Sys_LeftMenuType);
                var curObjects = new List<SObjectDTO>();
                var curGroups = new List<GroupInfo>();
                var itemParentList = new List<TreeNodeItem>();
                var itemList = new List<TreeNodeItem>();
                var nodeTable = new TreeNodeItem
                {
                    ObejcetId = "0",
                    DisplayName = "表",
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
                    curGroups = sqLiteHelper.db.Table<GroupInfo>().Where(a =>
                        a.ConnectId == selectConnection.ID &&
                        a.DataBaseName == selectDataBaseText).OrderBy(x => x.OrderFlag).ToList();
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
                                DisplayName = "表",
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
                    curObjects = (from a in sqLiteHelper.db.Table<GroupInfo>()
                                  join b in sqLiteHelper.db.Table<GroupObjects>() on a.Id equals b.GroupId
                                  where a.ConnectId == selectConnection.ID &&
                                        a.DataBaseName == selectDataBaseText
                                  select new SObjectDTO
                                  {
                                      GroupName = a.GroupName,
                                      ObjectName = b.ObjectName
                                  }).ToList();
                }
                #endregion

                if (isSelectDb)
                {
                    #region 更新左侧菜单
                    var model = new Model();
                    try
                    {
                        var dbInstance = ExporterFactory.CreateInstance(selectConnection.DbType,
                            selectConnection.SelectedDbConnectString(selectDataBaseText), selectDataBase.DbName);
                        model = dbInstance.Init();
                        menuData = model;
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
                    #endregion
                }

                var textColor = "#4f5d79";
                #region 表
                foreach (var table in menuData.Tables)
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
                                var ppGroup = pGroup.Children.FirstOrDefault(x => x.DisplayName == "表");
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
                nodeTable.IsChecked = ParentIsChecked(nodeTable);
                #endregion

                #region 视图
                foreach (var view in menuData.Views)
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
                nodeView.IsChecked = ParentIsChecked(nodeView);
                #endregion

                #region 存储过程
                foreach (var proc in menuData.Procedures)
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
                nodeProc.IsChecked = ParentIsChecked(nodeProc);
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
                DisplayName = "表",
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
            var menuData = MenuData;
            var currObjects = new List<SObjectDTO>();
            var currGroups = new List<GroupInfo>();
            var itemParentList = new List<TreeNodeItem>();
            leftMenuType = LeftMenuType.All.GetHashCode();
            #region 分组业务处理
            if (leftMenuType == LeftMenuType.Group.GetHashCode())
            {
                currGroups = sqLiteHelper.db.Table<GroupInfo>().Where(a =>
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
                        DisplayName = "表",
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
                currObjects = (from a in sqLiteHelper.db.Table<GroupInfo>()
                               join b in sqLiteHelper.db.Table<GroupObjects>() on a.Id equals b.GroupId
                               where a.ConnectId == selectConnection.ID &&
                                     a.DataBaseName == selectDataBase
                               select new SObjectDTO
                               {
                                   GroupName = a.GroupName,
                                   ObjectName = b.ObjectName
                               }).ToList();
            }
            #endregion

            #region 表
            if (menuData.Tables != null)
            {
                foreach (var table in menuData.Tables)
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
                                var ppGroup = pGroup.Children.FirstOrDefault(x => x.DisplayName == "表");
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
            if (menuData.Views != null)
            {
                foreach (var view in menuData.Views)
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
            if (menuData.Procedures != null)
            {
                foreach (var proc in menuData.Procedures)
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

        private int curProgressNum = 0;
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
            var doctype = "";// DocumentType();
            var docTypeEnum = (DocType)(Enum.Parse(typeof(DocType), doctype));
            if (string.IsNullOrEmpty(doctype))
            {
                Oops.Oh("请选择输出文档类型");
                return;
            }
            var checkAny = exportData.Count(x => x.Type == "Type" && x.IsChecked == false);
            if (checkAny == 3)
            {
                Oops.Oh("请选择需要导出的对象");
                return;
            }
            if (CharacterList.Any(x => TxtFileName.Text.Contains(x)))
            {
                Oops.Oh("文档名称不能包含下列任何符号：\\ / : * ? \" < > |");
                return;
            }
           
            #endregion
        }

        /// <summary>
        /// 取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCancel_OnClick(object sender, RoutedEventArgs e)
        {
            //this.Parent.Close();
        }

        private void TxtFileName_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var fileName = TxtFileName.Text;
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }
            if (CharacterList.Any(x => fileName.Contains(x)))
            {
                Oops.Oh("文档名称不能包含下列任何符号：\\ / : * ? \" < > |");
            }
        }

        /// <summary>
        /// 特殊符号
        /// </summary>
        private static List<string> CharacterList = new List<string>
        {
            "\\","/", ":","*","?","\"","<",">","|"
        };

        private void TreeViewTables_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!(TreeViewTables.SelectedItem is TreeNodeItem selectedObject) || selectedObject.Type == ObjType.Group)
            {
                return;
            }
            TreeViewData.ForEach(x =>
            {
                x.Children.ForEach(item =>
                {
                    if (item.DisplayName == selectedObject.DisplayName)
                    {
                        item.IsChecked = !item.IsChecked;
                    }
                });
            });
        }

        public bool? ParentIsChecked(TreeNodeItem node)
        {
            #region MyRegion
            bool? viewIsCheck = null;
            var viewCount = node.Children.Count;
            var viewCheckCount = node.Children.Count(x => x.IsChecked == true);
            if (viewCount == viewCheckCount)
            {
                viewIsCheck = true;
            }
            if (viewCheckCount == 0)
            {
                viewIsCheck = false;
            }
            return viewIsCheck;
            #endregion
        }

        private void EventSetter_OnHandler(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;
        }
    }
}
