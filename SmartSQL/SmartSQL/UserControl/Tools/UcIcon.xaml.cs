using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using SmartSQL.Helper;
using SmartSQL.Views;
using hyjiacan.py4n;
using ToolGood.Words;
using System.Collections.Generic;
using FontAwesome.WPF;
using FontAwesome6;
using System.Windows.Input;
using System.Threading.Tasks;

namespace SmartSQL.UserControl
{
    /// <summary>
    /// MainContent.xaml 的交互逻辑
    /// </summary>
    public partial class UcIcon : BaseUserControl
    {

        public static readonly DependencyProperty IconListProperty = DependencyProperty.Register(
            "IconList", typeof(List<string>), typeof(UcIcon), new PropertyMetadata(default(List<string>)));

        public static readonly DependencyProperty Icon6ListProperty = DependencyProperty.Register(
            "Icon6List", typeof(List<string>), typeof(UcIcon), new PropertyMetadata(default(List<string>)));

        public List<string> IconList
        {
            get => (List<string>)GetValue(IconListProperty);
            set
            {
                SetValue(IconListProperty, value);
                OnPropertyChanged(nameof(IconList));
            }
        }
        public List<string> Icon6List
        {
            get => (List<string>)GetValue(Icon6ListProperty);
            set
            {
                SetValue(Icon6ListProperty, value);
                OnPropertyChanged(nameof(Icon6List));
            }
        }

        public UcIcon()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void BaseUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            var icons = new List<string>();
            var icon6s = new List<string>();
            Task.Run(() =>
            {
                //foreach (var value in Enum.GetValues(typeof(FontAwesomeIcon)))
                //{
                //    if (value.ToString().Equals("None"))
                //    {
                //        continue;
                //    }
                //    icons.Add(value.ToString());
                //}


                icon6s = Enum.GetValues(typeof(EFontAwesomeIcon)).Cast<EFontAwesomeIcon>()
                    .Where(x => !x.ToString().ToLower().Equals("none"))
                    .Select(x => x.ToString())
                    .ToList();
                //foreach (var value in Enum.GetValues(typeof(EFontAwesomeIcon)))
                //{
                //    if (value.ToString().Equals("None"))
                //    {
                //        continue;
                //    }
                //    icon6s.Add(value.ToString());
                //}
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    IconList = icons;
                    Icon6List = icon6s;
                }));
            });
        }

        /// <summary>
        /// 返回
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnReturn_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = (ToolBox)Window.GetWindow(this);
            parentWindow.UcBox.Content = new UcMainTools();
        }
        private void UIElement_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var iconText = ((Border)sender).DataContext.ToString();
                var xamlText = $"<fa:FontAwesome Icon=\"{iconText}\">";
                TxtFont.Text = xamlText;
                Clipboard.SetText(iconText);
                Oops.Success($"【{iconText}】已复制到剪切板");
            }
            catch { }
        }

        private void BtnCopy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var xamlText = TxtFont.Text;
                Clipboard.SetText(xamlText);
                TxtFont.SelectAll();
            }
            catch (Exception) { }
        }

        private void SearchIcon_TextChanged(object sender, TextChangedEventArgs e)
        {
            var icon6s = new List<string>();
            var searchText = SearchIcon.Text.Trim();
            if (string.IsNullOrEmpty(searchText))
            {
                Task.Run(() =>
                {
                    icon6s = Enum.GetValues(typeof(EFontAwesomeIcon)).Cast<EFontAwesomeIcon>()
                        .Where(x => !x.ToString().ToLower().Equals("none"))
                        .Select(x => x.ToString())
                        .ToList();
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        Icon6List = icon6s;
                    }));
                });
                return;
            }
            icon6s = Enum.GetValues(typeof(EFontAwesomeIcon)).Cast<EFontAwesomeIcon>()
                .Where(x => x.ToString().ToLower().Contains(searchText.ToLower()))
                .Select(x => x.ToString())
                .ToList();
            Icon6List = icon6s;
        }

    }
}
