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
using ICSharpCode.AvalonEdit;
using SmartSQL.UserControl.Tags;
using SmartSQL.Framework.Util;
using System.Dynamic;

namespace SmartSQL.UserControl
{
    /// <summary>
    /// MainContent.xaml 的交互逻辑
    /// </summary>
    public partial class UcPasswordGen : BaseUserControl
    {
        /// <summary>
        /// 结果数据列表
        /// </summary>
        public static readonly DependencyProperty PaswordListProperty = DependencyProperty.Register(
            "PaswordList", typeof(List<PasswordResultDTO>), typeof(UcPasswordGen), new PropertyMetadata(default(List<PasswordResultDTO>)));
        public List<PasswordResultDTO> PaswordList
        {
            get => (List<PasswordResultDTO>)GetValue(PaswordListProperty);
            set
            {
                SetValue(PaswordListProperty, value);
                OnPropertyChanged(nameof(PaswordList));
            }
        }

        public UcPasswordGen()
        {
            InitializeComponent();
            DataContext = this;
            TableGrid.PreviewMouseWheel += (sender, e) =>
            {
                var eventArg = new System.Windows.Input.MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta);
                eventArg.RoutedEvent = System.Windows.UIElement.MouseWheelEvent;
                eventArg.Source = sender;
                this.TableGrid.RaiseEvent(eventArg);
            };
        }

        private void BtnReturn_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = (MainWindow)System.Windows.Window.GetWindow(this);
            parentWindow.UcMainTools.Content = new UcMainTools();
        }

        private void BtnGen_Click(object sender, RoutedEventArgs e)
        {
            var pwdNums = Convert.ToInt32(TextPwdNums.Value);
            var pwdLens = Convert.ToInt32(TextPwdLens.Value);
            var isNums = CheckNums.IsChecked;
            var isLowerCase = CheckLowerCase.IsChecked;
            var isUpperCase = CheckUpperCase.IsChecked;
            var isSymbol = CheckSymbol.IsChecked;
            var pwds = new List<PasswordResultDTO>();

            var strPwds = StrUtil.GeneratePassword(pwdLens, pwdNums, isNums.Value, isLowerCase.Value, isUpperCase.Value, isSymbol.Value);
            strPwds.ForEach(pwd =>
            {
                pwds.Add(new PasswordResultDTO
                {
                    Password = pwd
                });
            });

            PaswordList = pwds;
        }
    }
}
