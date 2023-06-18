using System.Collections;

#region Resharper disable
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
#endregion

namespace XD.XFinder.Lang.Base;

internal readonly struct StrSlice : IReadOnlyList<char>
{
    public readonly int Length;
    public readonly int StartIndex;
    public readonly string? Content;

    public StrSlice()
    {
        Content = null;
        Length = StartIndex = 0;
    }

    public StrSlice(string? content)
    {
        if (content == null)
        {
            Content = null;
            Length = StartIndex = 0;
            return;
        }

        StartIndex = 0;
        Length = content.Length;
        Content = content;
    }

    public StrSlice(string? content, int len)
    {
        if (content == null)
        {
            Content = null;
            Length = StartIndex = 0;
            return;
        }

        Content = content;
        Length = Math.Max(Math.Min(len, Content.Length), 0);
        StartIndex = 0;
    }

    public StrSlice(string? content, int startIndex, int len)
    {
        if (content == null)
        {
            Content = null;
            Length = StartIndex = 0;
            return;
        }

        Content = content;
        if (Content.Length <= 0)
        {
            Length = StartIndex = 0;
            return;
        }

        StartIndex = Math.Max(Math.Min(Content.Length - 1, startIndex), 0);
        Length = Math.Max(Math.Min(Content.Length - startIndex, len), 0);
    }

    public StrSlice(in StrSlice content)
    {
        Length = content.Length;
        Content = content.Content;
        StartIndex = content.StartIndex;
    }

    public StrSlice(in StrSlice content, int len)
    {
        Content = content.Content;
        StartIndex = content.StartIndex;
        Length = Math.Max(Math.Min(content.Length, len), 0);
    }

    public StrSlice(in StrSlice content, int startIndex, int len)
    {
        Content = content.Content;
        if (string.IsNullOrEmpty(Content))
        {
            StartIndex = Length = 0;
            return;
        }

        startIndex += content.StartIndex;
        StartIndex = Math.Max(Math.Min(Content.Length - 1, startIndex), 0);
        Length = Math.Max(Math.Min(Content.Length - startIndex, len), 0);
    }

    public bool StartWith(string? str)
    {
        if (string.IsNullOrEmpty(str)) return true;
        if (Length < str.Length) return false;
        // ReSharper disable once LoopCanBeConvertedToQuery
        for (var i = 0; i < str.Length; i++)
            if (this[i] != str[i])
                return false;
        return true;
    }

    public bool StartWith(in StrSlice str)
    {
        if (str.Length > Length) return false;
        // ReSharper disable once LoopCanBeConvertedToQuery
        for (var i = 0; i < str.Length; i++)
            if (this[i] != str[i])
                return false;
        return true;
    }

    public bool EndWith(string? str)
    {
        if (string.IsNullOrEmpty(str)) return true;
        if (Length < str.Length) return false;
        // ReSharper disable once LoopCanBeConvertedToQuery
        for (var i = str.Length - 1; i >= 0; i--)
            if (this[i] != str[i])
                return false;
        return true;
    }

    public bool EndWith(in StrSlice str)
    {
        if (str.Length > Length) return false;
        // ReSharper disable once LoopCanBeConvertedToQuery
        for (var i = str.Length - 1; i >= 0; i--)
            if (this[i] != str[i])
                return false;
        return true;
    }

    public ReadOnlySpan<char> AsSpan()
    {
        if (Content == null || Length == 0) return default;
        return Content.AsSpan(StartIndex, Length);
    }

    public IEnumerator<char> GetEnumerator() => Content!.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public static implicit operator StrSlice(string str) => new(str);
    public static implicit operator string(StrSlice str) => str.ToString();
    public static implicit operator ReadOnlySpan<char>(StrSlice str) => str.AsSpan();

    public static bool operator==(in StrSlice sp, string? str)
    {
        if (str == null) return sp.Content == null;
        if (sp.Length != str.Length) return false;
        // ReSharper disable once LoopCanBeConvertedToQuery
        for (var i = 0; i < str.Length; i++)
            if (sp[i] != str[i])
                return false;
        return true;
    }

    public static bool operator!=(in StrSlice sp, string? str)
    {
        if (str == null) return sp.Content != null;
        if (sp.Length != str.Length) return true;
        // ReSharper disable once LoopCanBeConvertedToQuery
        for (var i = 0; i < str.Length; i++)
            if (sp[i] != str[i])
                return true;
        return false;
    }

    public int Count => Length;
    public char this[int index] => index < Length
        ? Content![index + StartIndex]
        : throw new IndexOutOfRangeException(index.ToString());

    public override string ToString()
    {
        if (Content == null) return null!;
        if (Length == 0) return string.Empty;
        return Length == Content.Length
            ? Content : Content.Substring(StartIndex, Length);
    }

    public bool Equals(StrSlice other)
        => Length == other.Length && StartIndex == other.StartIndex && Content == other.Content;
    public override bool Equals(object? obj)
        => obj is StrSlice other && Equals(other);
    public override int GetHashCode()
        => HashCode.Combine(Length, StartIndex, Content);
}