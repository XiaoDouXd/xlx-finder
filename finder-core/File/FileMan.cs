using System.Collections.Concurrent;
using FinderCore.CommonUtils;
using OfficeOpenXml;

namespace FinderCore.File;

// ReSharper disable once ClassNeverInstantiated.Global
internal class FileDataCache
{
#pragma warning disable CS8618
#pragma warning disable CS0649
    public List<FileCollection.FileInfo> Infos;
#pragma warning restore CS0649
#pragma warning restore CS8618
}

internal class FileMan : Singleton<FileMan>
{
    public const byte FullInitProcess = byte.MaxValue;
    public byte InitProcess { get; private set; }
    public event Action<Guid, FileCollection.FileCollection.EChangeType, IEnumerable<Guid>?>? ChangeEvent;

    public ConcurrentStack<Guid> Init()
    {
        if (InitProcess == FullInitProcess)
            throw new ApplicationException("reinit file manager");

        var changedFiles = new ConcurrentStack<Guid>();
        if (System.IO.File.Exists(Const.FileCache))
        {
            using var fileStream = System.IO.File.Open(Const.FileCache, FileMode.Open);
            var cache = Utf8Json.JsonSerializer.Deserialize<FileDataCache>(fileStream);
            if (cache?.Infos != null)
            {
                for (var i = 0; i < cache.Infos.Count; i++)
                {
                    var info = cache.Infos[i];
                    var (id, isChanged) = _fileCollection.AddSerializeInfo(info, false);
                    if (id != Guid.Empty && isChanged) changedFiles.Push(id);
                    InitProcess = (byte)((float)i / cache.Infos.Count * FullInitProcess);
                }
            }
        }

        InitProcess = FullInitProcess;
        _fileCollection.ChangeEvent += OnFileChanged;

        if (!changedFiles.IsEmpty) UpdateFileCache();
        return changedFiles;
    }

    public void AddDir(string dirPath)
    {
        if (!Directory.Exists(dirPath)) return;
        var files = Directory.GetFiles(dirPath, string.Empty, SearchOption.AllDirectories);
        var task = Task.Run(() =>
        {
            var idList = new List<Guid>();
            foreach (var path in files)
            {
                var f = Path.GetFullPath(path);
                if (!System.IO.File.Exists(f)) continue;
                if (Path.GetExtension(f) != ".xlsx") continue;
                ExcelPackage? pkg;
                try { pkg = new ExcelPackage(path); }
                catch (Exception) { continue; }

                Guid id;
                lock (_lockObj)
                {
                    id = _fileCollection.Add(f, false);
                    if (id != Guid.Empty) idList.Add(id);
                }
                _excelCache[id] = pkg;
            }
            return idList;
        });

        task.ContinueWith(ctx =>
        {
            if (ctx.IsCompleted)
                ChangeEvent?.Invoke(Guid.Empty, FileCollection.FileCollection.EChangeType.Add, ctx.Result);
            UpdateFileCache();
        });
    }

    public FileCollection.FileCollection.IFileInfo? GetFile(in Guid id)
    {
        lock (_lockObj) return _fileCollection[id];
    }

    public ExcelPackage ReadExcel(in Guid id)
    {
        lock (_lockObj)
        {
            if (_excelCache.TryGetValue(id, out var cache))
                return cache;

            var fileInfo = _fileCollection[id];
            if (fileInfo == null) throw new ApplicationException("file not found");
            var excelPackage = new ExcelPackage(new FileInfo(fileInfo.FullPath));
            _excelCache.TryAdd(id, excelPackage);
            return excelPackage;
        }
    }

    private void UpdateFileCache()
    {
        lock (_lockObj) if (_fileCollection.Count == 0) return;
        var data = new FileDataCache { Infos = new List<FileCollection.FileInfo>() };

        lock (_lockObj)
        {
            foreach (var info in _fileCollection)
            {
                data.Infos.Add(new FileCollection.FileInfo
                {
                    Hash = info.Hash,
                    Uuid = info.Uuid,
                    Path = info.FullPath
                });
            }
        }

        using var file = System.IO.File.Open(Const.FileCache, FileMode.Create);
        Utf8Json.JsonSerializer.Serialize(file, data);
    }

    private void OnFileChanged(Guid uuid, FileCollection.FileCollection.EChangeType eType)
    {
        switch (eType)
        {
            case FileCollection.FileCollection.EChangeType.Add:
                ChangeEvent?.Invoke(uuid, FileCollection.FileCollection.EChangeType.Add, null);
                break;
            case FileCollection.FileCollection.EChangeType.Modify:
            {
                if (_excelCache.Remove(uuid, out var package)) package.Dispose();
                ChangeEvent?.Invoke(uuid, FileCollection.FileCollection.EChangeType.Modify, null);
                break;
            }
            case FileCollection.FileCollection.EChangeType.Delete:
            {
                if (_excelCache.Remove(uuid, out var package)) package.Dispose();
                ChangeEvent?.Invoke(uuid, FileCollection.FileCollection.EChangeType.Delete, null);
                break;
            }
            case FileCollection.FileCollection.EChangeType.Rename:
                ChangeEvent?.Invoke(uuid, FileCollection.FileCollection.EChangeType.Rename, null);
                break;
            default: throw new ArgumentOutOfRangeException(nameof(eType), eType, null);
        }
    }

    private readonly object _lockObj = new();
    private readonly FileCollection.FileCollection _fileCollection = new();
    private readonly ConcurrentDictionary<Guid, ExcelPackage> _excelCache = new();
}