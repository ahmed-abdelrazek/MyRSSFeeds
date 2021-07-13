using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using Windows.Storage.Streams;

namespace MyRSSFeeds.Helpers
{
    public class ByteArrayToImageConverter : IValueConverter
    {
        public ByteArrayToImageConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is byte[] ba && ba is not null)
            {
                using InMemoryRandomAccessStream stream = new();
                using (DataWriter writer = new(stream.GetOutputStreamAt(0)))
                {
                    writer.WriteBytes(ba);
                    writer.StoreAsync().GetResults();
                }
                BitmapImage image = new();
                image.SetSource(stream);
                return image;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
