using System;
using Windows.UI.Xaml.Data;

namespace OneDrive_Cloud_Player.Services.Converters
{
    class MediaTimeConverter : IValueConverter
    {
        object IValueConverter.Convert(object timeMs, Type targetType, object parameter, string language)
        {
            TimeSpan timeSpan = TimeSpan.FromMilliseconds(Convert.ToDouble(timeMs));
            return timeSpan.ToString(@"hh\:mm\:ss");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
