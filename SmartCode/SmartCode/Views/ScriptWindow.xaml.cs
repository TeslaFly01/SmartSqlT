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
            var instance = ExporterFactory.CreateInstance(DbType.SqlServer);
            if (TabCreateSql.IsSelected)
            {
                var createTableSql = instance.CreateTableSql(SelectedObject.DisplayName, SelectedColumns);
                TxtCreateSql.SqlText = createTableSql;
            }
            var objectName = SelectedObject.Name;
            var colList = SelectedColumns;
            Task.Run(() =>
            {
                //查询sql
                var selSql = instance.SelectSql(objectName, colList);
                //插入sql
                var insSql = instance.InsertSql(objectName, colList);
                //更新sql
                var updSql = instance.UpdateSql(objectName, colList);
                //删除sql
                var delSql = instance.DeleteSql(objectName, colList);
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    TxtSelectSql.SqlText = selSql.SqlFormat();
                    TxtInsertSql.SqlText = insSql.SqlFormat();
                    TxtUpdateSql.SqlText = updSql.SqlFormat();
                    TxtDeleteSql.SqlText = delSql.SqlFormat();
                }));
            });
        }
    }
}
