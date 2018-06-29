using System;
using System.Collections;
using System.Collections.Generic;

public class DDictionary<TKey, TValue> : IDictionary<TKey, TValue>
{
    private readonly IDictionary<TKey, TValue> dictionary;
    private readonly TValue defaultValue;

    public delegate void ChangeEvent(object sender, DDictionaryChangeEventArgs<TKey, TValue> data);
    public event ChangeEvent onChange;

    public DDictionary() : this(new Dictionary<TKey, TValue>(), default(TValue))
    {

    }

    public DDictionary(IDictionary<TKey, TValue> dictionary, TValue defaultValue)
    {
        this.dictionary = dictionary;
        this.defaultValue = defaultValue;
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return dictionary.GetEnumerator();
    }

    #region MODIFICADORES

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        lock (dictionary) dictionary.Add(item);
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
        lock (dictionary) dictionary.Add(key, value);
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
        lock (dictionary) retorno = dictionary.Remove(key);
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
        lock (dictionary) retorno = dictionary.Remove(item);
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
        lock (dictionary) dictionary.Clear();
        DDictionaryChangeEventArgs<TKey, TValue> parametros = new DDictionaryChangeEventArgs<TKey, TValue>
        {
            Operation = DDictionaryOperation.CLEAR,
            Success = true
        };
        onChange?.Invoke(this, parametros);
    }

    #endregion

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        return dictionary.Contains(item);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        dictionary.CopyTo(array, arrayIndex);
    }

    public int Count
    {
        get { return dictionary.Count; }
    }

    public bool IsReadOnly
    {
        get { return dictionary.IsReadOnly; }
    }

    public bool ContainsKey(TKey key)
    {
        return dictionary.ContainsKey(key);
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        if (!dictionary.TryGetValue(key, out value))
        {
            value = defaultValue;
        }

        return true;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return dictionary.GetEnumerator();
    }

    public TValue this[TKey key]
    {
        get
        {
            if (dictionary.ContainsKey(key))
                return dictionary[key];
            else
                return defaultValue;
        }

        set
        {
            bool change = false;
            lock (dictionary)
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

    public ICollection<TKey> Keys
    {
        get { return dictionary.Keys; }
    }

    public ICollection<TValue> Values
    {
        get
        {
            var values = new List<TValue>(dictionary.Values) { defaultValue };
            return values;
        }
    }
}