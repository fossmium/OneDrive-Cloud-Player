using OneDrive_Cloud_Player.Models.GraphData;
using System;
using Windows.UI.Xaml.Data;

namespace OneDrive_Cloud_Player.Services.Converters
{
    public class MainPageExplorerConverter : IValueConverter
    {
        /// <summary>
        /// Converts objects on runtime.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            //Changes the ItemContentType icon of the items in the explorer.
            if (parameter.Equals("ContentTypeExplorerItem"))
            {
                if ((bool)value)
                {
                    return "/Assets/Icons/folder.png";
                }
                else
                {
                    return "/Assets/Icons/MultiMediaIcon.png";
                }
            }

            //Returns the item child when it's a folder. Otherwise return a line.
            if (parameter.Equals("ContentChildCountExplorer"))
            {
                CachedDriveItem item = (CachedDriveItem)value;
                if (item.IsFolder)
                {
                    return item.ChildCount;
                }
                return "-";
            }

            //Returns the correct size format.
            if (parameter.Equals("ContentItemSizeExplorer"))
            {
                Console.WriteLine(value);
                long size = (long)value;
                if (size > 1000 && size < 1000000000)
                {

                    return Math.Round((size / (double)Math.Pow(1024, 2))) + " MB";
                }
                else if (size > 1000000000)
                {
                    return Decimal.Round((Decimal)(size / (double)Math.Pow(1024, 3)), 2) + " GB";
                }
                else
                {
                    return size + " Bytes";
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
