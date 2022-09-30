using HandyControl.Controls;
using HandyControl.Data;
using SmartSQL.Framework;
using SmartSQL.Framework.SqliteModel;
using SmartSQL.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;
using System.Windows.Threading;
using SmartSQL.Annotations;

namespace SmartSQL.Views.Category
{
    /// <summary>
    /// TagAddView.xaml 的交互逻辑
    /// </summary>
    public partial class TagAddView : INotifyPropertyChanged
    {
        public event ChangeRefreshHandler ChangeRefreshEvent;

        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #region DependencyProperty

        public static readonly DependencyProperty SelectedConnectionProperty = DependencyProperty.Register(
            "SelectedConnection", typeof(ConnectConfigs), typeof(TagAddView), new PropertyMetadata(default(ConnectConfigs)));
        public ConnectConfigs SelectedConnection
        {
            get => (ConnectConfigs)GetValue(SelectedConnectionProperty);
            set => SetValue(SelectedConnectionProperty, value);
        }

        public static readonly DependencyProperty SelectedDataBaseProperty = DependencyProperty.Register(
            "SelectedDataBase", typeof(string), typeof(TagAddView), new PropertyMetadata(default(string)));
        public string SelectedDataBase
        {
            get => (string)GetValue(SelectedDataBaseProperty);
            set => SetValue(SelectedDataBaseProperty, value);
        }

        public static readonly DependencyProperty SelectedTagProperty = DependencyProperty.Register(
            "SelectedTag", typeof(TagInfo), typeof(TagAddView), new PropertyMetadata(default(TagInfo)));
        public TagInfo SelectedTag
        {
            get => (TagInfo)GetValue(SelectedTagProperty);
            set
            {
                SetValue(SelectedTagProperty, value);
                OnPropertyChanged(nameof(SelectedTag));
            }
        }
        #endregion

        public TagAddView()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)(() =>
                {
                    Keyboard.Focus(TagName);
                    if (SelectedTag != null)
                    {
                        Title = "修改标签";
                    }
                }));
        }

        /// <summary>
        /// 保存标签
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var tagName = TagName.Text.Trim();
            if (string.IsNullOrEmpty(tagName))
            {
                //Oops.Oh("标签名称为空.");
                
                TextErrorMsg.Visibility = Visibility.Visible;
                return;
            }
            var sqLiteInstance = SQLiteHelper.GetInstance();
            if (SelectedTag == null)
            {
                var tag = sqLiteInstance.db.Table<TagInfo>().FirstOrDefault(x =>
                    x.ConnectId == SelectedConnection.ID &&
                    x.DataBaseName == SelectedDataBase &&
                    x.TagName == tagName);
                if (tag != null)
                {
                    Oops.Oh("已存在相同名称的标签.");
                    return;
                }
                //插入标签数据
                sqLiteInstance.db.Insert(new TagInfo()
                {
                    ConnectId = SelectedConnection.ID,
                    DataBaseName = SelectedDataBase,
                    TagName = tagName
                });
            }
            else
            {
                var tag = sqLiteInstance.db.Table<TagInfo>().FirstOrDefault(x =>
                    x.ConnectId == SelectedConnection.ID &&
                    x.DataBaseName == SelectedDataBase &&
                    x.TagId != SelectedTag.TagId &&
                    x.TagName == tagName);
                if (tag != null)
                {
                    Oops.Oh("已存在相同名称的标签.");
                    return;
                }
                tag = sqLiteInstance.db.Get<TagInfo>(x => x.TagId == SelectedTag.TagId);
                tag.TagName = tagName;
                sqLiteInstance.db.Update(tag);
            }
            if (ChangeRefreshEvent != null)
            {
                ChangeRefreshEvent();
            }
            this.Close();
            Oops.Success("保存成功.");
        }

        /// <summary>
        /// Enter键一键保存
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TagName_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnSave_Click(sender, e);
            }
        }

        /// <summary>
        /// 取消
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 文本框变更事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TagName_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TagName.Text.Trim()))
            {
                TextErrorMsg.Visibility = Visibility.Collapsed;
            }
        }
    }
}
