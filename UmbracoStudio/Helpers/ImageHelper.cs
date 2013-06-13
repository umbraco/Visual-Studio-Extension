using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Umbraco.UmbracoStudio.Helpers
{
    public class ImageHelper
    {
        public static Image GetImageFromResource(string relativeUriFileName)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(relativeUriFileName, UriKind.Relative);
            bitmap.EndInit();
            return new Image { Source = bitmap };
        }

        public static Image GetIconFromSolution(string baseDirectory, string iconFileName)
        {
            var absoluteUriFileName = Path.Combine(baseDirectory, iconFileName);

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(absoluteUriFileName, UriKind.Absolute);
            bitmap.EndInit();
            return new Image { Source = bitmap };
        }
    }
}