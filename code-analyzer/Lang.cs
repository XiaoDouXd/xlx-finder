using XD.XFinder.Lang.Base;

namespace XD.XFinder.Lang;

public static class LangDef
{
    public static void Test()
    {
        var sp1 = new StrSlice("     长太息以掩涕兮 find <''acd[z-67]>+");
        var sp2 = new StrSlice(@"  苍苔夕一眼底\细\t\0aaa  ", 0, int.MaxValue);
        Console.WriteLine($"/{sp1}/\n/{MatcherUtil.TrimHead(sp2).StartWith(" 苍苔")}/\n/{
            MatcherUtil.Escape(sp2)}/");
    }
}