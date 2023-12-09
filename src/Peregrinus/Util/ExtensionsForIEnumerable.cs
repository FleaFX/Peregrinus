using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Peregrinus.Util; 

public static class ExtensionsForIEnumerable {
    public static T Reduce<T>(this IEnumerable<T> enumerable, Func<T, T, T> reducer) {
        var arr = enumerable?.ToArray() ?? throw new ArgumentNullException(nameof(enumerable));
        if (!arr.Any()) throw new ArgumentOutOfRangeException(nameof(enumerable));
        var res = arr[0];
        for (var i = 1; i < arr.Length; i++) {
            res = reducer(res, arr[i]);
        }
        return res;
    }

    public static async Task<IEnumerable<U>> SelectAsync<T, U>(this IEnumerable<T> enumerable, Func<T, Task<U>> selector) {
        var results = new List<U>();
        foreach (var task in enumerable.Select(selector)) {
            results.Add(await task);
        }
        return results;
    }
}