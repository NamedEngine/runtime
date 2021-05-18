using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class DictionaryWrapper<T1, T2> : IReadOnlyDictionary<T1, T2> {
    readonly IReadOnlyDictionary<T1, T2>[] _dictionaries;
    
    public DictionaryWrapper(IReadOnlyDictionary<T1, T2>[] dictionaries) {
        _dictionaries = dictionaries;
    }

    public IEnumerator<KeyValuePair<T1, T2>> GetEnumerator() {
        return _dictionaries
            .SelectMany(dictionary => dictionary)
            .GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    public int Count => _dictionaries.Select(dictionary => dictionary.Count).Sum();
    public bool ContainsKey(T1 key) {
        return _dictionaries.Select(dictionary => dictionary.ContainsKey(key)).Any(c => c);
    }

    public bool TryGetValue(T1 key, out T2 value) {
        foreach (var dictionary in _dictionaries) {
            var got = dictionary.TryGetValue(key, out value);
            if (got) {
                return true;
            }
        }

        value = default;
        return false;
    }

    public T2 this[T1 key] {
        get {
            var found = TryGetValue(key, out var value);
            if (!found) {
                throw new KeyNotFoundException();
            }

            return value;
        }
    }

    public IEnumerable<T1> Keys {
        get {
            return _dictionaries
                .SelectMany(d => d)
                .Select(pair => pair.Key);
        }
    }

    public IEnumerable<T2> Values {
        get {
            return _dictionaries
                .SelectMany(d => d)
                .Select(pair => pair.Value);
        }
    }

    public static implicit operator DictionaryWrapper<T1, T2>(Dictionary<T1, T2> dict) =>
        new DictionaryWrapper<T1, T2>(new IReadOnlyDictionary<T1, T2>[] {dict});
}
