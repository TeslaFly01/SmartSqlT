using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.Rendering;

namespace SmartSQL.Helper
{
    public class BarQrCodeHelper
    {
        /// <summary>
        /// 生成条码、二维码
        /// </summary>
        /// <param name="code">要编码内容</param>
        /// <param name="format">编码格式(条码CODE_128、二维码QR_CODE)</param>
        /// <param name="options">编码参数（条码EncodingOptions、二维码QrCodeEncodingOptions）</param>
        /// <param name="renderer">条码、二维码Bitmap参数</param>
        /// <param name="qrcodeoptions">二维码参数</param>
        /// <returns></returns>
        public static Bitmap EncodeBarQrCode(string code, BarcodeFormat format, EncodingOptions options, BitmapRenderer renderer = null, QrCodeOptions qrcodeoptions = null)
        {
            BarcodeWriter writer = new BarcodeWriter();
            writer.Options = options;
            writer.Format = format;
            if (renderer != null)
                writer.Renderer = renderer;
            Bitmap bmp = writer.Write(code);


            if ((options is QrCodeEncodingOptions) && qrcodeoptions != null && qrcodeoptions.IsLogo)
            {
                int w = qrcodeoptions.Width - qrcodeoptions.LogoPadding * 2;
                int h = qrcodeoptions.Height - qrcodeoptions.LogoPadding * 2;
                Rectangle logo_rect = new Rectangle((bmp.Width - w) / 2, (bmp.Height - h) / 2, w, h);
                if (qrcodeoptions.LogoPadding == 0)
                {
                    logo_rect = new Rectangle((bmp.Width - qrcodeoptions.Width) / 2, (bmp.Height - qrcodeoptions.Height) / 2, qrcodeoptions.Width, qrcodeoptions.Height);
                }

                Graphics bmp_g = Graphics.FromImage(bmp);
                bmp_g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                bmp_g.CompositingQuality = CompositingQuality.HighQuality;
                bmp_g.SmoothingMode = SmoothingMode.AntiAlias;

                Bitmap logo_bmp = (Bitmap)qrcodeoptions.Logo;//按比例缩放Logo
                if ((qrcodeoptions.Logo.Width > logo_rect.Width || qrcodeoptions.Logo.Height > logo_rect.Height) || (qrcodeoptions.Logo.Width < logo_rect.Width && qrcodeoptions.Logo.Height < logo_rect.Height))
                {
                    logo_bmp = new Bitmap(logo_rect.Width, logo_rect.Height);
                    Graphics logo_g = Graphics.FromImage(logo_bmp);
                    logo_g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    logo_g.CompositingQuality = CompositingQuality.HighQuality;
                    logo_g.SmoothingMode = SmoothingMode.AntiAlias;

                    Rectangle rect = new Rectangle(0, 0, 0, 0);
                    float num1 = (float)logo_rect.Width / (float)qrcodeoptions.Logo.Width;
                    float num2 = (float)logo_rect.Height / (float)qrcodeoptions.Logo.Height;
                    float num3 = Math.Min(num1, num2);
                    rect.Width = (int)((float)qrcodeoptions.Logo.Width * (float)num3);
                    rect.Height = (int)((float)qrcodeoptions.Logo.Height * (float)num3);
                    rect.X = (logo_rect.Width - rect.Width) / 2;
                    rect.Y = (logo_rect.Height - rect.Height) / 2;

                    logo_g.DrawImage((Bitmap)qrcodeoptions.Logo, rect);
                    logo_g.Dispose();
                }

                Bitmap corner_bmp = new Bitmap(logo_rect.Width, logo_rect.Height);//圆角处理
                Graphics corner_g = Graphics.FromImage(corner_bmp);
                corner_g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                corner_g.CompositingQuality = CompositingQuality.HighQuality;
                corner_g.SmoothingMode = SmoothingMode.AntiAlias;

                corner_g.SetClip(CreateRoundedRectanglePath(new Rectangle(0, 0, logo_rect.Width, logo_rect.Height), qrcodeoptions.Radius));
                corner_g.DrawImage(logo_bmp, new Rectangle(0, 0, logo_bmp.Width, logo_bmp.Height));
                logo_bmp.Dispose();

                if (qrcodeoptions.LogoPadding != 0)
                {
                    Pen padding_pen = new Pen(Color.Gray);
                    SolidBrush padding_sb = new SolidBrush(qrcodeoptions.LogoBackColor);
                    Rectangle padding_rect = new Rectangle((bmp.Width - qrcodeoptions.Width) / 2, (bmp.Height - qrcodeoptions.Height) / 2, qrcodeoptions.Width, qrcodeoptions.Height);
                    GraphicsPath padding_path = CreateRoundedRectanglePath(padding_rect, qrcodeoptions.Radius);
                    bmp_g.FillPath(padding_sb, padding_path);
                    bmp_g.DrawPath(padding_pen, padding_path);
                    padding_sb.Dispose();
                    padding_pen.Dispose();
                    padding_path.Dispose();
                }

                bmp_g.DrawImage(corner_bmp, logo_rect);

                corner_bmp.Dispose();
                corner_g.Dispose();
                bmp_g.Dispose();
            }
            return bmp;
        }

        /// <summary>
        /// 解释条码、二维码
        /// </summary>
        /// <param name="code">要解码内容</param>
        /// <param name="options">解码参数</param>
        /// <returns></returns>
        public static string DecodeBarQrCode(Bitmap code, DecodingOptions options)
        {
            BarcodeReader reader = new BarcodeReader();
            reader.Options = options;
            reader.AutoRotate = true;
            Result result = reader.Decode(code);
            return result != null ? result.Text : String.Empty;
        }

        /// <summary>
        /// 创建圆角矩形
        /// </summary>
        /// <param name="rect">区域</param>
        /// <param name="cornerRadius">圆角角度</param>
        /// <returns></returns>
        private static GraphicsPath CreateRoundedRectanglePath(Rectangle rect, int cornerRadius)
        {
            GraphicsPath result = new GraphicsPath();
            int diameter = cornerRadius * 2;

            result.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            result.AddLine(rect.X + cornerRadius, rect.Y, rect.Right - cornerRadius, rect.Y);

            result.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            result.AddLine(rect.Right, rect.Y + cornerRadius, rect.Right, rect.Bottom - cornerRadius);

            result.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            result.AddLine(rect.Right - cornerRadius, rect.Bottom, rect.X + cornerRadius, rect.Bottom);

            result.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            result.AddLine(rect.X, rect.Bottom - cornerRadius, rect.X, rect.Y + cornerRadius);

            result.CloseFigure();
            return result;
        }


        /// <summary>
        /// 二维码参数
        /// </summary>
        public class QrCodeOptions
        {
            public QrCodeOptions()
            {

            }

            public QrCodeOptions(int _Radius, bool _IsLogo, Image Logo, int _Width, int _Height, int _LogoPadding, Color _LogoBackColor, Color _LogoBorderColor)
            {
                this.Radius = _Radius;
                this.IsLogo = _IsLogo;
                this.Logo = Logo;
                this.Width = _Width;
                this.Height = _Height;
                this.LogoPadding = _LogoPadding;
                this.LogoBackColor = _LogoBackColor;
                this.LogoBorderColor = _LogoBorderColor;
            }

            /// <summary>
            /// 圆角度数
            /// </summary>
            public int Radius = 4;
            /// <summary>
            /// 是否显示Logo
            /// </summary>
            public bool IsLogo = false;
            /// <summary>
            /// Logo图片
            /// </summary>
            public Image Logo;
            /// <summary>
            /// Logo宽度
            /// </summary>
            public int Width;
            /// <summary>
            /// Logo高度
            /// </summary>
            public int Height;
            /// <summary>
            /// Logo内边距
            /// </summary>
            public int LogoPadding = 4;
            /// <summary>
            /// Logo内边距背景颜色
            /// </summary>
            public Color LogoBackColor = Color.White;
            /// <summary>
            /// Logo边框颜色
            /// </summary>
            public Color LogoBorderColor = Color.Gray;
        }
    }
}
