using System;
using System.Globalization;
using System.Linq;
using Xamarin.Forms;

namespace O10.Client.Mobile.Base.Converters
{
    public class StringToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string expression = value?.ToString();

            if (parameter == null)
            {
                return false;
            }

            string param = (string)parameter;
            bool inverted = param.StartsWith("!");
            if (inverted)
            {
                param = param.Substring(1);
            }

            string[] opts = param.Split("|");

            return inverted ? opts.All(o => o != expression) : opts.Any(o => o == expression);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
