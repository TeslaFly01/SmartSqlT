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
            LoadMenu();
        }

        private void LoadMenu(string searchName = "")
        {
            Task.Run(() =>
            {
                var sqLiteHelper = new SQLiteHelper();
                var datalist = sqLiteHelper.db.Table<TemplateInfo>().Where(x => x.TempName.Contains(searchName)).ToList();
                Dispatcher.Invoke(() =>
                {
                    DataList = datalist;
                    NoDataText.Visibility = datalist.Any() ? Visibility.Collapsed : Visibility.Visible;
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
                LoadMenu();
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
            var tempId = Convert.ToInt32(HidId.Text);
            var tempName = TextTempName.Text.Trim();
            var tempContent = TextContent.Text.Trim();
            var sqLiteHelper = new SQLiteHelper();
            if (string.IsNullOrEmpty(tempName))
            {
                Oops.Oh("模板名称为空");
                return;
            }
            if (string.IsNullOrEmpty(tempContent))
            {
                Oops.Oh("模板内容为空");
                return;
            }
            if (sqLiteHelper.IsAny<TemplateInfo>(x => x.Id != tempId && x.TempName == tempName))
            {
                Oops.Oh("模板名称出现重复");
                return;
            }
            var temp = new TemplateInfo
            {
                Id = tempId,
                TempName = tempName,
                Content = tempContent
            };
            var isSuccess = false;
            if (tempId > 0)
            {
                isSuccess = sqLiteHelper.db.Update(temp) > 0;
            }
            else
            {
                isSuccess = sqLiteHelper.db.Insert(temp) > 0;
                TextTempName.Text = string.Empty;
                TextContent.Text = string.Empty;
            }
            if (isSuccess)
            {
                Oops.Success("保存成功");
            }
            LoadMenu();
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

        private void ListTemplate_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = (ListBox)sender;
            if (listBox.SelectedItems.Count > 0)
            {
                var tempInfo = (TemplateInfo)listBox.SelectedItems[0];
                HidId.Text = tempInfo.Id.ToString();
                TextTempName.Text = tempInfo.TempName;
                TextContent.Text = tempInfo.Content;
            }
        }

        private void SearchMenu_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var searchName = SearchMenu.Text.Trim();
            LoadMenu(searchName);
        }

        private void BtnAdd_OnClick(object sender, RoutedEventArgs e)
        {
            HidId.Text = "0";
            TextTempName.Text = "";
            TextContent.Text = "";
        }
    }
}
