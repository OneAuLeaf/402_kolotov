using System;
using System.Threading.Tasks;
using Recognition;


namespace ConsoleApp
{
    class Program
    {
        readonly static string ModelPath = @"../../../../YOLOV4/yolov4.onnx";

        readonly static int ThreadNum = Environment.ProcessorCount;
        // args: path_to_images [path_to_model, [threads_num]]
        static async Task Main(string[] args)
        {
            string pathImages, modelPath;
            int threadNum;
            switch (args.Length)
            {
                case 1:
                    pathImages = args[0];
                    modelPath = ModelPath;
                    threadNum = ThreadNum;
                    break;
                case 2:
                    pathImages = args[0];
                    modelPath = args[1];
                    threadNum = ThreadNum;
                    break;
                case 3:
                    pathImages = args[0];
                    modelPath = args[1];
                    if (!int.TryParse(args[2], out threadNum)) 
                        threadNum = ThreadNum;
                    break;
                default:
                    return;
            }

            Recognizer recognizer = new Recognizer(modelPath, threadNum);

            var res = recognizer.Recognize(pathImages);

            var t = Task.Factory.StartNew(() =>
            {
                if (Console.ReadLine().EndsWith("cc")) { // 'cc' + Enter = Cancel
                    recognizer.Cancel();
                }
            });
            
            RecognizedImage image;
            while (!res.IsCompleted || !recognizer.ResultsQueue.IsEmpty) {

                if (recognizer.ResultsQueue.TryDequeue(out image))
                    Console.WriteLine(image.ToString("0.00"));
            }

            await res;
        }
    }
}
