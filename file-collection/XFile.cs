
#region ReSharper disable

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

#endregion

namespace XDFileCollection;

public interface IFileInterpreter
{
    public string Type { get; }
    public object? Data(in FileStream fs);
    public bool Check(in FileStream fs, out object? cacheData);
}

public sealed class DefaultFileInterpreter : IFileInterpreter
{
    public static readonly DefaultFileInterpreter I = new();
    public static readonly DefaultFileInterpreter[] Arr = { I };

    public string Type => string.Empty;
    public object? Data(in FileStream fs) => null;
    public bool Check(in FileStream fs, out object? cacheData)
    {
        cacheData = null;
        return true;
    }

    private DefaultFileInterpreter() {}
}

public class XFile
{
    public event Action? OnFileStateChange;

    public object? Data { get; set; }
    public Guid Id { get; private set; }
    public XPath Path { get; private set; }
    public string Type => _interpreter.Type;

    internal ulong[] HashCode { get; private set; }
    internal XFile(IFileInterpreter interpreter, in XPath path, in ulong[] hash)
    {
        Path = path;
        Id = Guid.NewGuid();
        _interpreter = interpreter;
        HashCode = hash;
    }

    private IFileInterpreter _interpreter;
}