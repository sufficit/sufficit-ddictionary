using System;

public class DDictionaryChangeEventArgs<TKey, TValue> : EventArgs
{
    public TKey Key { get; set; }
    public TValue Value { get; set; }
    public DDictionaryOperation Operation { get; set; }
    public bool Success { get; set; }
}
