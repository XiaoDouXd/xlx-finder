#region Resharper disable
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
#endregion

namespace XD.XFinder.Lang.Base;

internal interface IResult
{
    public const byte ZeroDistance = 0x0;
    public const byte FullDistance = 0xFF;

    public int Type { get; }
    public object? Info { get; }
    public IEnumerable<IResult>? SubResult { get; }

    public byte Distance { get; }
    public StrSlice Content { get; }
}

internal interface IMatcher
{
    internal int MatcherType { get; }
    internal (IResult res, StrSlice left) Match(in StrSlice str);

    internal static void RelResultInst(IResult res)
    {
        if (res is not Result r) return;
        r.Info = default;
        r.Content = default;
        r.Distance = IResult.ZeroDistance;
        r.Type = default;
        r.SubResult = default;
        ResultsPool.Value.Push(r);
    }

    protected static Result NewResultInst()
    {
        if (!ResultsPool.IsValueCreated) return new Result();
        var pool = ResultsPool.Value;
        return pool.Count == 0 ? new Result() : pool.Pop();
    }

    protected sealed class Result : IResult
    {
        public int Type { get; set; }
        public object? Info { get; set; }
        public IEnumerable<IResult>? SubResult { get; set; }

        public byte Distance { get; set; } = IResult.ZeroDistance;
        public StrSlice Content { get; set; }
    }
    private static readonly Lazy<Stack<Result>> ResultsPool = new();
}