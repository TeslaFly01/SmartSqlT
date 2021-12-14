using System;
using System.Collections.Generic;
using System.Linq;
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
using SmartCode.Framework.Exporter;
using UserControlE = System.Windows.Controls.UserControl;

namespace SmartCode.Tool.UserControl
{
    /// <summary>
    /// UDiagram.xaml 的交互逻辑
    /// </summary>
    public partial class UDiagram : UserControlE
    {
        public UDiagram()
        {
            InitializeComponent();
            IExporter exporter = new SqlServer2008Exporter();
           var TableColumns = exporter.GetColumnsExt(165679738, "server=10.136.0.114;uid=ipsa;Pwd=ipsa@20200705;database=PSAData;");
            var list = TableColumns.Values.ToList();
            TableGrid.ItemsSource = list;
        }
    }

    public class TTTObj
    {
        public string Name { get; set; }

        public string Type { get; set; }
    }
}
