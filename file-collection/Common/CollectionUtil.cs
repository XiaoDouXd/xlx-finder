namespace XDFileCollection.Common;

public static class CollectionUtil
{
    public static bool Empty<T>(this IEnumerable<T>? e)
    {
        // ReSharper disable LoopCanBeConvertedToQuery
        if (e == null) return true;
        foreach (var _ in e) return false;
        return true;
        // ReSharper restore LoopCanBeConvertedToQuery
    }
}