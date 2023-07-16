
#region ReSharper disable

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

#endregion

namespace XDFileCollection;

public interface IFileInterpreter
{
    public string Type { get; }
    public object Data(in FileStream fs);
    public bool Check(in FileStream fs, out object? cacheData);
}

public class XFile
{
    public event Action? OnFileStateChange;

    public object? Data { get; set; }
    public Guid Id { get; private set; }
    public XPath Path { get; private set; }
    public string Type => _interpreter.Type;

    internal long[] HashCode { get; private set; } = Array.Empty<long>();
    internal XFile(IFileInterpreter interpreter, in XPath path)
    {
        Path = path;
        Id = Guid.NewGuid();
        _interpreter = interpreter;
    }

    private IFileInterpreter _interpreter;
}