using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SmartSQL.Converts
{
    public class DisplayNameConvert : IValueConverter
    {
        //当值从绑定源传播给绑定目标时，调用方法Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return DependencyProperty.UnsetValue;
            var d = (Visibility)value;
            var w = Visibility.Collapsed;
            switch (d)
            {
                case Visibility.Collapsed:
                case Visibility.Hidden:
                    w = Visibility.Visible;
                    break;
                case Visibility.Visible:
                    w = Visibility.Collapsed;
                    break;
            }
            return w;
        }

        //当值从绑定目标传播给绑定源时，调用此方法ConvertBack
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
