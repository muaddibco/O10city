using System;
using System.Globalization;

namespace O10.Client.Mobile.Base.Converters
{
    public class MultiBoolConjunctiveConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool res = true;

            for (int i = 0; i < values.Length; i++)
            {
                res &= (bool)(values[i] ?? false);
            }

            return res;
        }
    }
}
