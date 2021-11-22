using System.Linq;
using System.Collections.Generic;
using System.Drawing;

namespace Recognition
{
    public class RecognizedImage
    {
        public string ImageName { get; }
        public string ImagePath { get; }
        public Bitmap Bitmap { get; }

        public List<RecognizedObject> Objects;

        public RecognizedImage(string imageName, string imagePath, Bitmap bitmap, IReadOnlyList<YoloV4Result> objects)
        {
            ImageName = imageName;
            ImagePath = imagePath;
            Bitmap = bitmap;
            Objects = objects?.Select(x => new RecognizedObject(x, bitmap, imagePath)).ToList();
        }

        public string ToString(string format)
        {
            return $"{ImageName} : \t\n" +
                string.Join("\t\n", from data in Objects select data.ToString(format));
        }

        public override string ToString()
        {
            return ToString(null);
        }

    }
    public class RecognizedObject
    {
        public string Label { get; }
        public string OriginPath { get; }
        public float Confidence { get; }
        public Box Box { get; }
        public Bitmap CroppedImage { get; }

        public RecognizedObject(YoloV4Result res, Bitmap image, string originPath)
        {
            Label = res.Label;
            OriginPath = originPath;
            Box = new Box(res.BBox[0], res.BBox[1], res.BBox[2], res.BBox[3]);
            Confidence = res.Confidence;

            CroppedImage = new Bitmap((int)Box.W, (int)Box.H);

            using (Graphics g = Graphics.FromImage(CroppedImage))
            {
                g.DrawImage(image, new Rectangle(0, 0, CroppedImage.Width, CroppedImage.Height),
                                 Box.Rectangle,
                                 GraphicsUnit.Pixel);
            }
        }

        public string ToString(string format)
        {
            return $" label={Label}, p={Confidence.ToString(format)}) " + Box.ToString(format);
        }

        public override string ToString()
        {
            return ToString(null);
        }
    }

    public struct Box
    {
        public float X { get; }
        public float Y { get; }
        public float W { get; }
        public float H { get; }

        public Rectangle Rectangle => new Rectangle((int)X, (int)Y, (int)W, (int)H);

        public Box(float x1, float y1, float x2, float y2)
        {
            X = x1;
            Y = y1;
            W = x2 - x1;
            H = y2 - y1;
        }

        public string ToString(string format)
        {
            return $"Box : ({X.ToString(format)}; {Y.ToString(format)}; {(X + W).ToString(format)}; {(Y + H).ToString(format)})";
        }

        public override string ToString()
        {
            return ToString(null);
        }
    }
}
