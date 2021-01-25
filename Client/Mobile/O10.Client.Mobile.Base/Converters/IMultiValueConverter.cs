using System;
using System.Globalization;

namespace O10.Client.Mobile.Base.Converters
{
    public interface IMultiValueConverter
    {
        object Convert(object[] values, Type targetType, object parameter, CultureInfo culture);
    }
}
