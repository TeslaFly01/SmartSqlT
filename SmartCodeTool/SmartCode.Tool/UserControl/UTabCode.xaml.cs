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
using SmartCode.Framework.Exporter;
using SmartCode.Framework.PhysicalDataModel;
using SmartCode.Framework.Util;
using SmartCode.Tool.Models;
using UserControlE = System.Windows.Controls.UserControl;

namespace SmartCode.Tool.UserControl
{
    /// <summary>
    /// UTabCode.xaml 的交互逻辑
    /// </summary>
    public partial class UTabCode : BaseUserControl
    {
        public static readonly DependencyProperty SelectedObjectProperty = DependencyProperty.Register(
            "SelectedObject", typeof(PropertyNodeItem), typeof(UTabCode), new PropertyMetadata(default(PropertyNodeItem)));

        public static readonly DependencyProperty ConnStringProperty = DependencyProperty.Register(
            "ConnString", typeof(string), typeof(UTabCode), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty SelectedDataBaseProperty = DependencyProperty.Register(
            "SelectedDataBase", typeof(string), typeof(UTabCode), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty SelectedTableColunmsProperty = DependencyProperty.Register(
            "SelectedTableColunms", typeof(List<Column>), typeof(UTabCode), new PropertyMetadata(default(List<Column>)));

        public PropertyNodeItem SelectedObject
        {
            get => (PropertyNodeItem)GetValue(SelectedObjectProperty);
            set => SetValue(SelectedObjectProperty, value);
        }
        public string ConnString
        {
            get => (string)GetValue(ConnStringProperty);
            set => SetValue(ConnStringProperty, value);
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
            TextTableEditor.SyntaxHighlighting = HighlightingProvider.GetDefinition(SkinType.Dark, "SQL");
            TextColumnsEditor.SyntaxHighlighting = HighlightingProvider.GetDefinition(SkinType.Dark, "SQL");
            TextCsharpEditor.SyntaxHighlighting = HighlightingProvider.GetDefinition(SkinType.Dark, "C#");
        }

        private void TabCode_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (SelectedObject == null)
            {
                return;
            }
            TabParentSql.IsSelected = true;
            TabTableSql.IsSelected = true;
            var objE = SelectedObject;
            
            var list = SelectedTableColunms;
            var sb = new StringBuilder();

            #region 1、生成新建表脚本
            sb.Append($"CREATE TABLE {objE.DisplayName}(");
            sb.Append(Environment.NewLine);
            foreach (var column in list)
            {
                sb.Append($"\t{column.DisplayName} {column.DataType}{column.Length} ");
                if (column.IsIdentity)
                {
                    sb.Append("IDENTITY(1,1) ");
                }
                var isNull = column.IsNullable ? "NULL," : "NOT NULL,";
                sb.Append(isNull);
                sb.Append(Environment.NewLine);
            }
            var primaryKeyList = list.FindAll(x => x.IsPrimaryKey);
            if (primaryKeyList.Any())
            {
                sb.Append($"\tPRIMARY KEY (");
                var sbPriKey = new StringBuilder();
                foreach (var column in primaryKeyList)
                {
                    sbPriKey.Append($"{column.DisplayName},");
                }
                sb.Append(sbPriKey.ToString().TrimEnd(','));
                sb.Append(")");
                sb.Append(Environment.NewLine);
            }
            sb.Append(")");
            TextTableEditor.Text = sb.ToString();
            #endregion

            sb.Clear();
            #region 2、生成新建字段脚本
            foreach (var column in list)
            {
                sb.Append($"ALTER TABLE dbo.{column.ObjectName} ADD {column.DisplayName} {column.DataType.ToLower()} ");
                if (SqlServerDbTypeMapHelper.IsMulObj(column.DataType))
                {
                    sb.Append($"{column.Length} ");
                }
                var isNull = column.IsNullable ? "NULL " : "NOT NULL ";
                sb.Append(isNull);
                sb.Append(Environment.NewLine);
                sb.Append("GO");
                sb.Append(Environment.NewLine);
                #region 字段注释
                if (!string.IsNullOrEmpty(column.Comment))
                {
                    sb.Append($@"EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'{column.Comment}' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'{column.ObjectName}', @level2type=N'COLUMN',@level2name=N'{column.DisplayName}'");
                    sb.Append(Environment.NewLine);
                    sb.Append("GO");
                    sb.Append(Environment.NewLine);
                }
                #endregion
            }
            TextColumnsEditor.Text = sb.ToString();
            #endregion

            sb.Clear();
            sb.Append("using System;");
            sb.Append(Environment.NewLine);
            sb.Append("using System.Collections.Generic;");
            sb.Append(Environment.NewLine);
            sb.Append("namespace Test");
            sb.Append(Environment.NewLine);
            sb.Append("{");
            sb.Append(Environment.NewLine);
            sb.Append($"\tpublic class {objE.Name}");
            sb.Append(Environment.NewLine);
            sb.Append("\t{");
            sb.Append(Environment.NewLine);
            sb.Append($"\t\tpublic {objE.Name}()");
            sb.Append(Environment.NewLine);
            sb.Append("\t\t{");
            sb.Append(Environment.NewLine);
            sb.Append("\t\t}");
            sb.Append(Environment.NewLine);
            foreach (var column in list)
            {
                sb.Append("\t\t///<summary>");
                sb.Append(Environment.NewLine);
                sb.Append($"\t\t///{column.Comment}");
                sb.Append(Environment.NewLine);
                sb.Append("\t\t///</summary>");
                sb.Append(Environment.NewLine);
                sb.Append($"\t\tpublic {column.CSharpType} {column.DisplayName} {{ get; set; }}");
                sb.Append("\t\t");
                sb.Append(Environment.NewLine);
            }
            sb.Append("\t}");
            sb.Append(Environment.NewLine);
            sb.Append("}");
            TextCsharpEditor.Text = sb.ToString();
        }
    }
}
