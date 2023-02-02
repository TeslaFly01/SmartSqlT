using System;
using System.Collections.Generic;
using System.Windows;
using SmartSQL.Models;
using SmartSQL.Helper;
using SmartSQL.Views;

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
