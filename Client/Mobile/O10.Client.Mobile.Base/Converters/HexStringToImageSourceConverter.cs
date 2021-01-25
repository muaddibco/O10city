using System;
using System.Globalization;
using System.IO;
using O10.Core.ExtensionMethods;
using Xamarin.Forms;

namespace O10.Client.Mobile.Base.Converters
{
    public class HexStringToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            string imageString = value.ToString();
            byte[] imageBytes = imageString.HexStringToByteArray();

            ImageSource imageSource = ImageSource.FromStream(() => new MemoryStream(imageBytes));

            return imageSource;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
