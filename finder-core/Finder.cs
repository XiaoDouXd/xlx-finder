﻿using System.Diagnostics.CodeAnalysis;
using FinderCore.CommonUtils;
using FinderCore.File;
using FinderCore.Find;

namespace FinderCore;

#region ReSharper disable

// ReSharper disable MemberCanBeMadeStatic.Global
// ReSharper disable ArrangeTrailingCommaInMultilineLists

#endregion

public struct Pattern
{
    public enum EType
    {
        Normal,
        Regex,
        Wildcard,
    }

    public EType Type;
    public double NumContent;
    public string StrContent;

    public bool IsFullText;
    public bool IsIgnoreCase;
}

public struct FindTask
{
    public Pattern Target;
    public Pattern Content;
}

public class Finder : Singleton<Finder>
{
    public event Action<Guid>? OnFindEnd;
    public event Action<Guid>? OnFindStart;
    public event Action<Guid>? OnFindUpdate;

    public byte Process => (byte)(FileMan.I.InitProcess / 2);
    public void Init()
    {
        var changed = FileMan.I.Init();
        FindMan.I.Init(changed);

        FileMan.I.AddDir("../../workspace");
    }

    public Guid SubmitTask(in FindTask taskInfo)
    {
        return new Guid();
    }

    public FindResult? TryGetResult(in Guid uuid)
    {
        return null;
    }
}