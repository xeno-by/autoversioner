using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AutoVersioner.Tokens
{
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy("System.Collections.Generic.Mscorlib_DictionaryDebugView`2,mscorlib,Version=2.0.0.0,Culture=neutral,PublicKeyToken=b77a5c561934e089")]
    [DebuggerNonUserCode]
    internal abstract class BaseDictionary<K, V> : IDictionary<K, V>, IDictionary
    {
        public abstract int Count { get; }
        public abstract IEnumerator<KeyValuePair<K, V>> GetEnumerator();
        public abstract bool ContainsKey(K key);
        public abstract bool TryGetValue(K key, out V value);

        public ICollection<K> Keys { get { return ((IDictionary<K, V>)this).Keys; } }
        public ICollection<V> Values { get { return ((IDictionary<K, V>)this).Values; } }
        public V this[K key] { get { return ((IDictionary<K, V>)this)[key]; } set { ((IDictionary<K, V>)this)[key] = value; } }

        public abstract bool IsReadOnly { get; }
        public abstract void Add(K key, V value);
        public abstract bool Remove(K key);
        public abstract void Clear();
        protected abstract void SetValue(K key, V value);

        #region Implementation of boilerplate stuff (generic)

        private KeyCollection keys;
        private ValueCollection values;

        ICollection<K> IDictionary<K, V>.Keys
        {
            get
            {
                if (this.keys == null)
                    this.keys = new KeyCollection(this);

                return this.keys;
            }
        }

        ICollection<V> IDictionary<K, V>.Values
        {
            get
            {
                if (this.values == null)
                    this.values = new ValueCollection(this);

                return this.values;
            }
        }

        V IDictionary<K, V>.this[K key]
        {
            get
            {
                V value;
                if (!this.TryGetValue(key, out value))
                    throw new KeyNotFoundException();

                return value;
            }
            set
            {
                SetValue(key, value);
            }
        }

        void ICollection<KeyValuePair<K, V>>.Add(KeyValuePair<K, V> item)
        {
            this.Add(item.Key, item.Value);
        }

        bool ICollection<KeyValuePair<K, V>>.Contains(KeyValuePair<K, V> item)
        {
            V value;
            if (!this.TryGetValue(item.Key, out value))
                return false;

            return EqualityComparer<V>.Default.Equals(value, item.Value);
        }

        void ICollection<KeyValuePair<K, V>>.CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
        {
            Copy(this, array, arrayIndex);
        }

        bool ICollection<KeyValuePair<K, V>>.Remove(KeyValuePair<K, V> item)
        {
            if (!this.Contains(item))
                return false;

            return this.Remove(item.Key);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        [DebuggerNonUserCode]
        private abstract class Collection<T> : ICollection<T>
        {
            protected readonly IDictionary<K, V> dictionary;

            protected Collection(IDictionary<K, V> dictionary)
            {
                this.dictionary = dictionary;
            }

            public int Count
            {
                get { return this.dictionary.Count; }
            }

            public bool IsReadOnly
            {
                get { return true; }
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                Copy(this, array, arrayIndex);
            }

            public virtual bool Contains(T item)
            {
                foreach (T element in this)
                    if (EqualityComparer<T>.Default.Equals(element, item))
                        return true;
                return false;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return new Enumerator(this);
            }

            [DebuggerNonUserCode]
            private class Enumerator : IEnumerator<T>
            {
                private readonly Collection<T> _coll;
                private readonly IEnumerator _enum;

                public Enumerator(Collection<T> coll)
                {
                    _coll = coll;
                    _enum = coll.dictionary.GetEnumerator();
                }

                public bool MoveNext()
                {
                    return _enum.MoveNext();
                }

                public void Reset()
                {
                    _enum.Reset();
                }

                Object IEnumerator.Current { get { return Current; } }
                public T Current
                {
                    get
                    {
                        return _coll.GetItem((KeyValuePair<K, V>)_enum.Current);
                    }
                }

                public void Dispose()
                {
                    // do nothing
                }
            }

            protected abstract T GetItem(KeyValuePair<K, V> pair);

            public bool Remove(T item)
            {
                throw new NotSupportedException("Collection is read-only.");
            }

            public void Add(T item)
            {
                throw new NotSupportedException("Collection is read-only.");
            }

            public void Clear()
            {
                throw new NotSupportedException("Collection is read-only.");
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }

        [DebuggerDisplay("Count = {Count}")]
        [DebuggerTypeProxy("System.Collections.Generic.Mscorlib_DictionaryKeyCollectionDebugView`2,mscorlib,Version=2.0.0.0,Culture=neutral,PublicKeyToken=b77a5c561934e089")]
        [DebuggerNonUserCode]
        private class KeyCollection : Collection<K>
        {
            public KeyCollection(IDictionary<K, V> dictionary)
                : base(dictionary) { }

            protected override K GetItem(KeyValuePair<K, V> pair)
            {
                return pair.Key;
            }
            public override bool Contains(K item)
            {
                return this.dictionary.ContainsKey(item);
            }
        }

        [DebuggerDisplay("Count = {Count}")]
        [DebuggerTypeProxy("System.Collections.Generic.Mscorlib_DictionaryValueCollectionDebugView`2,mscorlib,Version=2.0.0.0,Culture=neutral,PublicKeyToken=b77a5c561934e089")]
        [DebuggerNonUserCode]
        private class ValueCollection : Collection<V>
        {
            public ValueCollection(IDictionary<K, V> dictionary)
                : base(dictionary) { }

            protected override V GetItem(KeyValuePair<K, V> pair)
            {
                return pair.Value;
            }
        }

        private static void Copy<T>(ICollection<T> source, T[] array, int arrayIndex)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            if (arrayIndex < 0 || arrayIndex > array.Length)
                throw new ArgumentOutOfRangeException("arrayIndex");

            if ((array.Length - arrayIndex) < source.Count)
                throw new ArgumentException("Destination array is not large enough. Check array.Length and arrayIndex.");

            foreach (T item in source)
                array[arrayIndex++] = item;
        }

        #endregion

        #region Implementation of boilerplate stuff (non-generic)

        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection<KeyValuePair<K, V>>)this).CopyTo((KeyValuePair<K, V>[])array, index);
        }

        private readonly Object _syncRoot = new Object();
        Object ICollection.SyncRoot
        {
            get { return _syncRoot; }
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        bool IDictionary.Contains(Object key)
        {
            return ContainsKey((K)key);
        }

        void IDictionary.Add(Object key, Object value)
        {
            Add((K)key, (V)value);
        }

        void IDictionary.Remove(Object key)
        {
            Remove((K)key);
        }

        Object IDictionary.this[Object key]
        {
            get { return this[(K)key]; }
            set { this[(K)key] = (V)value; }
        }

        ICollection IDictionary.Keys
        {
            get { return Keys.ToList(); }
        }

        ICollection IDictionary.Values
        {
            get { return Values.ToList(); }
        }

        bool IDictionary.IsFixedSize
        {
            get { return IsReadOnly; }
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return new DictionaryEnumerator(this);
        }

        [DebuggerNonUserCode]
        private class DictionaryEnumerator : IDictionaryEnumerator
        {
            private readonly IEnumerator<KeyValuePair<K, V>> _impl;

            public DictionaryEnumerator(BaseDictionary<K, V> @this)
            {
                _impl = @this.GetEnumerator();
            }

            public bool MoveNext()
            {
                return _impl.MoveNext();
            }

            public void Reset()
            {
                _impl.Reset();
            }

            public Object Current
            {
                get { return _impl.Current; }
            }

            public Object Key
            {
                get { return _impl.Current.Key; }
            }

            public Object Value
            {
                get { return _impl.Current.Value; }
            }

            public DictionaryEntry Entry
            {
                get { return new DictionaryEntry(Key, Value); }
            }
        }

        #endregion
    }
}