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
using SmartCode.Framework;
using SmartCode.Framework.PhysicalDataModel;
using SmartCode.Framework.SqliteModel;
using SmartCode.Framework.Util;
using SmartCode.Models;
using SqlSugar;

namespace SmartCode.Views
{
    /// <summary>
    /// ScriptWindow.xaml 的交互逻辑
    /// </summary>
    public partial class ScriptWindow
    {
        public static readonly DependencyProperty SelectedConnectionProperty = DependencyProperty.Register(
            "SelectedConnection", typeof(ConnectConfigs), typeof(ScriptWindow), new PropertyMetadata(default(ConnectConfigs)));
        /// <summary>
        /// 当前连接
        /// </summary>
        public ConnectConfigs SelectedConnection
        {
            get => (ConnectConfigs)GetValue(SelectedConnectionProperty);
            set => SetValue(SelectedConnectionProperty, value);
        }

        public static readonly DependencyProperty SelectedObjectProperty = DependencyProperty.Register(
            "SelectedObject", typeof(PropertyNodeItem), typeof(ScriptWindow), new PropertyMetadata(default(PropertyNodeItem)));
        /// <summary>
        /// 选中对象
        /// </summary>
        public PropertyNodeItem SelectedObject
        {
            get => (PropertyNodeItem)GetValue(SelectedObjectProperty);
            set => SetValue(SelectedObjectProperty, value);
        }

        public static readonly DependencyProperty SelectedColumnsProperty = DependencyProperty.Register(
            "SelectedColumns", typeof(List<Column>), typeof(ScriptWindow), new PropertyMetadata(default(List<Column>)));
        /// <summary>
        /// 选中字段
        /// </summary>
        public List<Column> SelectedColumns
        {
            get => (List<Column>)GetValue(SelectedColumnsProperty);
            set => SetValue(SelectedColumnsProperty, value);
        }
        public ScriptWindow()
        {
            InitializeComponent();
        }

        private void ScriptWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            var objectName = SelectedObject.DisplayName;
            var colList = SelectedColumns;
            var instance = ExporterFactory.CreateInstance(SelectedConnection.DbType, objectName, colList);
            if (TabSelectSql.IsSelected)
            {
                //查询sql
                var selSql = instance.SelectSql();
                TxtSelectSql.SqlText = selSql;
            }
            Task.Run(() =>
            {
                var ddlSql = instance.CreateTableSql();
                //插入sql
                var insSql = instance.InsertSql();
                //更新sql
                var updSql = instance.UpdateSql();
                //删除sql
                var delSql = instance.DeleteSql();
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    TxtCreateSql.SqlText = ddlSql;
                    TxtInsertSql.SqlText = insSql;
                    TxtUpdateSql.SqlText = updSql;
                    TxtDeleteSql.SqlText = delSql;
                }));
            });
        }

        private void BtnClose_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
