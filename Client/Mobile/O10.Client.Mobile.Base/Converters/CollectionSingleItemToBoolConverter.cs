using System;
using System.Collections;
using System.Globalization;
using Xamarin.Forms;

namespace O10.Client.Mobile.Base.Converters
{
    public class CollectionSingleItemToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is ICollection collection))
            {
                return "!".Equals(parameter?.ToString()) ? true : false;
            }

            return "!".Equals(parameter?.ToString()) ? collection.Count <= 1 : collection.Count > 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
