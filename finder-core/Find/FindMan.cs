using System.Collections.Concurrent;
using FinderCore.CommonUtils;
using FinderCore.File;
using FinderCore.File.FileCollection;

namespace FinderCore.Find;

internal class FindMan : Singleton<FindMan>
{
    private readonly ConcurrentDictionary<Guid, FindTaskInfo> _infos = new();
    private readonly IndexCollection.IndexCollection _indexCollection = new();

    public void Init()
    {
        if (FileMan.I.InitProcess != FileMan.FullInitProcess)
            throw new ApplicationException("file manager not inited");
        FileMan.I.ChangeEvent += OnFileChanged;


    }

    private void UpdateInfo()
    {

    }

    private void OnFileChanged(Guid id, FileCollection.EChangeType eType, IEnumerable<Guid>? others)
    {
        if (id != Guid.Empty)
        {

        }


    }
}