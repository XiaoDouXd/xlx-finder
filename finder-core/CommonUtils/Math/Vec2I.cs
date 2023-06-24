namespace FinderCore.CommonUtils.Math;

#region ReSharper disable

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

#endregion

public struct Vec2I
{
    public int R
    {
        get => X;
        set => X = value;
    }

    public int G
    {
        get => Y;
        set => Y = value;
    }

    public int X = 0;
    public int Y = 0;

    public Vec2I() {}
    public Vec2I(long combine)
    {
        Y = (int)combine;
        X = (int)(combine >> 32);
    }
    public Vec2I(in int x, in int y)
    {
        X = x;
        Y = y;
    }
    public Vec2I(in (int, int) tuple)
    {
        X = tuple.Item1;
        Y = tuple.Item2;
    }

    public static explicit operator long(in Vec2I self)
        => ((long)self.X << 32) | (uint)self.Y;

    public static explicit operator Vec2I(in long combine)
        => new(combine);

    public static implicit operator Vec2I(in (int, int) tuple)
        => new(tuple);

    public static implicit operator (int, int)(in Vec2I self)
        => (self.X, self.Y);
}