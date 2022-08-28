using HandyControl.Controls;
using HandyControl.Data;
using SmartSQL.Framework;
using SmartSQL.Framework.SqliteModel;
using SmartSQL.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
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

namespace SmartSQL.Views.Category
{
    /// <summary>
    /// TagAddView.xaml 的交互逻辑
    /// </summary>
    public partial class TagAddView
    {
        public event ChangeRefreshHandler ChangeRefreshEvent;
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
        #endregion

        public TagAddView()
        {
            InitializeComponent();
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
                Oops.Oh("标签名称为空.");
                return;
            }
            var sqLiteInstance = SQLiteHelper.GetInstance();
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
            if (ChangeRefreshEvent!=null)
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
    }
}
