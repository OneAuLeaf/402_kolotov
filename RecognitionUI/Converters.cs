using System;
using System.Globalization;
using System.Windows.Data;
using Recognition;
using System.Windows.Media.Imaging;
using System.Windows;
using System.IO;

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
                var source = new BitmapImage(new Uri(data.OriginPath));
                source.Freeze();
                var rect = new Int32Rect((int)data.Box.X, (int)data.Box.Y, (int)data.Box.W, (int)data.Box.H);
                var image = new CroppedBitmap(source, rect);
                image.Freeze();

                return image;
            }
            return DefaultConvert();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        private BitmapImage DefaultConvert()
        {
            var r = new Random(Environment.TickCount);
            switch (r.Next(4))
            {
                case 0:
                    return new BitmapImage(new Uri(Directory.GetCurrentDirectory() + @"\Resources\cat_once_frame_0003.jpg"));
                case 1:
                    return new BitmapImage(new Uri(Directory.GetCurrentDirectory() + @"\Resources\cat_once_frame_0006.jpg"));
                case 2:
                    return new BitmapImage(new Uri(Directory.GetCurrentDirectory() + @"\Resources\cat_once_frame_0013.jpg"));
                default:
                    return new BitmapImage(new Uri(Directory.GetCurrentDirectory() + @"\Resources\cat_once_frame_0014.jpg"));
            }
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
                return $"Find {data.Label} with confidence = {data.Confidence}";
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    [ValueConversion(typeof(RecognizedObject), typeof(string))]
    public class RecognizedObjectConverterFrom : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            RecognizedObject data = value as RecognizedObject;
            if (data != null)
            {
                return $"Cropped from Image = \"{data.OriginPath}\"";
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }

    [ValueConversion(typeof(StateVM), typeof(string))]
    public class StateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            StateVM data = value as StateVM;
            if (data != null)
            {
                switch (data.State)
                {
                    case StateVM.States.UNREADY:
                        return "Select model path and folder w/ images";
                    case StateVM.States.READY:
                        return $"{data.ImagesCount} images found";
                    case StateVM.States.PROCESS:
                        return $"processing...{data.ProgressCount}/{data.ImagesCount}";
                    case StateVM.States.CANCELLING:
                        return $"cancelling...";
                    case StateVM.States.COMPLETED:
                        return $"processed {data.ProgressCount}/{data.ImagesCount} images";
                    case StateVM.States.CANCELED:
                        return $"canceled {data.ProgressCount}/{data.ImagesCount} images";
                }
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
