using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace TracesTesCamions.Converters
{
    public class RelativePathToAbsoluteConverter : IValueConverter
    {
        // Assumes the base directory is the app directory or data folder, à adapter selon ton cas
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string relativePath = value as string;
            if (string.IsNullOrEmpty(relativePath))
                return null;

            // Ici on récupère le dossier courant de l'exécutable
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;

            string fullPath = Path.Combine(baseDir, relativePath);

            if (!File.Exists(fullPath))
                return null;

            return new BitmapImage(new Uri(fullPath));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
