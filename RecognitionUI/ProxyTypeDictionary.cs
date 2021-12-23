using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Recognition;
using Database;
using System.Threading.Tasks;

namespace RecognitionUI
{
    public class ProxyType : IEnumerable<RecognizedObject>
    {
        public string TypeLabel { get; private set; }
        public int Count { get; private set; }
        public IEnumerable<Item> Items { get; private set; }

        public ProxyType(Response resp, IEnumerable<Item> items)
        {
            TypeLabel = resp.Label;
            Count = resp.Count;
            Items = items;
        }

        public IEnumerator<RecognizedObject> GetEnumerator()
        {
            return Items.Select(
                x => DatabaseIModelConverter.ItemToRecognizedObject(x)
            ).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Items.Select(
                x => DatabaseIModelConverter.ItemToRecognizedObject(x)
            ).GetEnumerator();
        }

        public override string ToString()
        {
            return $"{TypeLabel} ({Count} objects recognized)";
        }
    }

    public class ProxyDictionary : IEnumerable<ProxyType>, INotifyCollectionChanged
    {
        DatabaseManager manager;

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public ProxyDictionary()
        {
            manager = new DatabaseManager();
        }

        public async Task Add(RecognizedImage image)
        {
            foreach (var obj in image.Objects)
            {
                var item = DatabaseIModelConverter.RecognizedObjectToItem(obj);
                await manager.AddAsync(item);
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
            
        }
            

        public void Clear()
        {
            manager.Clear();
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public IEnumerator<ProxyType> GetEnumerator()
        {
            return manager.GetClasses().Select(x => new ProxyType(x, manager.GetClassObjects(x.Label))).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return manager.GetClasses().Select(x => new ProxyType(x, manager.GetClassObjects(x.Label))).GetEnumerator();
        }
    }
}
