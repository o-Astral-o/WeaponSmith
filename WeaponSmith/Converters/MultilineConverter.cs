using System.Globalization;
using System.Windows.Data;

namespace WeaponSmith.Converters;

public class MultilineConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is string text && (text.Contains('\n') || text.Contains('\r'));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
