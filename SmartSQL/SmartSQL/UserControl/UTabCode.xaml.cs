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
using HandyControl.Data;
using SmartSQL.Framework;
using SmartSQL.Framework.Exporter;
using SmartSQL.Framework.PhysicalDataModel;
using SmartSQL.Framework.SqliteModel;
using SmartSQL.Framework.Util;
using SmartSQL.Helper;
using SmartSQL.Models;
using SqlSugar;
using UserControlE = System.Windows.Controls.UserControl;

namespace SmartSQL.UserControl
{
    /// <summary>
    /// UTabCode.xaml 的交互逻辑
    /// </summary>
    public partial class UTabCode : BaseUserControl
    {
        public static readonly DependencyProperty SelectedObjectProperty = DependencyProperty.Register(
            "SelectedObject", typeof(PropertyNodeItem), typeof(UTabCode), new PropertyMetadata(default(PropertyNodeItem)));

        public static readonly DependencyProperty SelectedConnectionProperty = DependencyProperty.Register(
            "SelectedConnection", typeof(ConnectConfigs), typeof(UTabCode), new PropertyMetadata(default(ConnectConfigs)));

        public static readonly DependencyProperty SelectedDataBaseProperty = DependencyProperty.Register(
            "SelectedDataBase", typeof(string), typeof(UTabCode), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty SelectedTableColunmsProperty = DependencyProperty.Register(
            "SelectedTableColunms", typeof(List<Column>), typeof(UTabCode), new PropertyMetadata(default(List<Column>)));

        public PropertyNodeItem SelectedObject
        {
            get => (PropertyNodeItem)GetValue(SelectedObjectProperty);
            set => SetValue(SelectedObjectProperty, value);
        }
        /// <summary>
        /// 当前数据连接
        /// </summary>
        public ConnectConfigs SelectedConnection
        {
            get => (ConnectConfigs)GetValue(SelectedConnectionProperty);
            set => SetValue(SelectedConnectionProperty, value);
        }
        public string SelectedDataBase
        {
            get => (string)GetValue(SelectedDataBaseProperty);
            set => SetValue(SelectedDataBaseProperty, value);
        }
        /// <summary>
        /// 当前选中表列数据
        /// </summary>
        public List<Column> SelectedTableColunms
        {
            get => (List<Column>)GetValue(SelectedTableColunmsProperty);
            set
            {
                SetValue(SelectedTableColunmsProperty, value);
            }
        }
        public UTabCode()
        {
            InitializeComponent();
            DataContext = this;
            HighlightingProvider.Register(SkinType.Dark, new HighlightingProviderDark());
            TextCsharpEditor.SyntaxHighlighting = HighlightingProvider.GetDefinition(SkinType.Dark, "C#");
            TextCsharpEditor.TextArea.SelectionCornerRadius = 0;
            TextCsharpEditor.TextArea.SelectionBorder = null;
            TextCsharpEditor.TextArea.SelectionForeground = null;
            TextCsharpEditor.TextArea.SelectionBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFADD6FF"));
        }

        private void TabCode_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (SelectedObject == null)
            {
                return;
            }
            TabParentSql.IsSelected = true;
            TabSelectSql.IsSelected = true;
            var objE = SelectedObject;

            var list = SelectedTableColunms;
            
            var instance = ExporterFactory.CreateInstance(SelectedConnection.DbType, objE.DisplayName, list);
            //建表sql
            var createTableSql = instance.CreateTableSql();
            TxtCreateSql.SqlText = createTableSql;
            //建字段sql
            var alterAdd = instance.AddColumnSql();
            TxtAddColumnSql.SqlText = alterAdd;
            //查询sql
            var selSql = instance.SelectSql();
            TxtSelectSql.SqlText = selSql;
            //插入sql
            var insSql = instance.InsertSql();
            TxtInsertSql.SqlText = insSql;
            //更新sql
            var updSql = instance.UpdateSql();
            TxtUpdateSql.SqlText = updSql;
            //删除sql
            var delSql = instance.DeleteSql();
            TxtDeleteSql.SqlText = delSql;
            var langInstance = LangFactory.CreateInstance(LangType.Csharp, objE.Name, list);
            var csharpEntityCode = langInstance.BuildEntity();
            TextCsharpEditor.Text = csharpEntityCode;
        }
    }
}
