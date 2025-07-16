using Avalonia.Data.Converters;
using Avalonia.Media;
using KWRP.Avalonia.Backend.Model;
using System;
using System.Globalization;

namespace KWRP.Avalonia.Frontend.Converters
{
    public class LogTypeToBrushConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is LogType type)
            {
                return type switch
                {
                    LogType.Debug => Brushes.LightGray,
                    LogType.Info => Brushes.LightBlue,
                    LogType.Warn => Brushes.Khaki,
                    LogType.Error => Brushes.IndianRed,
                    LogType.Fatal => Brushes.DarkRed,
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