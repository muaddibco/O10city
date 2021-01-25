using System;
using System.Collections;
using System.Globalization;
using Xamarin.Forms;

namespace O10.Client.Mobile.Base.Converters
{
    public class CollectionCountToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ICollection collection = value as ICollection;
            if (collection == null)
            {
                return "!".Equals(parameter?.ToString()) ? true : false;
            }

            return "!".Equals(parameter?.ToString()) ? collection.Count == 0 : collection.Count > 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
