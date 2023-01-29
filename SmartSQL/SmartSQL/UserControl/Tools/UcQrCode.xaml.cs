using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Drawing;
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
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.QrCode.Internal;
using ZXing.Rendering;
using Color = System.Drawing.Color;

namespace SmartSQL.UserControl
{
    /// <summary>
    /// MainContent.xaml 的交互逻辑
    /// </summary>
    public partial class UcQrCode : BaseUserControl
    {
        public UcQrCode()
        {
            InitializeComponent();
            DataContext = this;
        }

        /// <summary>
        /// 生成二维码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnGeneration_OnClick(object sender, RoutedEventArgs e)
        {
            //编码内容
            var text = TextContent.Text.Trim();
            if (string.IsNullOrEmpty(text))
            {
                Oops.Oh("请输入二维码内容");
                return;
            }
            //宽
            var width = 512;
            //长
            var height = 512;
            //编码配置
            var options = new QrCodeEncodingOptions
            {
                Margin = 1,
                Width = width,
                Height = height,
                CharacterSet = "UTF-8",
                PureBarcode = true,
                ErrorCorrection = ErrorCorrectionLevel.H
            };
            //颜色配置
            var renderer = new BitmapRenderer()
            {
                //Background = Color.DarkOrange,
                //Foreground = Color.Aqua,
            };
            //自定义配置
            var qrOption = new BarQrCodeHelper.QrCodeOptions()
            {
                Height = height,
                Width = width
            };
            var bitmap = BarQrCodeHelper.EncodeBarQrCode(text, BarcodeFormat.QR_CODE, options, renderer, qrOption);
            ImageResult.Source = ChangeBitmapToImageSource(bitmap);
            TextTip.Visibility = Visibility.Collapsed;
            ImageResult.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 清空
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            TextContent.Text = string.Empty;
            TextTip.Visibility = Visibility.Visible;
            ImageResult.Visibility = Visibility.Collapsed;
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

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        public static ImageSource ChangeBitmapToImageSource(Bitmap bitmap)
        {
            IntPtr hBitmap = bitmap.GetHbitmap();
            ImageSource wpfBitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            if (!DeleteObject(hBitmap))
            {
                throw new System.ComponentModel.Win32Exception();
            }
            return wpfBitmap;
        }
    }
}
