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
}