﻿
namespace FinderCore.Find.IndexCollection;

internal partial class IndexCollection
{
    public void Add(in Guid id)
    {
        lock (_info)
        {
            if (_info.ContainsKey(id))
                throw new ArgumentException("key already existed");
        }

        var task = CreateIndexFromFile(id);
        task.ContinueWith(t =>
        {
            if (t.Result.IsDispatched) return;
            if (!t.IsCompleted)
            {
                var path = GetIndexPath(t.Result.Uuid);
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
}