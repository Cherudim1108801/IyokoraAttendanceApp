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
    /// <summary>ConverterParameter に "Invert" を指定すると、文字列が空のときに true を返す（結果を反転する）。</summary>
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var hasValue = !string.IsNullOrEmpty(value as string);
        var invert = string.Equals(parameter as string, "Invert", StringComparison.OrdinalIgnoreCase);
        return invert ? !hasValue : hasValue;
    }

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

/// <summary>
/// <see cref="TimeOnly"/>? と <see cref="TimePicker.Time"/>（<see cref="TimeSpan"/>、null不可）を相互変換する。
/// 未設定（null）の間は 00:00 を表示上のみの既定値として扱う。ユーザーが操作するまでは
/// TimePicker から null に対する ConvertBack は呼ばれないため、未操作の項目は null のまま保たれる。
/// </summary>
public class TimeOnlyToTimeSpanConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is TimeOnly t ? t.ToTimeSpan() : TimeSpan.Zero;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is TimeSpan ts ? TimeOnly.FromTimeSpan(ts) : null;
}
