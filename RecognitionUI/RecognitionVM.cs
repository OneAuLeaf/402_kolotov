using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Windows.Threading;
using Recognition;

namespace RecognitionUI
{
    public interface IView
    {
        string ChooseDir();
    }

    public interface IModel
    {
        public List<string> FolderImages { get; }
        public ProxyDictionary RecognizedTypes { get; }

        void ChooseDir();
        void Start();
        void Cancel();
    }

    public class RecognitionViewModel: IModel, INotifyPropertyChanged, INotifyCollectionChanged
    {
        readonly static string ModelPath = @"E:\Projects\C#Labs\YOLOV4\yolov4.onnx";
        Recognizer recognizer = new Recognizer(ModelPath);
        Dispatcher dispatcher;
        IView view;
        string inputPath;

        public List<string> FolderImages { get; private set; }
        public ProxyDictionary RecognizedTypes { get; private set; }
        

        public event PropertyChangedEventHandler PropertyChanged;
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected void OnPropertyChange(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public RecognitionViewModel(IView _view, Dispatcher _dispatcher)
        {
            view = _view;
            dispatcher = _dispatcher;
            RecognizedTypes = new ProxyDictionary();
            FolderImages = new List<string>();
            RecognizedTypes.CollectionChanged += CollectionChanged;
        }

        public void ChooseDir()
        {
            inputPath = view.ChooseDir();
            FolderImages = Directory.GetFiles(inputPath).ToList();
            OnPropertyChange(nameof(FolderImages));
        }

        public void Start()
        {
            RecognizedTypes.Clear();

            Task<Task>.Factory.StartNew(() => { return recognizer.Recognize(inputPath); }).Wait();

            var t = Task.Factory.StartNew(async () =>
            {
                while (await recognizer.ResultsQueue.OutputAvailableAsync())
                {
                    var image = recognizer.ResultsQueue.Receive();
                    RecognizedTypes.Add(image);
                }
            },
            CancellationToken.None,
            TaskCreationOptions.None,
            TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void Cancel()
        {
            recognizer.Cancel();
        }
    }
}
