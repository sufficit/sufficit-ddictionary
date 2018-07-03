using System;
using System.Collections;
using System.Collections.Generic;

public class DDictionary<TKey, TValue> : IDictionary<TKey, TValue>
{
    private readonly object _dictionaryLock = new object();
    private readonly IDictionary<TKey, TValue> dictionary;
    private readonly TValue defaultValue;

    public delegate void ChangeEvent(object sender, DDictionaryChangeEventArgs<TKey, TValue> data);
    public event ChangeEvent onChange;
    
    #region CONSTRUTORES

    public DDictionary() : this(new Dictionary<TKey, TValue>(), default(TValue))
    {

    }

    public DDictionary(IDictionary<TKey, TValue> dictionary, TValue defaultValue)
    {
        this.dictionary = dictionary;
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

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        lock (_dictionaryLock) dictionary.CopyTo(array, arrayIndex);
    }

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
            lock (_dictionaryLock) return new List<TValue>(dictionary.Values) { defaultValue };
        }
    }

    #endregion
    
    public bool IsReadOnly
    {
        get { return dictionary.IsReadOnly; }
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
            bool change = false;
            lock (_dictionaryLock)
            {
                if (dictionary.ContainsKey(key))
                {
                    if (!Object.Equals(dictionary[key], value))
                    {
                        dictionary[key] = value;
                        change = true;
                    }
                }
                else if (!Object.Equals(defaultValue, value))
                {
                    dictionary[key] = value;
                    change = true;
                }
            }
            if (change)
            {
                DDictionaryChangeEventArgs<TKey, TValue> parametros = new DDictionaryChangeEventArgs<TKey, TValue>
                {
                    Key = key,
                    Value = value,
                    Operation = DDictionaryOperation.SET,
                    Success = true
                };
                onChange?.Invoke(this, parametros);
            }
        }
    }

}