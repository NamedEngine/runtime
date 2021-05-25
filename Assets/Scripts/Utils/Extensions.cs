using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class Extensions {
    public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer = null) {
        return new HashSet<T>(source, comparer);
    }
    
    public static T[] Concat<T>(this T[] x, T[] y) {
        if (x == null) {
            throw new ArgumentNullException(nameof(x));
        }
        if (y == null) {
            throw new ArgumentNullException(nameof(y));
        }
        
        int oldLen = x.Length;
        Array.Resize(ref x, x.Length + y.Length);
        Array.Copy(y, 0, x, oldLen, y.Length);
        return x;
    }

    public static Dictionary<T1, T2> ToDictionary<T1, T2>(this IEnumerable<KeyValuePair<T1, T2>> source) {
        return source.ToDictionary(pair => pair.Key, pair => pair.Value);
    }
    
    public static Dictionary<T1, T2> ToDictionary<T1, T2>(this IEnumerable<(T1, T2)> source) {
        return source.ToDictionary(pair => pair.Item1, pair => pair.Item2);
    }
    
    public static Dictionary<T2, T1> ToReverseDictionary<T1, T2>(this Dictionary<T1, T2> source) {
        return source.ToDictionary(pair => pair.Value, pair => pair.Key);
    }

    public static string StartWithLower(this string source) {
        if (source == "") {
            return source;
        }
        
        return char.ToLowerInvariant(source[0]) + source.Substring(1);
    }
    
    public static string StartWithUpper(this string source) {
        if (source == "") {
            return source;
        }
        
        return char.ToUpperInvariant(source[0]) + source.Substring(1);
    }
    
    public static string IfEmpty(this string source, string replacement) {
        if (source == "") {
            return replacement;
        }

        return source;
    }

    public static void Deconstruct<T1, T2>(this KeyValuePair<T1, T2> source, out T1 val1, out T2 val2) {
        val1 = source.Key;
        val2 = source.Value;
    }

    class OnceWrapper {
        bool _used;

        public System.Action Wrap(System.Action action) {
            void Wrapper() {
                if (_used) {
                    return;
                }

                _used = true;
                action();
            }

            return Wrapper;
        }
    }
    public static System.Action Once(this System.Action source) {
        return new OnceWrapper().Wrap(source);
    }

    public static string TypeToString(this IValue value) {
        string type;
        if (value.GetValueType() != ValueType.Null) {
            if (value is IVariable) {
                type = "Variable";
            } else {
                type = "Value";
            }

            type += $"[{value.GetValueType()}]";
        } else {
            type = value.GetType().ToString();
        }

        return type;
    }

    public const char StringPathSeparator = '/';
    public static string ToProperPath(this string str) {
        return Path.Combine(str.Split(StringPathSeparator));
    }

    public static string GetPathRelativeTo(this string path, string relativeTo) {
        var relativeDir = Path.GetDirectoryName(relativeTo) ?? "/";
        return Path.Combine(relativeDir, path);
    }

    public static bool HasExtension(this string path, string extension) {
        return path.ToLower().EndsWith('.' + extension.ToLower());
    }
    
    public static Coroutine StartThrowingCoroutine(this MonoBehaviour monoBehaviour, IEnumerator enumerator, Func<Exception, bool> exceptHandler) {
        return monoBehaviour.StartCoroutine(enumerator.OnException(exceptHandler));
    }

    static IEnumerator OnException(this IEnumerator enumerator, Func<Exception, bool> exceptHandler) {
        while (true) {
            object current;
            try {
                if (enumerator.MoveNext() == false) {
                    break;
                }
                current = enumerator.Current;
            }
            catch (Exception ex) {
                if (!exceptHandler(ex)) {
                    throw;
                }
                yield break;
            }
            yield return current;
        }
    }
}
