using System;
using System.Windows;
using System.Windows.Media;
using SmartSQL.Helper;
using SmartSQL.Views;
using HandyControl.Data;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using SharpVectors.Renderers.Utils;

namespace SmartSQL.UserControl
{
    /// <summary>
    /// MainContent.xaml 的交互逻辑
    /// </summary>
    public partial class UcJson2CSharp : BaseUserControl
    {
        public UcJson2CSharp()
        {
            InitializeComponent();
            DataContext = this;
            HighlightingProvider.Register(SkinType.Dark, new HighlightingProviderDark());
            TextJsonEditor.TextArea.TextView.ElementGenerators.Add(new TruncateLongLines());
            TextJsonEditor.SyntaxHighlighting = HighlightingProvider.GetDefinition(SkinType.Dark, "Json");
            TextJsonEditor.TextArea.SelectionCornerRadius = 0;
            TextJsonEditor.TextArea.SelectionBorder = null;
            TextJsonEditor.TextArea.SelectionForeground = null;
            TextJsonEditor.TextArea.SelectionBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFADD6FF"));
            TextCSharpEditor.TextArea.TextView.ElementGenerators.Add(new TruncateLongLines());
            TextCSharpEditor.SyntaxHighlighting = HighlightingProvider.GetDefinition(SkinType.Dark, "C#");
            TextCSharpEditor.TextArea.SelectionCornerRadius = 0;
            TextCSharpEditor.TextArea.SelectionBorder = null;
            TextCSharpEditor.TextArea.SelectionForeground = null;
            TextCSharpEditor.TextArea.SelectionBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFADD6FF"));
        }

        /// <summary>
        /// 格式化Json
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnFormatter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(TextJsonEditor.Text))
                {
                    Oops.Oh("请输入Json文本");
                    return;
                }
                TextJsonEditor.Text = StrUtil.JsonFormatter(TextJsonEditor.Text);

                
                JToken jToken = JToken.Parse(TextJsonEditor.Text);
                StringBuilder sb = new StringBuilder();
                GenerateCSharpEntity(jToken, "Root", sb);
                TextCSharpEditor.Text = sb.ToString();
            }
            catch (Exception)
            {
                Oops.Oh("Json解析失败，请检查");
            }
        }

        private void GenerateCSharpEntity(JToken jToken, string entityName, StringBuilder sb)
        {
            switch (jToken.Type)
            {
                case JTokenType.Object:
                    sb.AppendLine("public class " + entityName);
                    sb.AppendLine("{");
                    foreach (JProperty property in jToken.Children<JProperty>())
                    {
                        string propertyName = property.Name;
                        string typeName = property.Value.Type.ToString();
                        GenerateCSharpEntity(property.Value, propertyName, sb);
                        sb.AppendLine($"    public {typeName} {propertyName} {{ get; set; }}");
                    }
                    sb.AppendLine("}");
                    break;

                case JTokenType.Array:
                    JToken firstChild = jToken.First;
                    if (firstChild != null && firstChild.Type == JTokenType.Object)
                    {
                        GenerateCSharpEntity(firstChild, entityName, sb);
                    }
                    else
                    {
                        sb.AppendLine($"public List<{firstChild.Type}> {entityName} {{ get; set; }}");
                    }
                    break;

                default:
                    sb.AppendLine($"public {jToken.Type} {entityName} {{ get; set; }}");
                    break;
            }
        }

        /// <summary>
        /// 返回
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnReturn_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = (ToolBox)System.Windows.Window.GetWindow(this);
            parentWindow.UcBox.Content = new UcMainTools();
        }

        /// <summary>
        /// 复制文本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCopy_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(TextCSharpEditor.Text))
            {
                Clipboard.SetDataObject(TextCSharpEditor.Text);
                Oops.Success("文本已复制到剪切板");
            }
        }

        /// <summary>
        /// 清空输入框
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            TextJsonEditor.Text = string.Empty;
        }

        /// <summary>
        /// 编辑器获取焦点自动粘贴剪切板文本
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextJsonEditor_GotFocus(object sender, RoutedEventArgs e)
        {
            var clipboardText = Clipboard.GetText();
            if (string.IsNullOrEmpty(TextJsonEditor.Text) && !string.IsNullOrEmpty(clipboardText))
            {
                var isTryParse = false;
                try
                {
                    JsonDocument.Parse(clipboardText);
                    isTryParse = true;
                }
                catch { }
                if (isTryParse)
                {
                    TextJsonEditor.Text = clipboardText;
                }
            }
        }

        private static string GenerateCSharpClass(dynamic obj, string className)
        {
            string csharpCode = $"public class {className}" + Environment.NewLine;
            csharpCode += "{" + Environment.NewLine;
            foreach (var property in obj)
            {
                string propertyName = property.Name;
                string propertyValueTypeName = property.Value.GetType().Name;
                string propertyTypeName = GetCSharpType(propertyValueTypeName);
                if (property.Value is Newtonsoft.Json.Linq.JArray)
                {
                    // 如果属性是 JSON 数组，则递归调用 GenerateCSharpClass() 方法进行生成
                    csharpCode += $"    public List<{className}> {propertyName} {{ get; set; }}" + Environment.NewLine;
                    string childClassName = $"{className}_{propertyName}";
                    string childCsharpCode = GenerateCSharpClass(property.Value[0], childClassName);
                    csharpCode += childCsharpCode;
                }
                else
                {
                    // 如果属性不是 JSON 数组，则直接生成属性
                    csharpCode += $"    public {propertyTypeName} {propertyName} {{ get; set; }}" + Environment.NewLine;
                }
            }
            csharpCode += "}" + Environment.NewLine;
            return csharpCode;
        }

        private static string GetCSharpType(string typeName)
        {
            switch (typeName)
            {
                case "String":
                    return "string";
                case "Int32":
                    return "int";
                case "Boolean":
                    return "bool";
                case "Float":
                    return "float";
                case "Double":
                    return "double";
                default:
                    return typeName;
            }
        }
    }
}
