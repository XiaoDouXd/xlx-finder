using System.Collections;

#region ReSharper disable

// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable UnusedAutoPropertyAccessor.Global

#endregion

namespace XDFileCollection;

public enum XFileErr
{
    #region Path

    CreatePath,
    ReadPath,

    #endregion

    #region File



    #endregion

    #region Dir

    CreateDir,

    #endregion
}

public class XFileException : Exception
{
    public XFileErr Type { get; }
    public Exception Exception { get; }

    protected XFileException(XFileErr err, Exception e)
    {
        Type = err;
        Exception = e;
    }

    public override string ToString() => Exception.ToString();
    public override IDictionary Data => Exception.Data;
    public override string Message => Exception.Message;
    public override string? StackTrace => Exception.StackTrace;
    public override string? Source { get => Exception.Source; set => Exception.Source = value; }
    public override string? HelpLink { get => Exception.HelpLink; set => Exception.HelpLink = value; }
}

internal class XFileApplicationException : XFileException
{
    public XFileApplicationException(XFileErr err, string? msg = null, Exception? inner = null)
        : base(err, new ApplicationException(msg, inner)) {}
}

internal class XFileTypeInitializationException : XFileException
{
    public XFileTypeInitializationException(XFileErr err, Type type, Exception? innerException = null)
        : base(err, new TypeInitializationException(type.ToString(), innerException)) {}
}

internal class XFileIndexOutOfRangeException : XFileException
{
    public XFileIndexOutOfRangeException(XFileErr err, string? msg = null, Exception? inner = null)
        : base(err, new IndexOutOfRangeException(msg, inner)) {}
}

internal class XFileNullReferenceException : XFileException
{
    public XFileNullReferenceException(XFileErr err, string? msg = null, Exception? inner = null)
        : base(err, new NullReferenceException(msg, inner)) {}
}

internal class XFileArgumentException : XFileException
{
    public XFileArgumentException(XFileErr err, string? msg = null, string? paramName = null, Exception? inner = null)
        : base(err, new ArgumentException(msg, paramName, inner)) {}
}

internal class XFileArgumentNullException : XFileException
{
    public XFileArgumentNullException(XFileErr err, string? msg, string? paramName)
        : base(err, new ArgumentNullException(paramName, msg)) {}
    public XFileArgumentNullException(XFileErr err, string? msg = null, Exception? inner = null)
        : base(err, new ArgumentNullException(msg, inner)) {}
}

internal class XFileArgumentOutOfRangeException : XFileException
{
    public XFileArgumentOutOfRangeException(XFileErr err, string? msg, string? paramName)
        : base(err, new ArgumentOutOfRangeException(paramName, msg)) {}
    public XFileArgumentOutOfRangeException(XFileErr err, string? msg = null, Exception? inner = null)
        : base(err, new ArgumentOutOfRangeException(msg, inner)) {}
}