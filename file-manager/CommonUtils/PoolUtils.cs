using System.Collections.Concurrent;

namespace FileManager.CommonUtils;

public interface IPoolHandle<T> where T : class, new()
{
    public T Set(T obj);
    public void Reset(T obj);
}

public class DefaultPoolHandle<T> : IPoolHandle<T> where T : class, new()
{
    public T Set(T obj) => obj;
    public void Reset(T obj) {}
}

public static class Pool<T> where T : class, new()
{
    private const int MaxSize = 1000;
    private static readonly ConcurrentStack<T> P = new();

    public static T DePool() => P.TryPop(out var v) ? v : new T();

    public static void InPool(T obj) { if (P.Count < MaxSize) P.Push(obj); }
}

public static class Pool<T, THandle> where T : class, new() where THandle : IPoolHandle<T>, new()
{
    private const int MaxSize = 1000;
    private static readonly ConcurrentStack<T> P = new();
    private static readonly THandle F = new();

    public static T DePool() => P.TryPop(out var v) ? F.Set(v) : F.Set(new T());
    public static void InPool(T obj)
    {
        F.Reset(obj);
        if (P.Count < MaxSize) P.Push(obj);
    }
}