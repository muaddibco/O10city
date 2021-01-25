using System;
using System.Globalization;
using O10.Client.Mobile.Base.Enums;
using Xamarin.Forms;

namespace O10.Client.Mobile.Base.Converters
{
    public class AttributeStateToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            AttributeState attributeState = (AttributeState)value;

            switch (attributeState)
            {
                case AttributeState.NotConfirmed:
                    return Color.FromHex("#767676");
                case AttributeState.Confirmed:
                    return Color.FromHex("#008001");
                case AttributeState.Disabled:
                    return Color.FromHex("#FE0000");
                default:
                    return Color.Default;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
