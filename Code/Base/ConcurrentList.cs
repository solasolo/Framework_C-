using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime;
using System.Text;

namespace SoulFab.Core.Base
{
    public class ConcurrentList<T> : IList<T>, ICollection<T>, IEnumerable<T>
    {
        object LockObject;

        List<T> _list;

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public ConcurrentList()
        {
            this.LockObject = new Object();

            this._list = new List<T>();
        }

        public ConcurrentList(IEnumerable<T> collection)
        {
            _list = new List<T>(collection);
        }

        [TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        public ConcurrentList(int capacity)
        {
            _list = new List<T>(capacity);
        }


        public void CopyTo(T[] array, int index)
        {
            lock (this.LockObject) _list.CopyTo(array, index);
        }

        public int Count
        {
            get 
            {
                int count;

                lock(this) count = _list.Count;

                return count;
            }
        }

        public void Clear()
        {
            lock (this.LockObject) _list.Clear();
        }

        public bool Contains(T value)
        {
            lock (this.LockObject) return _list.Contains(value);
        }

        public int IndexOf(T value)
        {
            lock (this.LockObject) return _list.IndexOf(value);
        }

        public void Insert(int index, T value)
        {
            lock (this.LockObject) _list.Insert(index, value);
        }

        public void Add(T item)
        {
            lock (this.LockObject) _list.Add(item);
        }

        public bool Remove(T item)
        {
            lock (this.LockObject) return _list.Remove(item);
        }

        public void RemoveAt(int index)
        {
            lock (this.LockObject) _list.RemoveAt(index);
        }

        T IList<T>.this[int index]
        {
            get
            {
                lock (this.LockObject) return _list[index];
            }
            set
            {
                lock (this.LockObject) _list[index] = value;
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            lock (this.LockObject)
            {
                foreach (var item in this._list)
                {
                    yield return item;
                }
            }
        }

        public IEnumerator GetEnumerator()
        {
            lock (this.LockObject)
            {
                foreach (var item in this._list)
                {
                    yield return item;
                }
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }
    }

}
