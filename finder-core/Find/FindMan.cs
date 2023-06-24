using System.Collections.Concurrent;
using FinderCore.CommonUtils;
using FinderCore.File;
using FinderCore.File.FileCollection;

namespace FinderCore.Find;

internal class FindMan : Singleton<FindMan>
{
    private class FileChangdTaskInfo
    {

    }

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
        if (id != Guid.Empty) _changingFiles.Enqueue(id);
        if (others != null) foreach (var idOther in others) _changingFiles.Enqueue(idOther);

        _changingFileTask ??= Task.Run(() =>
        {
            while (true)
            {
                if (_changingFiles.IsEmpty)
                {
                    _changingFileTask = null;
                    return;
                }

                _changingFiles.TryDequeue(out var fId);
                _indexCollection.AddOrUpdate(fId);
            }
        });
    }

    private Task? _changingFileTask;
    private readonly ConcurrentQueue<Guid> _changingFiles = new();
}