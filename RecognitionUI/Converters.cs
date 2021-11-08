using System;
using System.Globalization;
using System.Windows.Data;
using System.Drawing;
using Recognition;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace RecognitionUI
{

    [ValueConversion(typeof(RecognizedObject), typeof(BitmapSource))]
    public class RecognizedObjectConverterImage : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            RecognizedObject data = value as RecognizedObject;
            if (data != null)
            {
                return FastConvert(data.CroppedImage);
            }
            return DefaultConvert();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        private static BitmapSource FastConvert(Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(
                bitmapData.Width, bitmapData.Height,
                bitmap.HorizontalResolution, bitmap.VerticalResolution,
                PixelFormats.Bgr24, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);

            return bitmapSource;
        }

        private BitmapImage DefaultConvert()
        {
            var r = new Random(Environment.TickCount);
            switch (r.Next(4))
            {
                case 0:
                    return new BitmapImage(new Uri(@"E:\Projects\C#Labs\cat_once_frame_0003.jpg"));
                case 1:
                    return new BitmapImage(new Uri(@"E:\Projects\C#Labs\cat_once_frame_0006.jpg"));
                case 2:
                    return new BitmapImage(new Uri(@"E:\Projects\C#Labs\cat_once_frame_0013.jpg"));
                case 3:
                    return new BitmapImage(new Uri(@"E:\Projects\C#Labs\cat_once_frame_0014.jpg"));
            }
            return new BitmapImage(new Uri(@"E:\Projects\C#Labs\cat_once.gif"));
        }
    }

    [ValueConversion(typeof(RecognizedObject), typeof(string))]
    public class RecognizedObjectConverterInfo : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            RecognizedObject data = value as RecognizedObject;
            if (data != null)
            {
                return $"{data.Label} {data.Confidence}";
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    [ValueConversion(typeof(RecognizedObject), typeof(string))]
    public class RecognizedObjectConverterBox : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            RecognizedObject data = value as RecognizedObject;
            if (data != null)
            {
                return $"{data.Box}";
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
