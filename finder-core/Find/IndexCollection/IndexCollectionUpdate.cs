using System.Collections.Concurrent;
using FinderCore.CommonUtils;
using FinderCore.File;

namespace FinderCore.Find.IndexCollection;

internal partial class IndexCollection
{
    private class UpdateTaskInfo
    {
        public Guid Uuid;
        public bool IsDispatched;
        public IndexCreateUtils.CreateProcessController? Controller;
    }

    private class IndexInfo
    {
        public bool IsLocked;
    }

    public event Action<Guid>? CreateIndexFinishEvent;

    public bool IsIndexCreating(in Guid id)
        => _createIndexTasks.ContainsKey(id);

    public void LockIndex(in Guid id)
    {
        lock (_info)
            if (_info.TryGetValue(id, out var info)) info.IsLocked = true;
        UpdateInfoCache();
    }

    public void UnlockIndex(in Guid id)
    {
        lock (_info)
            if (_info.TryGetValue(id, out var info)) info.IsLocked = false;
        UpdateInfoCache();
    }

    private void CreateIndexStart(Guid uuid)
    {
        var task = CreateIndexFromFile(uuid);
        task.ContinueWith(t =>
        {
            if (t.Result.IsDispatched) return;
            if (!t.IsCompleted)
            {
                var path = GetIndexPath(uuid);
                if (Directory.Exists(path)) Directory.Delete(path);
            }
            _createIndexTasks.TryRemove(t.Result.Uuid, out _);

            lock (_info)
            {
                if (!_info.ContainsKey(t.Result.Uuid))
                    _info[t.Result.Uuid] = new IndexInfo();
            }
            CreateIndexFinishEvent?.Invoke(t.Result.Uuid);
        });
        task.Start();
    }

    private Task<UpdateTaskInfo> CreateIndexFromFile(Guid fileId)
    {
        if (FileMan.I.InitProcess != FileMan.FullInitProcess)
            return Task.FromException<UpdateTaskInfo>(new ApplicationException("file manager not inited"));
        var fileInfo = FileMan.I.GetFile(fileId);
        if (fileInfo == null)
            return Task.FromException<UpdateTaskInfo>(new ApplicationException("file not found"));
        if (_createIndexTasks.TryGetValue(fileId, out var oldTaskInfo))
        {
            if (oldTaskInfo.Controller != null)
            {
                lock (oldTaskInfo.Controller)
                {
                    oldTaskInfo.Controller.IsCancel = true;
                    oldTaskInfo.IsDispatched = true;
                }
            }
        }

        var ctrl = new IndexCreateUtils.CreateProcessController{ IsCancel = false };
        var taskInfo = new UpdateTaskInfo
        {
            Uuid = fileId,
            Controller = ctrl,
            IsDispatched = false
        };
        var task = new Task<UpdateTaskInfo>(() =>
        {
            var excelInfo = FileMan.I.ReadExcel(fileId);
            IndexCreateUtils.CreateOrUpdate(GetIndexPath(fileId), excelInfo, ctrl);
            return taskInfo;
        });
        _createIndexTasks.TryAdd(fileId, taskInfo);
        return task;
    }

    private void UpdateInfoCache()
    {
        lock (_info)
        {
            var maybeChangingInfos = new Stack<Guid>();
            foreach (var (uid, i) in _info)
                if (i.IsLocked) maybeChangingInfos.Push(uid);

            using var file = System.IO.File.Open(Const.IndexInfoCache, FileMode.Create);
            Utf8Json.JsonSerializer.Serialize(file, maybeChangingInfos);
        }
    }

    private static string GetIndexPath(in Guid uuid)
        => Const.IndexPath + uuid;
    private readonly ConcurrentDictionary<Guid, IndexInfo> _info = new();
    private readonly ConcurrentDictionary<Guid, UpdateTaskInfo> _createIndexTasks = new();
}