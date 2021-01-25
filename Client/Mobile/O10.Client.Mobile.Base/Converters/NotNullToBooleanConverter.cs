using System;
using System.Globalization;
using Xamarin.Forms;

namespace O10.Client.Mobile.Base.Converters
{
    public class NotNullToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return "!".Equals(parameter) ? value == null : value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
