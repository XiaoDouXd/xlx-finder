#region ReSharper disable
// ReSharper disable UnusedMember.Global
// ReSharper disable ArrangeTrailingCommaInMultilineLists
#endregion

namespace XD.XFinder.Lang;

internal enum MatcherEnum
{
    None = default,
    Number,
    KeyWord,
    Bracket,
    TxtContent,
}

internal static class EnumUtil
{
    internal static int Int<TEnum>(this TEnum e) where TEnum : Enum
        => Convert.ToInt32(e);
    internal static long Long<TEnum>(this TEnum e) where TEnum : Enum
        => Convert.ToInt64(e);
    internal static sbyte SByte<TEnum>(this TEnum e) where TEnum : Enum
        => Convert.ToSByte(e);
}