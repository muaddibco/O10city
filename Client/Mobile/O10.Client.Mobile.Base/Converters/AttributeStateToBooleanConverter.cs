using System;
using System.Globalization;
using O10.Client.Mobile.Base.Enums;
using Xamarin.Forms;

namespace O10.Client.Mobile.Base.Converters
{
    public class AttributeStateToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            AttributeState currentValue = (AttributeState)value;
            if (Enum.TryParse(parameter?.ToString(), true, out AttributeState targetValue))
            {
                return currentValue == targetValue;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
