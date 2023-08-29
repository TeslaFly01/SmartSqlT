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


        public static readonly DependencyProperty DataListProperty = DependencyProperty.Register(
            "DataList", typeof(List<TemplateInfo>), typeof(UcGenTemplate), new PropertyMetadata(default(List<TemplateInfo>)));
        public List<TemplateInfo> DataList
        {
            get => (List<TemplateInfo>)GetValue(DataListProperty);
            set
            {
                SetValue(DataListProperty, value);
                OnPropertyChanged(nameof(DataList));
            }
        }
        #endregion

        public UcGenTemplate()
        {
            InitializeComponent();
            DataContext = this;
            HighlightingProvider.Register(SkinType.Dark, new HighlightingProviderDark());
            TextContent.SyntaxHighlighting = HighlightingProvider.GetDefinition(SkinType.Dark, "C#");
            TextContent.TextArea.SelectionCornerRadius = 0;
            TextContent.TextArea.SelectionBorder = null;
            TextContent.TextArea.SelectionForeground = null;
            TextContent.TextArea.SelectionBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFADD6FF"));
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }
            Task.Run(() =>
            {
                var sqLiteHelper = new SQLiteHelper();
                var datalist = sqLiteHelper.db.Table<TemplateInfo>().ToList();
                Dispatcher.Invoke(() =>
                {
                    DataList = datalist;
                    if (!datalist.Any())
                    {
                        NoDataText.Visibility = Visibility.Visible;
                    }
                });
            });
        }

        private void MenuDelete_OnClick(object sender, RoutedEventArgs e)
        {
            #region MyRegion
            if (!(ListTemplate.SelectedItem is TemplateInfo selectedTemp))
            {
                Oops.Oh("请选择需要删除的模板.");
                return;
            }
            var sqlLiteInstance = SQLiteHelper.GetInstance();
            Task.Run(() =>
            {
                sqlLiteInstance.db.Delete<TemplateInfo>(selectedTemp.Id);
                var list = sqlLiteInstance.db.Table<TemplateInfo>().ToList();
                Dispatcher.Invoke(() =>
                {
                    DataList = list;
                });
            });
            #endregion
        }

        /// <summary>
        /// 导出数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            var tempName = TextTempName.Text.Trim();
            var tempContent = TextContent.Text.Trim();
            var sqLiteHelper = new SQLiteHelper();
            if (string.IsNullOrEmpty(tempName))
            {
                Oops.God("模板名称为空");
                return;
            }
            if (string.IsNullOrEmpty(tempContent))
            {
                Oops.God("模板内容为空");
                return;
            }
            if (sqLiteHelper.IsAny<TemplateInfo>(x => x.TempName == tempName))
            {
                Oops.God("模板名称出现重复");
                return;
            }
            var temp = new TemplateInfo
            {
                TempName = tempName,
                Content = tempContent
            };
            sqLiteHelper.db.Insert(temp);
            var list = sqLiteHelper.db.Table<TemplateInfo>().ToList();
            DataList = list;
            TextTempName.Text = string.Empty;
            TextContent.Text = string.Empty;
        }

        /// <summary>
        /// 取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCancel_OnClick(object sender, RoutedEventArgs e)
        {
            var mainWindow = (GenCode)System.Windows.Window.GetWindow(this);
            mainWindow?.Close();
        }
    }
}
