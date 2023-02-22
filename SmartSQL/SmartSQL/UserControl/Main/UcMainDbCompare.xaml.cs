using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
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
using SmartSQL.Framework.PhysicalDataModel;
using SmartSQL.Models;
using System.Runtime.CompilerServices;
using SmartSQL.Framework;
using SmartSQL.Framework.SqliteModel;
using SmartSQL.Annotations;
using SmartSQL.DocUtils;
using SmartSQL.Framework.Const;
using SmartSQL.Helper;
using SmartSQL.UserControl;
using SmartSQL.Views;
using SqlSugar;
using ComboBox = System.Windows.Controls.ComboBox;
using FontAwesome = FontAwesome.WPF.FontAwesome;
using TabControl = System.Windows.Controls.TabControl;
using TabItem = System.Windows.Controls.TabItem;

namespace SmartSQL.UserControl
{
    /// <summary>
    /// MainContent.xaml 的交互逻辑
    /// </summary>
    public partial class UcMainDbCompare : BaseUserControl
    {
        public UcMainDbCompare()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void UcMainDbCompare_OnLoaded(object sender, RoutedEventArgs e)
        {
            var sqLiteHelper = SQLiteHelper.GetInstance();
            var connectConfigs = sqLiteHelper.ToList<ConnectConfigs>();
            ComSourceConnect.ItemsSource = null;
            ComSourceConnect.ItemsSource = connectConfigs;
            ComTargetConnect.ItemsSource = null;
            ComTargetConnect.ItemsSource = connectConfigs;
        }

        /// <summary>
        /// 选中源连接加载对应数据库
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComSourceConnect_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }
            var selConnectConfig = (ConnectConfigs)((ComboBox)sender).SelectedItem;
            var dbInstance = ExporterFactory.CreateInstance(selConnectConfig.DbType, selConnectConfig.DbMasterConnectString);
            var list = dbInstance.GetDatabases(selConnectConfig.DefaultDatabase);
            ComSourceDatabase.ItemsSource = list;
            if (selConnectConfig.DbType == DbType.PostgreSQL)
            {
                ComSourceDatabase.SelectedItem = list.FirstOrDefault(x => x.DbName.EndsWith("public"));
                return;
            }
            ComSourceDatabase.SelectedItem = list.FirstOrDefault(x => x.DbName == selConnectConfig.DefaultDatabase);
        }

        /// <summary>
        /// 选中目标连接加载对应数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComTargetConnect_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }
            var selConnectConfig = (ConnectConfigs)((ComboBox)sender).SelectedItem;
            var dbInstance = ExporterFactory.CreateInstance(selConnectConfig.DbType, selConnectConfig.DbMasterConnectString);
            var list = dbInstance.GetDatabases(selConnectConfig.DefaultDatabase);
            ComTargetDatabase.ItemsSource = list;
            if (selConnectConfig.DbType == DbType.PostgreSQL)
            {
                ComTargetDatabase.SelectedItem = list.FirstOrDefault(x => x.DbName.EndsWith("public"));
                return;
            }
            ComTargetDatabase.SelectedItem = list.FirstOrDefault(x => x.DbName == selConnectConfig.DefaultDatabase);
        }
    }
}
