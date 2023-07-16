using System.Runtime.InteropServices;
using XDFileCollection.Common;
using XDFileCollection.XPathFormatter;

namespace XDFileCollection;

#region ReSharper disable

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable PossibleMultipleEnumeration

#endregion

public class XFileConfig : Singleton<XFileConfig>
{
    public IXPathFormatter PathFormatter
    {
        get
        {
            if (_pathFormatter != null) return _pathFormatter;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                _pathFormatter = new XPathFormatterWin32();
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                _pathFormatter = new XPathFormatterLinux();
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                _pathFormatter = new XPathFormatterOSX();
            else throw new NotSupportedException();
            return _pathFormatter;
        }
    }
    private IXPathFormatter? _pathFormatter;
}