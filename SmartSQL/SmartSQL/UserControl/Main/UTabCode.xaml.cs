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
    public partial class UTabCode : BaseUserControl
    {
        public static readonly DependencyProperty SelectedObjectProperty = DependencyProperty.Register(
            "SelectedObject", typeof(TreeNodeItem), typeof(UTabCode), new PropertyMetadata(default(TreeNodeItem)));

        public static readonly DependencyProperty SelectedConnectionProperty = DependencyProperty.Register(
            "SelectedConnection", typeof(ConnectConfigs), typeof(UTabCode), new PropertyMetadata(default(ConnectConfigs)));

        public static readonly DependencyProperty SelectedDataBaseProperty = DependencyProperty.Register(
            "SelectedDataBase", typeof(string), typeof(UTabCode), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty SelectedTableColunmsProperty = DependencyProperty.Register(
            "SelectedTableColunms", typeof(List<Column>), typeof(UTabCode), new PropertyMetadata(default(List<Column>)));

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
            var instance = ExporterFactory.CreateInstance(SelectedConnection.DbType, SelectedObject.DisplayName, SelectedTableColunms);
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
            var instance = ExporterFactory.CreateInstance(SelectedConnection.DbType, SelectedObject.DisplayName, SelectedTableColunms);
            var selLan = (ListBoxItem)ListBoxLanguage.SelectedItem;
            switch (selLan.Content)
            {
                case "SQL":
                    TextCsharpEditor.SyntaxHighlighting = HighlightingProvider.GetDefinition(SkinType.Dark, "SQL");
                    //建表sql
                    var createTableSql = instance.CreateTableSql();
                    TextCsharpEditor.Text = createTableSql;
                    break;
                case "C#":
                    TextCsharpEditor.SyntaxHighlighting = HighlightingProvider.GetDefinition(SkinType.Dark, "C#");
                    var langInstance = LangFactory.CreateInstance(LangType.Csharp, SelectedObject.Name, SelectedObject.Comment, SelectedTableColunms);
                    TextCsharpEditor.Text = langInstance.BuildEntity();
                    break;
                case "Java":
                    TextCsharpEditor.SyntaxHighlighting = HighlightingProvider.GetDefinition(SkinType.Dark, "Java");

                    string TTF_Path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources\\template\\Csharp.tmpl");
                    var template = Engine.LoadTemplate(TTF_Path);
                    template.Set("ClassInfo", SelectedObject);
                    template.Set("FieldList", SelectedTableColunms);
                    var result = template.Render();
                    break;
                case "PHP":
                    TextCsharpEditor.SyntaxHighlighting = HighlightingProvider.GetDefinition(SkinType.Dark, "PHP"); break;
                case "C++": break;
                case "ObjectC": break;
            }

        }
    }
}
