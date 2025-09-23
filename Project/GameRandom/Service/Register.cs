using System;
using System.Collections.Generic;

namespace GameRandom.Service;

public class Register<TKey, TValue>
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

    public TValue GetObjectFromRegister(TKey key)
    {
        if (key == null)
            throw new  ArgumentNullException("Key is null");

        if (_registerValues.TryGetValue(key, out var value))
        {
            return value;
        }
        
        throw new Exception($"Not find key: {key} on dictionary");
    }
}