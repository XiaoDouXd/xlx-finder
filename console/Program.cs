// 测试用的控制台项目

using FinderCore;

try
{
    long i = 0;
    Finder.I.Init();
    while (true)
    {
        i++;
    }
}
catch(Exception e)
{
    Console.WriteLine($"runtime error: {e}");
}