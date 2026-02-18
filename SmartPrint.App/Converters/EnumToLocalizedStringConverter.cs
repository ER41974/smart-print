using System;
using System.Globalization;
using System.Windows.Data;
using SmartPrint.Core.Resources;

namespace SmartPrint.App.Converters;

public class EnumToLocalizedStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null) return string.Empty;

        var type = value.GetType();
        type = Nullable.GetUnderlyingType(type) ?? type;

        if (type.IsEnum)
        {
            var key = $"{type.Name}_{value}";
            var localized = Strings.Get(key);

            // If resource is missing, Strings.Get returns the key.
            // We prefer the enum value string in that case.
            if (localized == key)
            {
                return value.ToString();
            }
            return localized;
        }
        return value.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
