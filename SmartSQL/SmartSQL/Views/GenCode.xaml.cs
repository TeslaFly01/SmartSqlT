using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
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
using SmartSQL.Helper;
using SmartSQL.Models;
using SmartSQL.UserControl.GenCodes;
using SqlSugar;
using DbType = SqlSugar.DbType;

namespace SmartSQL.Views
{
    /// <summary>
    /// ExportDoc.xaml 的交互逻辑
    /// </summary>
    public partial class GenCode : INotifyPropertyChanged
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
        #region DependencyProperty
        public static readonly DependencyProperty SelectedConnectionProperty = DependencyProperty.Register(
            "SelectedConnection", typeof(ConnectConfigs), typeof(GenCode), new PropertyMetadata(default(ConnectConfigs)));
        /// <summary>
        /// 当前连接
        /// </summary>
        public ConnectConfigs SelectedConnection
        {
            get => (ConnectConfigs)GetValue(SelectedConnectionProperty);
            set => SetValue(SelectedConnectionProperty, value);
        }

        public static readonly DependencyProperty SelectedDataBaseProperty = DependencyProperty.Register(
            "SelectedDataBase", typeof(DataBase), typeof(GenCode), new PropertyMetadata(default(DataBase)));
        /// <summary>
        /// 当前数据库
        /// </summary>
        public DataBase SelectedDataBase
        {
            get => (DataBase)GetValue(SelectedDataBaseProperty);
            set => SetValue(SelectedDataBaseProperty, value);
        }

        public static readonly DependencyProperty TreeViewDataProperty = DependencyProperty.Register(
            "TreeViewData", typeof(List<TreeNodeItem>), typeof(GenCode), new PropertyMetadata(default(List<TreeNodeItem>)));
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
            "ExportData", typeof(List<TreeNodeItem>), typeof(GenCode), new PropertyMetadata(default(List<TreeNodeItem>)));
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
            "MenuData", typeof(Model), typeof(GenCode), new PropertyMetadata(default(Model)));
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
        #endregion

        public GenCode()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void GenCode_OnLoaded(object sender, RoutedEventArgs e)
        {
            var ucGenCode = new UcGenCode();
            ucGenCode.SelectedConnection = SelectedConnection;
            ucGenCode.SelectedDataBase = SelectedDataBase;
            ucGenCode.TreeViewData = TreeViewData;
            ucGenCode.ExportData = ExportData;
            ucGenCode.MenuData = MenuData;
            UcGenMain.Content = ucGenCode;

        }

        private void MenuGen_OnSelected(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }
            var ucGenCode = new UcGenCode();
            ucGenCode.SelectedConnection = SelectedConnection;
            ucGenCode.SelectedDataBase = SelectedDataBase;
            ucGenCode.TreeViewData = TreeViewData;
            ucGenCode.ExportData = ExportData;
            ucGenCode.MenuData = MenuData;
            UcGenMain.Content = ucGenCode;
        }

        private void MenuTemplate_OnSelected(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }
            var ucGenCode = new UcGenTemplate();
            ucGenCode.SelectedConnection = SelectedConnection;
            UcGenMain.Content = ucGenCode;
        }
    }
}
