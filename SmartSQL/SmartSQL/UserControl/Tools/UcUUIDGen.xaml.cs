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
    public partial class UcUUIDGen : BaseUserControl
    {
        /// <summary>
        /// 结果数据列表
        /// </summary>
        public static readonly DependencyProperty UUIDListProperty = DependencyProperty.Register(
            "UUIDList", typeof(List<UUIDResultDTO>), typeof(UcUUIDGen), new PropertyMetadata(default(List<UUIDResultDTO>)));
        public List<UUIDResultDTO> UUIDList
        {
            get => (List<UUIDResultDTO>)GetValue(UUIDListProperty);
            set
            {
                SetValue(UUIDListProperty, value);
                OnPropertyChanged(nameof(UUIDList));
            }
        }

        public UcUUIDGen()
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
            var parentWindow = (ToolBox)System.Windows.Window.GetWindow(this);
            parentWindow.UcBox.Content = new UcMainTools();
        }

        private void BtnGen_Click(object sender, RoutedEventArgs e)
        {
            var uuidNums = Convert.ToInt32(TextUUIDNums.Value);
            var uuidIsCaseUpper = CheckUpper.IsChecked;
            var uuidIsRemoveHLine = CheckRemoveHLine.IsChecked;

            var uuids = new List<UUIDResultDTO>();
            var uuidFotmat = uuidIsRemoveHLine.Value ? "N" : "D";
            for (int i = 0; i < uuidNums; i++)
            {
                var uuid = Guid.NewGuid().ToString(uuidFotmat);
                uuids.Add(new UUIDResultDTO
                {
                    UUID = uuidIsCaseUpper.Value ? uuid.ToUpper() : uuid.ToLower()
                });
            }
            UUIDList = uuids;
        }

        /// <summary>
        /// 复制行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnCopyRow_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedItem = (UUIDResultDTO)TableGrid.CurrentItem;
            if (selectedItem != null)
            {
                Clipboard.SetDataObject(selectedItem.UUID);
                Oops.Success("文本已复制到剪切板");
            }
        }
    }
}
