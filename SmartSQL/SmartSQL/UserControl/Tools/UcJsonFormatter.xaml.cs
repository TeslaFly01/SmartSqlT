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
using ComboBox = System.Windows.Controls.ComboBox;
using FontAwesome = FontAwesome.WPF.FontAwesome;
using TabControl = System.Windows.Controls.TabControl;
using TabItem = System.Windows.Controls.TabItem;
using HandyControl.Data;
using HandyControl.Controls;
using SqlSugar.DistributedSystem.Snowflake;

namespace SmartSQL.UserControl
{
    /// <summary>
    /// MainContent.xaml 的交互逻辑
    /// </summary>
    public partial class UcJsonFormatter : BaseUserControl
    {
        public UcJsonFormatter()
        {
            InitializeComponent();
            DataContext = this;
            HighlightingProvider.Register(SkinType.Dark, new HighlightingProviderDark());
            TextEditor.SyntaxHighlighting = HighlightingProvider.GetDefinition(SkinType.Dark, "SQL");
            TextEditor.TextArea.SelectionCornerRadius = 0;
            TextEditor.TextArea.SelectionBorder = null;
            TextEditor.TextArea.SelectionForeground = null;
            TextEditor.TextArea.SelectionBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFADD6FF"));
        }

        private void BtnFormatter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TextEditor.Text = StrUtil.JsonFormatter(TextEditor.Text);
            }
            catch (Exception)
            {
                Oops.Oh("Json解析失败，请检查");
            }
        }

        private void BtnReturn_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = (MainWindow)System.Windows.Window.GetWindow(this);
            parentWindow.UcMainTools.Content=new UcMainTools();
        }

        private void BtnCopy_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TextEditor.Text))
            {
                TextEditor.SelectAll();
                Clipboard.SetDataObject(TextEditor.Text);
                Oops.Success("文本已复制到剪切板");
            }
        }
    }
}
