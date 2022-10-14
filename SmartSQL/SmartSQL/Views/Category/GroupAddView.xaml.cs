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
    public partial class GroupAddView : INotifyPropertyChanged
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

        public static readonly DependencyProperty SelectedGroupProperty = DependencyProperty.Register(
            "SelectedGroup", typeof(ObjectGroup), typeof(TagAddView), new PropertyMetadata(default(ObjectGroup)));
        public ObjectGroup SelectedGroup
        {
            get => (ObjectGroup)GetValue(SelectedGroupProperty);
            set
            {
                SetValue(SelectedGroupProperty, value);
                OnPropertyChanged(nameof(SelectedGroup));
            }
        }
        #endregion

        public GroupAddView()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, (Action)(() =>
                {
                    Keyboard.Focus(TextGroupName);
                    if (SelectedGroup != null)
                    {
                        Title = "修改分组";
                        CheckCurrent.IsChecked = SelectedGroup.OpenLevel == 1;
                        CheckChild.IsChecked = SelectedGroup.OpenLevel == 2;
                        CheckNone.IsChecked = SelectedGroup.OpenLevel == null || SelectedGroup.OpenLevel == 0;
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
            var groupName = TextGroupName.Text.Trim();
            if (string.IsNullOrEmpty(groupName))
            {
                TextErrorMsg.Visibility = Visibility.Visible;
                return;
            }
            var sqLiteInstance = SQLiteHelper.GetInstance();
            if (SelectedGroup == null)
            {
                var tag = sqLiteInstance.db.Table<ObjectGroup>().FirstOrDefault(x =>
                    x.ConnectId == SelectedConnection.ID &&
                    x.DataBaseName == SelectedDataBase &&
                    x.GroupName == groupName);
                if (tag != null)
                {
                    Oops.Oh("已存在相同名称的分组.");
                    return;
                }
                //插入标签数据
                sqLiteInstance.db.Insert(new ObjectGroup()
                {
                    ConnectId = SelectedConnection.ID,
                    DataBaseName = SelectedDataBase,
                    GroupName = groupName
                });
            }
            else
            {
                var tag = sqLiteInstance.db.Table<ObjectGroup>().FirstOrDefault(x =>
                    x.ConnectId == SelectedConnection.ID &&
                    x.DataBaseName == SelectedDataBase &&
                    x.Id != SelectedGroup.Id &&
                    x.GroupName == groupName);
                if (tag != null)
                {
                    Oops.Oh("已存在相同名称的分组.");
                    return;
                }
                tag = sqLiteInstance.db.Get<ObjectGroup>(x => x.Id == SelectedGroup.Id);
                tag.GroupName = groupName;
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
        private void TextGroupName_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TextGroupName.Text.Trim()))
            {
                TextErrorMsg.Visibility = Visibility.Collapsed;
            }
        }
    }
}
