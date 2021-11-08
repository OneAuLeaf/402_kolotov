using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace Recognition
{
    public class ProxyType: IEnumerable<RecognizedObject>, INotifyCollectionChanged
    {
        public string TypeLabel { get; private set; }
        public List<RecognizedObject> Items { get; private set; }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public ProxyType(string typeLabel)
        {
            TypeLabel = typeLabel;
            Items = new List<RecognizedObject>();
        }

        public void Add(RecognizedObject obj)
        {
            Items.Add(obj);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public IEnumerator<RecognizedObject> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        public override string ToString()
        {
            return $"{TypeLabel} ({Items.Count} objects recognized)";
        }
    }

    public class ProxyDictionary: IEnumerable<ProxyType>, INotifyCollectionChanged
    {
        public Dictionary<string, ProxyType> Dict { get; private set; }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public ProxyDictionary()
        {
            Dict = new Dictionary<string, ProxyType>();
        }

        public void Add(RecognizedImage image)
        {
            foreach (var obj in image.Objects) {
                if (!Dict.ContainsKey(obj.Label))
                    Dict.Add(obj.Label, new ProxyType(obj.Label));
                Dict[obj.Label].Add(obj);
            }
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public void Clear()
        {
            Dict.Clear();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public IEnumerator<ProxyType> GetEnumerator()
        {
            return Dict.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Dict.Values.GetEnumerator();
        }
    }
}
