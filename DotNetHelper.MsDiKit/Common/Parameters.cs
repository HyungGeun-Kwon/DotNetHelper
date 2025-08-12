using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetHelper.MsDiKit.Common
{
    public class Parameters
    {
        private readonly Dictionary<string, object?> _parameters = [];

        public Parameters Add(string key, object value) { _parameters[key] = value; return this; }
        public bool ContainsKey(string key) => _parameters.ContainsKey(key);

        public object? this[string key] => _parameters[key];

        public T GetValue<T>(string key)
        {
            if (!_parameters.TryGetValue(key, out object? value))
                throw new KeyNotFoundException($"Parameter with key '{key}' was not found.");

            if (value is not T tValue)
                throw new InvalidCastException($"Cannot convert parameter '{key}' to {typeof(T).Name}");

            return tValue;
        }

        public object? GetValue(string key)
        {
            if (!_parameters.TryGetValue(key, out object? value))
                throw new KeyNotFoundException($"Parameter with key '{key}' was not found.");

            return value;
        }

        public bool TryGetValue<T>(string key, out T? value)
        {
            if (_parameters.TryGetValue(key, out var v) && v is T t) { value = t; return true; }
            value = default; return false;
        }

        public bool TryGetValue(string key, out object? value)
        {
            if (_parameters.TryGetValue(key, out value)) return true;
            value = default; return false;
        }
    }
}
