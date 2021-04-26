using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

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
        Array.Resize<T>(ref x, x.Length + y.Length);
        Array.Copy(y, 0, x, oldLen, y.Length);
        return x;
    }

    public static Dictionary<T1, T2> ToDictionary<T1, T2>(this IEnumerable<KeyValuePair<T1, T2>> source) {
        return source.ToDictionary(pair => pair.Key, pair => pair.Value);
    }
}