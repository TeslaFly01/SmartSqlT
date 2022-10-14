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
using ICSharpCode.AvalonEdit.Highlighting;
using JinianNet.JNTemplate;
using SmartSQL.Framework;
using SmartSQL.Framework.Exporter;
using SmartSQL.Framework.Lang;
using SmartSQL.Framework.PhysicalDataModel;
using SmartSQL.Framework.SqliteModel;
using SmartSQL.Framework.Util;
using SmartSQL.Helper;
using SmartSQL.Models;
using SqlSugar;
using Path = System.IO.Path;
using UserControlE = System.Windows.Controls.UserControl;

namespace SmartSQL.UserControl
{
    /// <summary>
    /// UTabCode.xaml 的交互逻辑
    /// </summary>
    public partial class UcTabCode : BaseUserControl
    {
        public static readonly DependencyProperty SelectedObjectProperty = DependencyProperty.Register(
            "SelectedObject", typeof(TreeNodeItem), typeof(UcTabCode), new PropertyMetadata(default(TreeNodeItem)));

        public static readonly DependencyProperty SelectedConnectionProperty = DependencyProperty.Register(
            "SelectedConnection", typeof(ConnectConfigs), typeof(UcTabCode), new PropertyMetadata(default(ConnectConfigs)));

        public static readonly DependencyProperty SelectedDataBaseProperty = DependencyProperty.Register(
            "SelectedDataBase", typeof(string), typeof(UcTabCode), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty SelectedTableColumnsProperty = DependencyProperty.Register(
            "SelectedTableColumns", typeof(List<Column>), typeof(UcTabCode), new PropertyMetadata(default(List<Column>)));
        /// <summary>
        /// 当前对象
        /// </summary>
        public TreeNodeItem SelectedObject
        {
            get => (TreeNodeItem)GetValue(SelectedObjectProperty);
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
        /// <summary>
        /// 当前数据库
        /// </summary>
        public string SelectedDataBase
        {
            get => (string)GetValue(SelectedDataBaseProperty);
            set => SetValue(SelectedDataBaseProperty, value);
        }
        /// <summary>
        /// 当前选中表列数据
        /// </summary>
        public List<Column> SelectedTableColumns
        {
            get => (List<Column>)GetValue(SelectedTableColumnsProperty);
            set => SetValue(SelectedTableColumnsProperty, value);
        }

        public UcTabCode()
        {
            InitializeComponent();
            DataContext = this;
            HighlightingProvider.Register(SkinType.Dark, new HighlightingProviderDark());
            TextCsharpEditor.SyntaxHighlighting = HighlightingProvider.GetDefinition(SkinType.Dark, "SQL");
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
            ListBoxLanguage.SelectedIndex = 0;
            var instance = ExporterFactory.CreateInstance(SelectedConnection.DbType, SelectedObject.DisplayName, SelectedTableColumns);
            //建表sql
            var createTableSql = instance.CreateTableSql();
            TextCsharpEditor.Text = createTableSql;

        }

        /// <summary>
        /// 变更语言事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListBoxLanguage_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
            {
                return;
            }
            var instance = ExporterFactory.CreateInstance(SelectedConnection.DbType, SelectedObject.DisplayName, SelectedTableColumns);
            var selLan = (ListBoxItem)ListBoxLanguage.SelectedItem;
            var highlightingDefinition = HighlightingProvider.GetDefinition(SkinType.Dark, "SQL");
            switch (selLan.Content)
            {
                case "SQL":
                    //建表sql
                    var createTableSql = instance.CreateTableSql();
                    TextCsharpEditor.Text = createTableSql;
                    break;
                case "C#":
                    highlightingDefinition = HighlightingProvider.GetDefinition(SkinType.Dark, "C#");
                    var langInstance = LangFactory.CreateInstance(LangType.Csharp, SelectedObject.Name, SelectedObject.Comment, SelectedTableColumns);
                    TextCsharpEditor.Text = langInstance.BuildEntity();
                    break;
                case "Java":
                    highlightingDefinition = HighlightingProvider.GetDefinition(SkinType.Dark, "Java");
                    
                    break;
                case "PHP":
                    highlightingDefinition = HighlightingProvider.GetDefinition(SkinType.Dark, "PHP"); break;
                case "C++": break;
                case "ObjectC": break;

            }
            TextCsharpEditor.SyntaxHighlighting = highlightingDefinition;
        }
    }
}
