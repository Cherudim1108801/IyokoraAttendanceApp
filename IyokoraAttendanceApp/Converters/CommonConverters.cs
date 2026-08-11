using System.Globalization;

namespace IyokoraAttendanceApp.Converters;

public class InvertedBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is bool b && !b;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is bool b && !b;
}

public class StringToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        !string.IsNullOrEmpty(value as string);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public class IntToBoolConverter : IValueConverter
{
    /// <summary>ConverterParameter に "Invert" を指定すると、0以下のときに true を返す（結果を反転する）。</summary>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var hasItems = value is int i && i > 0;
        var invert = string.Equals(parameter as string, "Invert", StringComparison.OrdinalIgnoreCase);
        return invert ? !hasItems : hasItems;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
