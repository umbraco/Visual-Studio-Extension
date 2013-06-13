using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace UmbracoStudioApp.Helpers
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
    }
}