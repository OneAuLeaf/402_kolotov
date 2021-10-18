using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Collections.Concurrent;
using Recognition; 
using static Microsoft.ML.Transforms.Image.ImageResizingEstimator;

namespace Recognition
{
    public class Recognizer
    {
        // model is available here:
        // https://github.com/onnx/models/tree/master/vision/object_detection_segmentation/yolov4

        static readonly int s_threadMax = Environment.ProcessorCount;

        static readonly string[] s_imageExtensions = { ".png", ".jpg", ".bmp" };

        static readonly string[] s_classesNames = new string[] { "person", "bicycle", "car", "motorbike", "aeroplane", "bus", "train", "truck", "boat",
            "traffic light", "fire hydrant", "stop sign", "parking meter", "bench", "bird", "cat", "dog", "horse", "sheep", "cow", "elephant", "bear",
            "zebra", "giraffe", "backpack", "umbrella", "handbag", "tie", "suitcase", "frisbee", "skis", "snowboard", "sports ball", "kite", "baseball bat",
            "baseball glove", "skateboard", "surfboard", "tennis racket", "bottle", "wine glass", "cup", "fork", "knife", "spoon", "bowl", "banana", "apple",
            "sandwich", "orange", "broccoli", "carrot", "hot dog", "pizza", "donut", "cake", "chair", "sofa", "pottedplant", "bed", "diningtable", "toilet",
            "tvmonitor", "laptop", "mouse", "remote", "keyboard", "cell phone", "microwave", "oven", "toaster", "sink", "refrigerator", "book", "clock", "vase",
            "scissors", "teddy bear", "hair drier", "toothbrush"
        };

        SemaphoreSlim cancelRequest = new SemaphoreSlim(0, 1);

        public ConcurrentQueue<RecognizedImage> ResultsQueue = new ConcurrentQueue<RecognizedImage>();
        public string ModelPath { get; private set; }
        public int ThreadNum { get; private set; }

        public Recognizer(string modelPath, int? threadsNum=null)
        {
            ModelPath = modelPath;
            ThreadNum = threadsNum ?? s_threadMax;
        }

        public async Task Recognize(string inputPath)
        {
            var cts = new CancellationTokenSource();

            MLContext mlContext = new MLContext();

            // model is available here:
            // https://github.com/onnx/models/tree/master/vision/object_detection_segmentation/yolov4

            // Define scoring pipeline
            var pipeline = mlContext.Transforms.ResizeImages(inputColumnName: "bitmap", outputColumnName: "input_1:0", imageWidth: 416, imageHeight: 416, resizing: ResizingKind.IsoPad)
                .Append(mlContext.Transforms.ExtractPixels(outputColumnName: "input_1:0", scaleImage: 1f / 255f, interleavePixelColors: true))
                .Append(mlContext.Transforms.ApplyOnnxModel(
                    shapeDictionary: new Dictionary<string, int[]>()
                    {
                        { "input_1:0", new[] { 1, 416, 416, 3 } },
                        { "Identity:0", new[] { 1, 52, 52, 3, 85 } },
                        { "Identity_1:0", new[] { 1, 26, 26, 3, 85 } },
                        { "Identity_2:0", new[] { 1, 13, 13, 3, 85 } },
                    },
                    inputColumnNames: new[]
                    {
                        "input_1:0"
                    },
                    outputColumnNames: new[]
                    {
                        "Identity:0",
                        "Identity_1:0",
                        "Identity_2:0"
                    },
                    modelFile: ModelPath, recursionLimit: 100));
            // Fit on empty list to obtain input data schema
            var fitted = pipeline.Fit(mlContext.Data.LoadFromEnumerable(new List<YoloV4BitmapData>()));

            var allFiles = Directory.GetFiles(inputPath).ToArray();

            var cancel_check = Task.Factory.StartNew(() =>
            {
                cancelRequest.Wait();
                cts.Cancel();
            },
            TaskCreationOptions.LongRunning);

            var preProcessing = new TransformBlock<string, RecognizedImage>(file =>
            {
                if (cts.Token.IsCancellationRequested) {
                    return null;
                }
                var name = Path.GetFileName(file);
                var fullPath = Path.Combine(inputPath, name);
                if (s_imageExtensions.Contains(Path.GetExtension(name))) {
                    return new RecognizedImage(name, fullPath, new Bitmap(Image.FromFile(fullPath)), null);
                } else {
                    return null;
                }
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cts.Token,
                MaxDegreeOfParallelism = ThreadNum
            });

            var predicting = new ActionBlock<RecognizedImage>(x =>
            {
                if (cts.Token.IsCancellationRequested) {
                    return;
                }
                var engine = mlContext.Model.CreatePredictionEngine<YoloV4BitmapData, YoloV4Prediction>(fitted);
                var results = engine.Predict(new YoloV4BitmapData() { Image = x.Bitmap });
                x.Objects = results.GetResults(s_classesNames, 0.5f, 0.7f).Select(x => new RecognizedObject(x)).ToList();
                ResultsQueue.Enqueue(x);
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cts.Token,
                MaxDegreeOfParallelism = 1
            });

            var linkOption = new DataflowLinkOptions { PropagateCompletion = true };
            preProcessing.LinkTo(predicting, linkOption, x => x != null);

            Parallel.ForEach(allFiles, file => preProcessing.Post(file));

            preProcessing.Complete();
            try
            {
                await predicting.Completion; 
            }
            catch (TaskCanceledException)
            {
            }
        }
            
        public void Cancel()
        {
            cancelRequest.Release();
        }
    }
}
