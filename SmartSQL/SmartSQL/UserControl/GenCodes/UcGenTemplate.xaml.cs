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
    public partial class UcGenTemplate : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        #region PropertyFiled
        public static readonly DependencyProperty SelectedConnectionProperty = DependencyProperty.Register(
            "SelectedConnection", typeof(ConnectConfigs), typeof(UcGenTemplate), new PropertyMetadata(default(ConnectConfigs)));
        /// <summary>
        /// 当前选中连接
        /// </summary>
        public ConnectConfigs SelectedConnection
        {
            get => (ConnectConfigs)GetValue(SelectedConnectionProperty);
            set => SetValue(SelectedConnectionProperty, value);
        }

        public static readonly DependencyProperty SelectedDataBaseProperty = DependencyProperty.Register(
            "SelectedDataBase", typeof(DataBase), typeof(UcGenTemplate), new PropertyMetadata(default(DataBase)));
        /// <summary>
        /// 当前数据库
        /// </summary>
        public DataBase SelectedDataBase
        {
            get => (DataBase)GetValue(SelectedDataBaseProperty);
            set => SetValue(SelectedDataBaseProperty, value);
        }

        public static readonly DependencyProperty TreeViewDataProperty = DependencyProperty.Register(
            "TreeViewData", typeof(List<TreeNodeItem>), typeof(UcGenTemplate), new PropertyMetadata(default(List<TreeNodeItem>)));
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
            "ExportData", typeof(List<TreeNodeItem>), typeof(UcGenTemplate), new PropertyMetadata(default(List<TreeNodeItem>)));
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
            "MenuData", typeof(Model), typeof(UcGenTemplate), new PropertyMetadata(default(Model)));
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

        public UcGenTemplate()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }
        /// <summary>
        /// 导出数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnExport_OnClick(object sender, RoutedEventArgs e)
        {

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

    }
}
