using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeekStream.Core
{
    [Serializable]
    public class HighscoreList<T> : IEnumerable<T>
    {

        private Comparison<T> _comparer;
        private T[] _items;
        
        public int Size { get; protected set; }
        public int Count { get; protected set; }



        public HighscoreList(Comparison<T> comparer,  int size = 10)
        {
            Size = size;
            _items = new T[size];
            Count = 0;
            _comparer = comparer;
        }

        public void Add(T item)
        {
            lock (this)
            {
                int idx = Array.IndexOf(_items, item);
                bool itemExists = idx >= 0;

                if(itemExists)
                {
                    PropogateItem(idx);
                }
                else if (Count < Size)
                {
                    _items[Count++] = item;
                    PropogateLastItem();
                }
                else if (IsGoodEnough(item))
                {
                    _items[Count - 1] = item;
                    PropogateLastItem();
                }
            }
        }

        private bool IsGoodEnough(T item)
        {
            return _comparer.Invoke(item, _items[Count - 1]) < 0;
        }

        private void PropogateLastItem()
        {
            PropogateItem(Count - 1);
        }

        private void PropogateItem(int itemIndex)
        {
            int idx = itemIndex;
            while(idx > 0 && _comparer.Invoke(_items[idx], _items[idx -1]) < 0)
            {
                //swap
                T temp = _items[idx-1];
                _items[idx - 1] = _items[idx];
                _items[idx] = temp;
                idx--;
            }
        }


        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return _items[i];
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Remove(T item)
        {
            int idx = Array.IndexOf(_items, item);
            bool exists = idx >= 0;
            if (exists)
            {
                Count--;

                //Move all items upward a step starting just 
                //below the item to be removed
                _items[idx] = default(T);
                while(idx < Count - 1)
                {
                    _items[idx] = _items[idx + 1];
                    idx++;
                }
                //Clear the last item
                _items[Size - 1] = default(T);
            }
            return exists;
        }

        public int RemoveItems(IEnumerable<T> items)
        {
            int itemsRemoved = 0;
            foreach (var item in items)
            {
                if (Remove(item)) itemsRemoved++;
            }
            return itemsRemoved;
        }
    }
}
