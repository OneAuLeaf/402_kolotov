using System;
using System.Threading.Tasks.Dataflow;
using System.Threading.Tasks;
using System.Threading;
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
            },
            TaskCreationOptions.LongRunning);

            while (await recognizer.ResultsQueue.OutputAvailableAsync()) {
                var image = recognizer.ResultsQueue.Receive();
                Console.WriteLine(image.ToString("0.00"));
            }
        }
    }
}
