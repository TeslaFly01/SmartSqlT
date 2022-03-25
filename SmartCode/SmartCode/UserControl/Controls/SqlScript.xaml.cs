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
using HandyControl.Controls;
using HandyControl.Data;
using SmartCode.Annotations;
using SmartCode.Framework;
using SmartCode.Framework.Util;

namespace SmartCode.UserControl.Controls
{
    /// <summary>
    /// SqlScript.xaml 的交互逻辑
    /// </summary>
    public partial class SqlScript : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static readonly DependencyProperty SqlTextProperty = DependencyProperty.Register(
            "SqlText", typeof(string), typeof(SqlScript), new PropertyMetadata(default(string)));
        /// <summary>
        /// 提示文字
        /// </summary>
        public string SqlText
        {
            get => (string)GetValue(SqlTextProperty);
            set
            {
                SetValue(SqlTextProperty, value);
                OnPropertyChanged(nameof(SqlText));
            }
        }
        public SqlScript()
        {
            InitializeComponent();
            DataContext = this;
            HighlightingProvider.Register(SkinType.Dark, new HighlightingProviderDark());
            TextEditor.SyntaxHighlighting = HighlightingProvider.GetDefinition(SkinType.Dark, "SQL");
        }

        private void SqlScript_OnLoaded(object sender, RoutedEventArgs e)
        {
            TextEditor.Text = SqlText.SqlFormat();
        }

        /// <summary>
        /// 复制脚本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCopyScript_OnClick(object sender, RoutedEventArgs e)
        {
            Clipboard.SetDataObject(SqlText.SqlFormat());
            Growl.SuccessGlobal(new GrowlInfo { Message = "脚本已复制到剪切板.", WaitTime = 1, ShowDateTime = false });
        }
    }
}
