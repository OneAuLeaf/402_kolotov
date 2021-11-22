using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Recognition;

namespace RecognitionUI
{
    public interface IView
    {
        string ChooseDir();
        string ChooseModel();
    }

    public interface IModel
    {
        public string ModelPath { get; }
        public string InputPath { get; }
        public StateVM State { get; }
        public ProxyDictionary RecognizedTypes { get; }

        void ChooseDir();
        void ChooseModel();
        void Start();
        void Cancel();
    }

    public class StateVM: INotifyPropertyChanged
    {
        private States _state;
        private int _images;
        private int _progress;
        public enum States { UNREADY, READY, PROCESS, CANCELLING, CANCELED, COMPLETED }
        public States State
        {
            get => _state;
            set
            {
                _state = value;
                OnPropertyChange(nameof(State));
            }
        }
        public int ImagesCount
        {
            get => _images;
            set
            {
                _images = value;
                OnPropertyChange(nameof(ImagesCount));
            }
        }
        public int ProgressCount
        {
            get => _progress;
            set
            {
                _progress = value;
                OnPropertyChange(nameof(ProgressCount));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChange(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public StateVM()
        {
            State = States.UNREADY;
            ImagesCount = 0;
            ProgressCount = 0;
        }
        
    }

    public class RecognitionViewModel: IModel, INotifyPropertyChanged, INotifyCollectionChanged
    {
        static readonly string[] s_imageExtensions = { ".png", ".jpg", ".bmp" };
        static readonly string s_modelExtension = ".onnx";

        Recognizer recognizer;
        IView view;
        
        public string ModelPath { get; private set; }
        public string InputPath { get; private set; }
        public StateVM State { get; private set; }
        public ProxyDictionary RecognizedTypes { get; private set; }
        

        public event PropertyChangedEventHandler PropertyChanged;
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected void OnPropertyChange(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void OnStateChangeHandler(object sender, PropertyChangedEventArgs args)
        {
            OnPropertyChange(nameof(State));
        }

        private bool checkUnreadyState()
        {
            return string.IsNullOrEmpty(ModelPath) || string.IsNullOrEmpty(InputPath) || 
                State.ImagesCount == 0 || Path.GetExtension(ModelPath) != s_modelExtension;
        }

        public RecognitionViewModel(IView _view)
        {
            view = _view;
            RecognizedTypes = new ProxyDictionary();
            State = new StateVM();
            RecognizedTypes.CollectionChanged += CollectionChanged;
            State.PropertyChanged += OnStateChangeHandler;
        }

        public void ChooseDir()
        {
            var res = view.ChooseDir();
            if (res != null)
            {
                InputPath = res;
                OnPropertyChange(nameof(InputPath));
                State.ImagesCount = Directory.GetFiles(InputPath).Where(x => s_imageExtensions.Contains(Path.GetExtension(x))).Count();
                if (checkUnreadyState())
                {
                    State.State = StateVM.States.UNREADY;
                }
                else
                {
                    State.State = StateVM.States.READY;
                }
                RecognizedTypes.Clear();
            }
        }

        public void ChooseModel()
        {
            var res = view.ChooseModel();
            if (res != null)
            {
                ModelPath = res;
                OnPropertyChange(nameof(ModelPath));
                recognizer = new Recognizer(ModelPath, 2);
                if (checkUnreadyState())
                {
                    State.State = StateVM.States.UNREADY;
                }
                else
                {
                    State.State = StateVM.States.READY;
                }
                RecognizedTypes.Clear();
            }
        }

        public void Start()
        {
            RecognizedTypes.Clear();
            State.ProgressCount = 0;
            State.State = StateVM.States.PROCESS;

            Task.Run(() => { return recognizer.Recognize(InputPath); });

            var t = Task.Factory.StartNew(async () =>
            {
                while (await recognizer.ResultsQueue.OutputAvailableAsync())
                {
                    var image = recognizer.ResultsQueue.Receive();
                    RecognizedTypes.Add(image);
                    State.ProgressCount += 1;
                    if (State.ProgressCount == State.ImagesCount)
                    {
                        State.State = StateVM.States.COMPLETED;
                        break;
                    }
                }
            },
            recognizer.CTS.Token,
            TaskCreationOptions.None,
            TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void Cancel()
        {
            State.State = StateVM.States.CANCELLING;
            recognizer.Cancel();
            State.State = StateVM.States.CANCELED;
        }
    }
}
