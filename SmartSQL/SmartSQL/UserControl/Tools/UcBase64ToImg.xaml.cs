using System;
using System.Drawing;
using System.IO;
using System.Windows;
using SmartSQL.Helper;
using SmartSQL.Views;

namespace SmartSQL.UserControl
{
    /// <summary>
    /// MainContent.xaml 的交互逻辑
    /// </summary>
    public partial class UcBase64ToImg : BaseUserControl
    {
        public UcBase64ToImg()
        {
            InitializeComponent();
            DataContext = this;
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

        /// <summary>
        /// 编码
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnEncode_Click(object sender, RoutedEventArgs e)
        {
            var file = ImageFile.Uri;
            if (file == null)
            {
                Oops.Oh("请选择图片");
                return;
            }
            var fileExt = Path.GetExtension(file.LocalPath);
            FileStream stream = new FileInfo(ImageFile.Uri.LocalPath).OpenRead();
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, Convert.ToInt32(stream.Length));
            var result = Convert.ToBase64String(buffer);
            TextOutput.Text = $"data:image/png;base64,{result}";
        }

        /// <summary>
        /// 清空
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            TextOutput.Text = string.Empty;
        }

        /// <summary>
        /// 复制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCopy_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextOutput.Text))
            {
                return;
            }
            Clipboard.SetDataObject(TextOutput.Text);
            Oops.Success("复制成功");
        }
    }
}
