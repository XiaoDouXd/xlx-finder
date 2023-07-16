namespace XDFileCollection.XPathFormatter;

public class XPathFormatterLinux : IXPathFormatter
{
    public string ToPathStr(in IEnumerable<string> path)
    {
        throw new NotImplementedException();
    }

    public string[] ToPathChain(in ReadOnlySpan<char> path)
    {
        throw new NotImplementedException();
    }
}