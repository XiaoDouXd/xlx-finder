namespace FinderCore.CommonUtils;

#region ReSharper disable

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

#endregion

public class Singleton<T> where T : Singleton<T>, new()
{
    public static T I => _inst.Value;
    public static void Clear() => _inst = new Lazy<T>();

    protected Singleton() {}
    private static Lazy<T> _inst = new();
}