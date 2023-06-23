using System.Collections.Concurrent;
using FinderCore.CommonUtils;

namespace FinderCore.Find;

internal class FindMan : Singleton<FindMan>
{
    private readonly ConcurrentDictionary<Guid, FindTaskInfo> _infos = new();

    public void Init()
    {

    }

    private void UpdateInfo()
    {

    }
}