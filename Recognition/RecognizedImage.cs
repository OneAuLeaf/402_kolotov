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
            Objects = objects?.Select(x => new RecognizedObject(x)).ToList();
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
        public float Confidence { get; }

        public Box Box;

        public RecognizedObject(YoloV4Result res)
        {
            Label = res.Label;
            Box = new Box(res.BBox[0], res.BBox[1], res.BBox[2], res.BBox[3]);
            Confidence = res.Confidence;
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
        public float X { get; private set; }
        public float Y { get; private set; }
        public float W { get; private set; }
        public float H { get; private set; }

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
