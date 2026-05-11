using Avalonia.Data.Converters;
using Avalonia.Media;
using KWRP.Avalonia.Backend.Enums;
using KWRP.Avalonia.Backend.Model;
using KWRP.Frontend.Models.Localizer;
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
                    RollerWheelType.Single => Localizer.Instance["Domain.Front.SingleWheel"],
                    RollerWheelType.Tandem => Localizer.Instance["Domain.Front.TandemWheel"],
                    _ => "Undefined"
                };
            }

            return "Undefined";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}