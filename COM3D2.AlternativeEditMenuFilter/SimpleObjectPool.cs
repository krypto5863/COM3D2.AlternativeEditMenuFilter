namespace COM3D2.AlternativeEditMenuFilter
{
    using System;
    using System.Collections.Generic;

    namespace COM3D2.AlternativeEditMenuFilter
    {
        public class SimpleObjectPool<T> where T : new()
        {
            private readonly Stack<T> _pool;
            private readonly List<T> _loaned;
            private readonly Action<T> _onGet;
            private readonly Action<T> _onRelease;

            public SimpleObjectPool(Action<T> onGet = null, Action<T> onRelease = null, int initialCapacity = 0)
            {
                _pool = new Stack<T>(initialCapacity > 0 ? initialCapacity : 4);
                _loaned = new List<T>();
                _onGet = onGet;
                _onRelease = onRelease;

                for (int i = 0; i < initialCapacity; i++)
                {
                    _pool.Push(new T());
                }
            }

            public T Get()
            {
                T item = _pool.Count > 0 ? _pool.Pop() : new T();
                _onGet?.Invoke(item);
                _loaned.Add(item);
                return item;
            }

            public void Release(T item)
            {
                _onRelease?.Invoke(item);
                _pool.Push(item);
                _loaned.Remove(item);
            }

            public void ReleaseAll()
            {
                for (var index = _loaned.Count - 1; index >= 0; index--)
                {
                    var item = _loaned[index];
                    Release(item);
                }
            }

            public int Count => _pool.Count;
        }
    }
}
