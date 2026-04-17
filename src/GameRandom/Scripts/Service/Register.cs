using System;
using System.Collections.Generic;

namespace GameRandom.Service;

public class Register<TKey, TValue> : IDisposable
{
    private Dictionary<TKey, TValue> _registerValues = new Dictionary<TKey, TValue>();

    public void RegisterNewObject(TKey key, TValue value)
    {
        if (key is null)
            throw new ArgumentNullException(nameof(key));
        if (value is null)
            throw new ArgumentNullException(nameof(value));
        
        if (!_registerValues.TryAdd(key, value))
            Console.WriteLine("Dictionary have this key");
    }

    public bool GetObjectFromRegister(TKey key, out TValue? tValue)
    {
        if (key == null)
            throw new ArgumentNullException();

        return _registerValues.TryGetValue(key, out tValue);
    }


    public void Dispose()
    {
        _registerValues.Clear();
    }
}