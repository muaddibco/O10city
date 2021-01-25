using System;
using System.Globalization;
using Xamarin.Forms;

namespace O10.Client.Mobile.Base.Converters
{
    public class DateTimeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime dateTime = (DateTime)value;

            return $"{dateTime.ToShortDateString()} {dateTime.ToShortTimeString()}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
