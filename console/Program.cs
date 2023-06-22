// 测试用的控制台项目

using FileManager;
using FileManager.FileCollection;
using XD.XFinder.Lang;

try
{
    var s =  FileCollectionUtils.Path(new string[] { "F", "aaa", "bbb" });
    Console.WriteLine($"|{s}|");

    var sList = FileCollectionUtils.Path("F://.//EF//.//AAAAAEEC /VR/./RRTV./.");
    Console.Write("{");
    foreach (var str in sList) Console.Write($" |{str}|,");
    Console.Write("}\n");
}
catch(Exception e)
{
    Console.WriteLine($"runtime error: {e}");
}