using System.Globalization;
using System.Windows.Data;

namespace MathForge;

// 把 0~100 的正确率映射成 0~300 像素宽度，用于简易条形图
public class RateToWidthConverter : IValueConverter
{
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
  {
    if (value is double rate)
    {
      return Math.Max(2, rate / 100.0 * 300);
    }
    return 2.0;
  }

  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
  {
    throw new NotSupportedException();
  }
}