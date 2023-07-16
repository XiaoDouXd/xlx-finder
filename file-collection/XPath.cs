using System.Collections;

#region ReSharper disable

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable PossibleMultipleEnumeration

#endregion

namespace XDFileCollection;

public interface IXPathFormatter
{
    /// <summary>
    /// 文件夹串 (倒序) 转路径
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public string ToPathStr(in IEnumerable<string> path);

    /// <summary>
    /// 路径转文件夹串 (倒序)
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public string[] ToPathChain(in ReadOnlySpan<char> path);
}

public readonly struct XPath : IReadOnlyList<string>, IComparable<XPath>
{
    public int Count => PathChain.Count;
    public string this[int index] => PathChain[index];

    public readonly string Path = string.Empty;
    public readonly IReadOnlyList<string> PathChain = Array.Empty<string>();

    public XPath() {}

    public XPath(string? path)
    {
        try
        {
            if (string.IsNullOrEmpty(path)) return;
            path = System.IO.Path.GetFullPath(path);
            PathChain = string.IsNullOrEmpty(path)
                ? Array.Empty<string>()
                : ToPathChain(path);
            Path = ToPathStr(PathChain);
        }
        catch (Exception e)
        {
            Path = string.Empty;
            PathChain = Array.Empty<string>();
            throw new XFileTypeInitializationException(XFileErr.CreatePath, GetType(), e);
        }
    }

    public XPath(IEnumerable<string>? path)
    {
        if (path == null) return;
        var count = path.Count();
        if (count == 0) return;

        try
        {
            var list = new string[count];
            count = 0;
            foreach (var str in path) list[count++] = str;
            PathChain = list;
            Path = ToPathStr(PathChain);
        }
        catch (Exception e)
        {
            Path = string.Empty;
            PathChain = Array.Empty<string>();
            throw new XFileTypeInitializationException(XFileErr.CreatePath, GetType(), e);
        }
    }

    public IEnumerator<string> GetEnumerator() => PathChain.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString() => Path;
    public int CompareTo(XPath obj) => string.Compare(Path, obj.Path, StringComparison.Ordinal);
    public override int GetHashCode() => Path.GetHashCode();
    public bool Equals(in XPath other) => Path == other.Path;
    public override bool Equals(object? obj)
    {
        if (obj == null || GetType() != obj.GetType()) return false;
        return Path == obj.ToString();
    }

    public static implicit operator XPath(string self) => new(self);
    public static bool operator ==(XPath left, XPath right) => left.Path == right.Path;
    public static bool operator !=(XPath left, XPath right) => left.Path != right.Path;

    public bool Contains(in XPath path)
    {
        if (path.Count == 0) return false;
        if (this == path) return true;
        if (path.Count <= Count) return false;

        for (var i = path.Count - 1; i >= 0; i--)
        {
            var idx = Count - (path.Count - i);
            if (idx < 0) return true;
            if (path.PathChain[i] != PathChain[idx]) return false;
        }
        return true;
    }

    public int Sub(in XPath path)
    {
        if (path.Count == 0) return -1;
        if (this == path) return 0;
        if (path.Count <= Count) return -1;

        var i = path.Count - 1;
        for (; i >= 0; i--)
        {
            var idx = Count - (path.Count - i);
            if (idx < 0) return i;
            if (path.PathChain[i] != PathChain[idx]) return -1;
        }
        return i;
    }

    public XPath Parent(int i = 0) => new(this, i);
    private XPath(in XPath child, int parentLv)
    {
        try
        {
            if (parentLv < 0 || parentLv >= child.Count - 1)
                throw new XFileIndexOutOfRangeException(XFileErr.CreatePath);
            var list = new string[child.Count - parentLv];
            for (var i = 1; i < child.Count; i++) list[i - 1] = child[i];
            PathChain = list;
            Path = ToPathStr(PathChain);
        }
        catch (Exception e)
        {
            Path = string.Empty;
            PathChain = Array.Empty<string>();
            throw new XFileTypeInitializationException(XFileErr.CreatePath, GetType(), e);
        }
    }

    private static string ToPathStr(in IEnumerable<string> path)
        => XFileConfig.I.PathFormatter.ToPathStr(path);
    private static string[] ToPathChain(in ReadOnlySpan<char> path)
        => XFileConfig.I.PathFormatter.ToPathChain(path);
}