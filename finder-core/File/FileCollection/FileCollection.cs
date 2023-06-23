using System.Collections;
using FinderCore.CommonUtils;

#region ReSharper disable

// ReSharper disable UnusedMember.Global
// ReSharper disable UnassignedField.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable ArrangeTrailingCommaInMultilineLists

#endregion

namespace FinderCore.File.FileCollection;

internal struct FileInfo
{
    // ReSharper disable FieldCanBeMadeReadOnly.Global
    public Guid Uuid = Guid.Empty;
    public string Path = string.Empty;
    public ulong[] Hash = Array.Empty<ulong>();
    // ReSharper restore FieldCanBeMadeReadOnly.Global
    public FileInfo() {}
}

internal partial class FileCollection : IReadOnlyCollection<FileCollection.IFileInfo>
{
    internal enum EChangeType
    {
        Add,
        Modify,
        Delete,
        Rename,
    }
    internal delegate void OnFileChange(Guid uuid, EChangeType changeType);
    internal event OnFileChange? ChangeEvent;

    internal IFileInfo? this[Guid idx] => _fileDict.TryGetValue(idx, out var node) ? node.FInfo : null;
    internal IFileInfo? this[IEnumerable<string> path] => Find(path)?.FInfo;
    internal IFileInfo? this[string path] => this[FileCollectionUtils.Path(path)];

    internal Guid AddSerializeInfo(in FileInfo info, bool sendFileChangeMessage = true)
    {
        if (_fileDict.ContainsKey(info.Uuid)) throw new ArgumentException("info key had exist");

        if (!System.IO.File.Exists(info.Path)) return Guid.Empty;
        var filePath = FileCollectionUtils.Path(info.Path);
        if (filePath.Length <= 1) throw new ArgumentException($"invalid file path: {info.Path}");
        if (Find(filePath) != null) throw new ArgumentException("file had exist in fileCollection");
        var node = GetOrCreateDirNode(filePath);
        var fileNode = Get(Node.EType.File, filePath[0], node);
        fileNode.SetInfo(info.Uuid, filePath, info.Hash);
        node.Add(fileNode);
        _fileDict.Add(info.Uuid, fileNode);
        if (sendFileChangeMessage) ChangeEvent?.Invoke(info.Uuid, EChangeType.Add);
        return info.Uuid;
    }

    internal Guid Add(string path, bool sendFileChangeMessage = true)
    {
        if (!System.IO.File.Exists(path)) throw new ArgumentException("file not found");
        var filePath = FileCollectionUtils.Path(path);
        if (filePath.Length <= 1) throw new ArgumentException($"invalid file path: {path}");
        if (Find(filePath) != null) throw new ArgumentException("file had exist in fileCollection");
        var node = GetOrCreateDirNode(filePath);
        var fileNode = Get(Node.EType.File, filePath[0], node);
        node.Add(fileNode);
        var uuid = new Guid();
        fileNode.SetInfo(uuid, filePath);
        _fileDict.Add(uuid, fileNode);
        if (sendFileChangeMessage) ChangeEvent?.Invoke(uuid, EChangeType.Add);
        return uuid;
    }

    public IEnumerator<IFileInfo> GetEnumerator()
    {
        foreach (var (_, node) in _fileDict)
            yield return node.FInfo!;
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    internal void Clear()
    {
        _fileDict.Clear();
        foreach (var (_, node) in _roots) node.Reset();
        _roots.Clear();
    }

    internal bool Contains(Guid id)
    {
        return _fileDict.ContainsKey(id);
    }

    internal bool Remove(Guid id)
    {
        if (!_fileDict.TryGetValue(id, out var node))
            return false;
        _fileDict.Remove(id);
        node.Reset();

        ChangeEvent?.Invoke(id, EChangeType.Delete);
        return true;
    }

    public int Count => _fileDict.Count;

    #region Inner

    private void MoveFile(Node curNode, in IEnumerable<string> target)
    {
        if (curNode.Type != Node.EType.File || curNode.FInfo == null)
            throw new ArgumentException("node is not file");
        var info = curNode.FInfo!;
        var node = Get(Node.EType.File, target.First(), GetOrCreateDirNode(target));
        node.SetInfo(info);
        _fileDict[info.Uuid] = node;
        ChangeEvent?.Invoke(info.Uuid, EChangeType.Rename);
    }

    private Node? Find(in IEnumerable<string> path)
    {
        var stack = new Stack<string>();
        foreach (var v in path)
            stack.Push(v);

        Node? curNode;
        if (stack.TryPop(out var rootStr))
        {
            if (!_roots.TryGetValue(rootStr, out curNode))
                return null;
        }
        else return null;

        while (stack.TryPop(out var str))
        {
            if (string.IsNullOrEmpty(str)) return null;
            var child = curNode.GetChild(str);
            if (child == null) return null;
            curNode = child;
        }
        return curNode;
    }

    private Node GetOrCreateDirNode(in IEnumerable<string> path, bool isTrimFirst = true)
    {
        var stack = new Stack<string>();
        foreach (var v in path)
        {
            if (isTrimFirst)
            {
                isTrimFirst = false;
                continue;
            }
            stack.Push(v);
        }

        Node? curNode;
        if (stack.TryPop(out var rootStr))
        {
            if (!_roots.TryGetValue(rootStr, out curNode))
            {
                if (string.IsNullOrEmpty(rootStr)) throw new ArgumentNullException(nameof(path));
                _roots.Add(rootStr, curNode = Get(Node.EType.Directory, rootStr));
            }
        }
        else throw new ArgumentNullException(nameof(path));

        while (stack.TryPop(out var str))
        {
            if (string.IsNullOrEmpty(str)) throw new ArgumentNullException(nameof(path));
            var child = curNode.GetChild(str);
            if (child == null) curNode.Add(child = Get(Node.EType.Directory, str, curNode));
            curNode = child;
        }
        return curNode;
    }

    private Node Get(Node.EType type, string name, Node? parent = null)
    {
        var node = Pool<Node>.DePool();
        node.Set(this, type, name, parent);
        return node;
    }

    private readonly Dictionary<Guid, Node> _fileDict = new();
    private readonly SortedDictionary<string, Node> _roots = new();

    #endregion
}