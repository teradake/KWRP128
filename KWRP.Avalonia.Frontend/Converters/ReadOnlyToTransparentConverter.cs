using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWRP.Avalonia.Frontend.Converters
{
    public class ReadOnlyToTransparentConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isReadOnly)
            {
                return isReadOnly
                    ? new SolidColorBrush(Color.FromArgb(5, 255, 255, 255))
                    : Brushes.White;
            }
            return Brushes.White;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
