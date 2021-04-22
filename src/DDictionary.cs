using System;
using System.Collections;
using System.Collections.Generic;

public class DDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary
{
    private readonly object _dictionaryLock;
    private readonly IDictionary<TKey, TValue> dictionary;
    private readonly TValue defaultValue;

    public delegate void ChangeEvent(object sender, DDictionaryChangeEventArgs<TKey, TValue> data);
    public event ChangeEvent onChange;
    
    #region CONSTRUTORES

    public DDictionary() : this(new Dictionary<TKey, TValue>(), default(TValue))
    {

    }

    public DDictionary(IDictionary<TKey, TValue> dictionary, TValue defaultValue = default)
    {
        this._dictionaryLock = new object();
        this.dictionary = dictionary ?? new Dictionary<TKey, TValue>();
        this.defaultValue = defaultValue;
    }

    #endregion
    #region MODIFICADORES

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        lock (_dictionaryLock) dictionary.Add(item);
        DDictionaryChangeEventArgs<TKey, TValue> parametros = new DDictionaryChangeEventArgs<TKey, TValue>
        {
            Key = item.Key,
            Value = item.Value,
            Operation = DDictionaryOperation.ADD,
            Success = true
        };
        onChange?.Invoke(this, parametros);
    }

    public void Add(TKey key, TValue value)
    {
        lock (_dictionaryLock) dictionary.Add(key, value);
        DDictionaryChangeEventArgs<TKey, TValue> parametros = new DDictionaryChangeEventArgs<TKey, TValue>
        {
            Key = key,
            Value = value,
            Operation = DDictionaryOperation.ADD,
            Success = true
        };
        onChange?.Invoke(this, parametros);
    }

    public bool Remove(TKey key)
    {
        bool retorno = false;
        lock (_dictionaryLock) retorno = dictionary.Remove(key);
        DDictionaryChangeEventArgs<TKey, TValue> parametros = new DDictionaryChangeEventArgs<TKey, TValue>
        {
            Key = key,
            Operation = DDictionaryOperation.REMOVE,
            Success = retorno
        };
        onChange?.Invoke(this, parametros);
        return retorno;
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        bool retorno = false;
        lock (_dictionaryLock) retorno = dictionary.Remove(item);
        DDictionaryChangeEventArgs<TKey, TValue> parametros = new DDictionaryChangeEventArgs<TKey, TValue>
        {
            Key = item.Key,
            Value = item.Value,
            Operation = DDictionaryOperation.REMOVE,
            Success = retorno
        };
        onChange?.Invoke(this, parametros);
        return retorno;
    }

    public void Clear()
    {
        lock (_dictionaryLock) dictionary.Clear();
        DDictionaryChangeEventArgs<TKey, TValue> parametros = new DDictionaryChangeEventArgs<TKey, TValue>
        {
            Operation = DDictionaryOperation.CLEAR,
            Success = true
        };
        onChange?.Invoke(this, parametros);
    }

    #endregion
    #region IDICTIONARY ENUMERATORS

    public int Count
    {
        get { lock (_dictionaryLock) return dictionary.Count; }
    }

    #region IDictionaryEnumerator

    IDictionaryEnumerator IDictionary.GetEnumerator()
    {
        lock (_dictionaryLock)
            return new DDictionaryEnumerator(dictionary.GetEnumerator());
    }

    public class DDictionaryEnumerator : IDictionaryEnumerator
    {
        public DDictionaryEnumerator(IEnumerator<KeyValuePair<TKey, TValue>> enumerator)
        {
            Enumerator = enumerator;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> Enumerator;

        public DictionaryEntry Entry
        {
            get { return new DictionaryEntry(Enumerator.Current.Key, Enumerator.Current.Value); }
        }

        public object Key
        {
            get { return Enumerator.Current.Key; }
        }

        public object Value
        {
            get { return Enumerator.Current.Value; }
        }

        public object Current
        {
            get { return Entry; }
        }

        public bool MoveNext()
        {
            return Enumerator.MoveNext();
        }

        public void Reset()
        {
            Enumerator.Reset();
        }
    }

    #endregion
    #region METODOS INTERNOS
    
    private void CopyTo(Array array, int arrayIndex)
    {
        lock (_dictionaryLock)
        {
            using (IEnumerator<KeyValuePair<TKey, TValue>> empEnumerator = dictionary.GetEnumerator())
            {
                for (int x = arrayIndex; empEnumerator.MoveNext(); x++)
                {
                    array.SetValue(empEnumerator.Current, x);
                }
            }
        }
    }

    #endregion

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        lock (_dictionaryLock) return dictionary.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        lock (_dictionaryLock) return dictionary.GetEnumerator();
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        lock (_dictionaryLock) return dictionary.Contains(item);
    }

    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
     => CopyTo(array, arrayIndex);

    public bool ContainsKey(TKey key)
    {
        lock (_dictionaryLock) return dictionary.ContainsKey(key);
    }

    public ICollection<TKey> Keys
    {
        get {
            TKey[] resultado = null;
            lock (_dictionaryLock)
            {
                resultado = new TKey[dictionary.Keys.Count];
                dictionary.Keys.CopyTo(resultado, 0);
            }
            return resultado;
        }
    }

    public ICollection<TValue> Values
    {
        get
        {
            lock (_dictionaryLock) return new List<TValue>(dictionary.Values);
        }
    }

    #endregion
    
    public bool IsReadOnly
    {
        get { return dictionary.IsReadOnly; }
    }

    public TValue this[TKey key]
    {
        get
        {
            lock (_dictionaryLock)
            {
                if (dictionary.ContainsKey(key))
                    return dictionary[key];
                else
                    return defaultValue;
            }
        }

        set
        {
            bool success = false;
            DDictionaryOperation op = DDictionaryOperation.SET;

            lock (_dictionaryLock)
            {
                if (dictionary.ContainsKey(key))
                {
                    if (!Object.Equals(dictionary[key], value))
                    {
                        dictionary[key] = value;
                        success = true;
                    }
                }
                else if (!Object.Equals(defaultValue, value))
                {                    
                    dictionary[key] = value;
                    success = true;
                    op = DDictionaryOperation.ADD;
                }
            }

            DDictionaryChangeEventArgs<TKey, TValue> parametros = new DDictionaryChangeEventArgs<TKey, TValue>
            {
                Key = key,
                Value = value,
                Operation = op,
                Success = success
            };
            onChange?.Invoke(this, parametros);         
        }
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        lock (_dictionaryLock)
        {
            if (!dictionary.TryGetValue(key, out value))
            {
                value = defaultValue;
                return false;
            }
        }
        return true;
    }

    #region IMPLEMENTACAO GENERIC IDICTIONARY
    
    int ICollection.Count => this.Count;    

    bool ICollection.IsSynchronized { get { return System.Threading.Monitor.TryEnter(_dictionaryLock); } }

    object IDictionary.this[object key] { get { return this[(TKey)key]; } set { this[(TKey)key] = (TValue)value; } }
    
    bool IDictionary.Contains(object key) { return this.ContainsKey((TKey)key); }

    void IDictionary.Add(object key, object value) { this.Add((TKey)key, (TValue)value); }

    void IDictionary.Clear() { this.Clear(); }

    void IDictionary.Remove(object key) { this.Remove((TKey)key); }
    
    void ICollection.CopyTo(Array array, int index) => CopyTo(array, index);

    ICollection IDictionary.Keys => new List<TKey>(dictionary.Keys);

    ICollection IDictionary.Values => throw new NotImplementedException();

    bool IDictionary.IsReadOnly => throw new NotImplementedException();

    bool IDictionary.IsFixedSize => throw new NotImplementedException();

    object ICollection.SyncRoot => throw new NotImplementedException();

    #endregion
}


