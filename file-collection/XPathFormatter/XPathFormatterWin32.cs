﻿namespace XDFileCollection.XPathFormatter;

internal class XPathFormatterWin32 : IXPathFormatter
{
    private const char DirSeparatorChar = '/';
    private const char AltDirSeparatorChar = '\\';
    private const char VolumeSeparatorChar = ':';
    private const char DirLienChar = '.';

    public string[] ToPathChain(in ReadOnlySpan<char> path)
    {
        if (path.Length == 0) return Array.Empty<string>();

        // 把字符串分段
        Span<int> part = stackalloc int[path.Length];
        var idx = 0;
        var isFrom = true;
        for (var i = 0; i < path.Length; i++)
        {
            if (path[i] is DirSeparatorChar or VolumeSeparatorChar or AltDirSeparatorChar)
            {
                if (!isFrom)
                {
                    part[idx++] = i - 1;
                    if (idx > 1)
                    {
                        var to = part[idx - 1];
                        var from = part[idx - 2];
                        switch (to - from)
                        {
                            case 0 when path[from] == DirLienChar:
                                idx -= 2;
                                break;
                            case 1 when path[from] == DirLienChar && path[to] == DirLienChar:
                                idx -= 4;
                                break;
                        }
                    }
                }
                isFrom = true;
                continue;
            }
            if (!isFrom) continue;

            part[idx++] = i;
            isFrom = false;
        }

        // ReSharper disable ConvertIfStatementToSwitchStatement
        if (idx == 0) return Array.Empty<string>();
        if (idx % 2 != 0) part[idx++] = path.Length - 1;
        if (idx > 1)
        {
            var to = part[idx - 1];
            var from = part[idx - 2];
            switch (to - from)
            {
                case 0 when path[from] == DirLienChar:
                    idx -= 2;
                    break;
                case 1 when path[from] == DirLienChar && path[to] == DirLienChar:
                    idx -= 4;
                    break;
            }
        }
        if (idx < 0) throw new IndexOutOfRangeException();
        if (idx == 0) return Array.Empty<string>();
        // ReSharper restore ConvertIfStatementToSwitchStatement

        var list = new string[idx / 2];
        for (var i = 0; i < idx; i += 2)
        {
            var from = part[i];
            var to = part[i + 1];
            list[^(i / 2 + 1)] = path.Slice(from, to - from + 1).ToString();
        }
        return list;
    }

    public string ToPathStr(in IEnumerable<string> path)
    {
        var len = PathLenght(path);
        if (len <= 0) return string.Empty;

        Span<char> strStack = stackalloc char[len];
        // ReSharper disable once PossibleMultipleEnumeration
        using var e = path.GetEnumerator();
        var idx = len - 1;
        if (!e.MoveNext()) return string.Empty;
        var s1 = e.Current;
        while (e.MoveNext())
        {
            WriteDirNameReverseOrder(ref strStack, ref idx, s1);
            s1 = e.Current;
        }
        WriteDirRootReverseOrder(ref strStack, ref idx, s1, idx == len - 1);
        return strStack.ToString();
    }

    /// <summary>
    /// 计算路径长度
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private static int PathLenght(in IEnumerable<string> path)
    {
        var len = 0;
        var sepCount = 0;
        // ReSharper disable once PossibleMultipleEnumeration
        foreach (var s in path)
        {
            if (string.IsNullOrEmpty(s)) throw new ArgumentException("path invalid");
            sepCount++;
            len += s.Length;
        }
        len += sepCount + (sepCount > 1 ? 0 : 1);
        return len <= 1 ? 0 : len;
    }

    /// <summary>
    /// 倒序写入目录名
    /// </summary>
    /// <param name="sp"></param>
    /// <param name="curIdx"></param>
    /// <param name="name"></param>
    private static void WriteDirNameReverseOrder(
        ref Span<char> sp,
        ref int curIdx,
        in string name)
    {
        for (var i = name.Length - 1; i >= 0; i--) sp[curIdx--] = name[i];
        sp[curIdx--] = DirSeparatorChar;
    }

    /// <summary>
    /// 倒序写入根目录名
    /// </summary>
    /// <param name="sp"></param>
    /// <param name="curIdx"></param>
    /// <param name="name"></param>
    /// <param name="isWriteDirSep"></param>
    private static void WriteDirRootReverseOrder(
        ref Span<char> sp,
        ref int curIdx,
        in string name,
        in bool isWriteDirSep)
    {
        if (isWriteDirSep) sp[curIdx--] = DirSeparatorChar;
        sp[curIdx--] = VolumeSeparatorChar;
        for (var i = name.Length - 1; i >= 0; i--) sp[curIdx--] = name[i];
    }
}