using System;
using System.Collections.Generic;

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
}