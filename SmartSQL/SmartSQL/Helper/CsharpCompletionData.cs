using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using SmartSQL.Framework.Const;
using SmartSQL.Models;
using System.Windows.Controls;

namespace SmartSQL.Helper
{
    public class CsharpCompletionData : ICompletionData
    {
        public CsharpCompletionData(string text, string objType = null, string desc = null)
        {
            this.Text = text;
            this.Desc = desc;
            SetItem(objType);
        }
        // 智能提示的文本内容
        public string Text { get; private set; }
        //赋值默认图标
        public ImageSource Image { get; private set; }

        // 在提示列表中用于排序的属性
        public double Priority => 0;

        //设置智能提示的内容
        public object Content { get; private set; }
        public object Description { get; private set; }
        // 智能提示的描述文本
        public string Desc { get; set; }
        // 智能提示的文本颜色
        public SolidColorBrush Foreground { get; private set; }

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            var line = textArea.Document.GetLineByOffset(completionSegment.Offset);
            var lineText = textArea.Document.GetText(line);
            var lastSpaceIndex = lineText.LastIndexOf(' ');
            if (lineText.EndsWith(" ") ||lineText.EndsWith(".") || lineText.EndsWith("@"))
            {
                textArea.Document.Replace(completionSegment, this.Text);
                return;
            }
            int startOffset;
            if (lastSpaceIndex > 0)
            {
                startOffset = line.Offset + lastSpaceIndex + 1;
            }
            else
            {
                startOffset = line.Offset;
            }
            if (startOffset<0)
            {
                startOffset = 0;
            }
            ISegment newSegment = new TextSegment() { StartOffset = startOffset, EndOffset = completionSegment.EndOffset };
            textArea.Document.Replace(newSegment, this.Text);
        }

        private void SetItem(string objType)
        {
            #region MyRegion
            var black = new SolidColorBrush(Colors.Black);
            var blue = new SolidColorBrush(Colors.Blue);
            var funColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#fd03fd"));
            switch (objType)
            {
                case ObjType.Table:
                    this.Image = new BitmapImage(new Uri(SysConst.Sys_TableIcon));                    
                    this.Content= ItemGrid();
                    break;
                case ObjType.View:
                    this.Image = new BitmapImage(new Uri(SysConst.Sys_ViewIcon));
                    this.Content= new TextBlock() { Text = this.Text, Foreground = black };
                    break;
                case ObjType.Proc:
                    this.Image = new BitmapImage(new Uri(SysConst.Sys_ProcIcon));
                    this.Content= new TextBlock() { Text = this.Text, Foreground = black };
                    break;
                case ObjType.Db:
                    this.Image = new BitmapImage(new Uri(SysConst.Sys_DatabaseIcon));
                    this.Content= new TextBlock() { Text = this.Text, Foreground = black };
                    break;
                case ObjType.Func:
                    this.Content= new TextBlock() { Text = this.Text, Foreground = funColor };
                    break;
                default:
                    this.Content= new TextBlock() { Text = this.Text, Foreground = blue };
                    this.Content= ItemGrid();
                    break;
            }
            #endregion
        }

        private Grid ItemGrid()
        {
            #region MyRegion
            var black = new SolidColorBrush(Colors.Black);
            var blue = new SolidColorBrush(Colors.Blue);
            var grid = new Grid();
            grid.Width = 500;
            grid.Children.Add(new TextBlock()
            {
                Text = this.Text,
                Foreground = blue,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Left
            });
            grid.Children.Add(new TextBlock()
            {
                Text = Desc,
                Margin= new System.Windows.Thickness(20, 0, 0, 0),
                Foreground = new SolidColorBrush(Colors.Gray),
                TextTrimming= System.Windows.TextTrimming.WordEllipsis,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Right
            });
            return grid; 
            #endregion
        }
    }
}
