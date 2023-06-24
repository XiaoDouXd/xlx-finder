using System.Collections;
using FinderCore.CommonUtils;

namespace FinderCore.File.FileCollection;

#region ReSharper disable

// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local

#endregion

internal partial class FileCollection
{
    internal interface IFileInfo
    {
        internal Guid Uuid { get; }
        internal ulong[] Hash { get; }
        internal string FullPath { get; }
        internal IEnumerable<string> Path { get; }
    }

    private class Node : IEnumerable<string>
    {
        internal enum EType : byte
        {
            File,
            Directory
        }

        internal class FileInfo : IFileInfo
        {
            public Guid Uuid { get; }
            public Node? Affiliation { get; set; }
            public ulong[] Hash { get; }
            public string FullPath => Watcher.Path;
            public IEnumerable<string> Path => Affiliation ?? Enumerable.Empty<string>();
            internal readonly FileSystemWatcher Watcher;

            private void OnFileChanged(object sender, FileSystemEventArgs e)
                => Affiliation?.OnFileChanged(this, EChangeType.Modify, e);
            private void OnFileDeleted(object sender, FileSystemEventArgs e)
                => Affiliation?.OnFileChanged(this, EChangeType.Delete, e);
            private void OnFileRenamed(object sender, FileSystemEventArgs e)
                => Affiliation?.OnFileChanged(this, EChangeType.Rename, e);

            internal FileInfo(in Guid uuid, in Node affiliation, string path)
            {
                Uuid = uuid;
                Affiliation = affiliation;
                Watcher = new FileSystemWatcher(path);
                Watcher.Changed += OnFileChanged;
                Watcher.Deleted += OnFileDeleted;
                Watcher.Renamed += OnFileRenamed;
                Hash = FileCollectionUtils.Hash(path);
            }

            internal FileInfo(in Guid uuid, in Node affiliation, ulong[] hash, string path)
            {
                Uuid = uuid;
                Affiliation = affiliation;
                Watcher = new FileSystemWatcher(path);
                Watcher.Changed += OnFileChanged;
                Watcher.Deleted += OnFileDeleted;
                Watcher.Renamed += OnFileRenamed;
                Hash = hash;
            }
        }

        #region Props

        internal readonly SortedDictionary<string, Node> Children = new();
        internal Node? Parent { get; private set; }

        internal EType Type { get; private set; }
        internal string? Name { get; private set; }
        internal FileInfo? FInfo { get; private set; }

        #endregion

        internal void Set(FileCollection affiliation, EType type, string name, Node? parent = null)
        {
            Type = type;
            Name = name;
            Parent = parent;
            _affiliation = affiliation;
        }

        internal void Reset() => Reset(false);

        internal void SetInfo(Guid uuid, IEnumerable<string>? path = null)
        {
            if (Type != EType.File)
                throw new ArgumentException("node is not a file");

            var realPath = Path(path);
            if (!Directory.Exists(realPath))
                throw new FileNotFoundException("file not found", realPath);
            FInfo = new FileInfo(uuid, this, realPath);
        }

        internal void SetInfo(Guid uuid, IEnumerable<string> path, ulong[] hash)
        {
            FInfo = new FileInfo(uuid, this, hash, FileCollectionUtils.Path(path));
        }

        internal void SetInfo(FileInfo info)
        {
            FInfo = info;
            FInfo.Affiliation = this;
        }

        internal Node? GetChild(string str)
        {
            if (string.IsNullOrEmpty(str)) throw new ArgumentNullException(nameof(str));
            return Children.TryGetValue(str, out var node) ? node : null;
        }

        internal void Add(Node node)
        {
            if (Type == EType.File) throw new ArgumentException("try to add child node to a file");
            if (string.IsNullOrEmpty(node.Name)) throw new ArgumentException("invalid node");
            Children.Add(node.Name, node);
        }

        internal string Path(IEnumerable<string>? path = null)
        {
            return FileCollectionUtils.Path(path ?? this);
        }

        public IEnumerator<string> GetEnumerator()
        {
            var currNode = this;
            while (currNode != null)
            {
                yield return currNode.Name!;
                currNode = currNode.Parent;
            }
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #region Inner

        private void Reset(bool fromParent)
        {
            if (!fromParent)
            {
                if (Parent != null)
                {
                    if (Parent.Children.Count <= 1) Parent.Reset();
                    return;
                }
                _affiliation!._roots.Remove(Name!);
            }

            Name = null;
            FInfo = null;
            Parent = null;
            _affiliation = null;
            if (FInfo != null) FInfo.Affiliation = null;
            foreach (var (_, child) in Children) child.Reset(true);
            Children.Clear();
            Pool<Node>.InPool(this);
        }

        private void OnFileChanged(in FileInfo fileInfo, in EChangeType changeType, in FileSystemEventArgs arg)
        {
            switch (changeType)
            {
                case EChangeType.Modify:
                    _affiliation?.ChangeEvent?.Invoke(fileInfo.Uuid, EChangeType.Modify);
                    break;
                case EChangeType.Delete:
                    _affiliation?.Remove(fileInfo.Uuid);
                    break;
                case EChangeType.Rename:
                    _affiliation?.MoveFile(fileInfo.Affiliation!, FileCollectionUtils.Path(arg.FullPath));
                    break;
                case EChangeType.Add: break;
                default: throw new ArgumentOutOfRangeException(nameof(changeType), changeType, null);
            }
        }

        private FileCollection? _affiliation;

        #endregion

    }
}