using System.Drawing;
using System.IO;
using Recognition;
using Database;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;

namespace RecognitionUI
{
    public static class DatabaseIModelConverter
    {
        public static Item RecognizedObjectToItem(RecognizedObject obj)
        {
            return new Item
            {
                Label = obj.Label,
                Path = obj.OriginPath,
                Confidence = obj.Confidence,
                X = obj.Box.X,
                Y = obj.Box.Y,
                W = obj.Box.W,
                H = obj.Box.H,
                Details = new ItemDetails
                {
                    Image = FromBitmapToBytes(obj.CroppedImage)
                }
            };
        }

        public static RecognizedObject ItemToRecognizedObject(Item item)
        {
            return new RecognizedObject(
                item.Label,
                item.Path,
                item.Confidence,
                item.X,
                item.Y,
                item.W,
                item.H,
                FromBytesToBitmap(item.Details.Image)
            );
        }

        public static Bitmap FromBytesToBitmap(byte[] im)
        {
            using (var stream = new MemoryStream(im))
            {
                return new Bitmap(stream);
            }
        }    
        
        public static byte[] FromBitmapToBytes(Bitmap im)
        {
            using (var stream = new MemoryStream())
            {
                im.Save(stream, ImageFormat.Bmp);
                return stream.ToArray();
            }
            
        }
    }
}
