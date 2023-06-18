namespace XD.XFinder.Lang.Base;

internal static class MatcherUtil
{
    public const char EscapeChar = '~';

    public static StrSlice TrimHead(in StrSlice str)
    {
        if (str.Length == 0) return str;

        var i = 0;
        while (i < str.Length && char.IsWhiteSpace(str[i])) i++;
        return new StrSlice(str, i, str.Length - i);
    }

    public static string Escape(in ReadOnlySpan<char> source, char escapeChar = '\\')
    {
        var isEscaping = false;
        Span<char> charArray = stackalloc char[source.Length];

        var i = 0;
        foreach (var c in source)
        {
            if (isEscaping)
            {
                charArray[i++] = c switch
                {
                    '0' => '\0',
                    'a' => '\a',
                    'b' => '\b',
                    'f' => '\f',
                    'n' => '\n',
                    'r' => '\r',
                    't' => '\t',
                    'v' => '\v',
                    _ => c
                };
                isEscaping = false;
                continue;
            }
            if (c == escapeChar)
            {
                isEscaping = true;
                continue;
            }
            charArray[i++] = c;
        }
        return charArray[..i].ToString();
    }
}