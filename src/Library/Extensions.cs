using System.Collections.Generic;
using System.Linq;

namespace Library;

public static class Extensions
{
    public static IEnumerable<T>? NullIfEmpty<T>(this IEnumerable<T>? list)
    {
        return list.IsNullOrEmpty() ? null : list;
    }

    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? list)
    {
        return list == null || !list.Any();
    }

    public static V? TryGetValue<K, V>(this IDictionary<K, V>? dict, K key)
    {
        return dict != null && dict.ContainsKey(key) ? dict[key] : default(V);
    }
}