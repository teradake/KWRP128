using Avalonia.Data.Converters;
using Avalonia.Media;
using KWRP.Avalonia.Backend.Enums;
using KWRP.Avalonia.Backend.Model;
using System;
using System.Globalization;

namespace KWRP.Avalonia.Frontend.Converters
{
    public class WheelTypeConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is RollerWheelType type)
            {
                return type switch
                {
                    RollerWheelType.Single => "片鉄輪",
                    RollerWheelType.Tandem => "両鉄輪",
                    _ => Brushes.White
                };
            }

            return Brushes.White;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}