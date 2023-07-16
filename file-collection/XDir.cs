#region ReSharper disable

// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable CollectionNeverQueried.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable ArrangeTrailingCommaInMultilineLists

#endregion

using XDFileCollection.Common;

namespace XDFileCollection;

public sealed class XDir
{
    public enum EChangedType
    {
        Changed,
        Deleted,
        Renamed,
        Created,
    }

    public struct ChangeInfo
    {
        public Guid Id;
        public EChangedType Type;
    }

    public event Action<ChangeInfo>? OnChanged;
    public XPath Path { get; }
    public XDir(in XPath path, IEnumerable<IFileInterpreter>? fileFilter = null)
    {
        var pathStr = path.ToString();
        if (!Directory.Exists(pathStr))
            throw new XFileArgumentException(XFileErr.CreateDir, "path not exists", nameof(path));
        var filter = fileFilter.Empty() ? DefaultFileInterpreter.Arr : fileFilter!;

        try
        {
            _watcher = new FileSystemWatcher(path.ToString());
            _root = new Node(path[0]);
            CollectDirs(_root, pathStr);
            _watcher.Deleted += OnWatcherDeleted;
            _watcher.Created += OnWatcherCreated;
            _watcher.Renamed += OnWatcherRenamed;
            _watcher.Changed += OnWatcherChanged;
        }
        catch (Exception e)
        {
            _root = Node.Default;
            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
            _watcher?.Dispose();
            _watcher = null!;
            throw new XFileTypeInitializationException(XFileErr.CreateDir, GetType(), e);
        }

        void CollectDirs(Node root, string dir)
        {
            foreach (var file in Directory.EnumerateFiles(dir, string.Empty, SearchOption.TopDirectoryOnly))
            {
                using var fs = new FileStream(file, FileMode.Open);
                CheckAndCreateFiles(fs, root);
            }

            foreach (var d in Directory.EnumerateDirectories(dir, string.Empty, SearchOption.TopDirectoryOnly))
            {
                var name = System.IO.Path.GetDirectoryName(d);
                if (string.IsNullOrEmpty(name)) continue;
                var node = new Node(name);
                CollectDirs(node, d);
                root.Child.Add(node);
            }

            void CheckAndCreateFiles(in FileStream fs, Node r)
            {
                foreach (var i in filter)
                {
                    object? d;
                    ulong[] hash;

                    try
                    {
                        if (!i.Check(fs, out d)) continue;
                        hash = HashUtil.Hash(fs);
                    }
                    catch (Exception) { continue; }
                    r.Files.Add(new XFile(i, fs.Name, hash) { Data = d });
                    return;
                }
            }
        }
    }

    private void OnWatcherDeleted(object sender, FileSystemEventArgs e)
    {

    }

    private void OnWatcherRenamed(object sender, FileSystemEventArgs e)
    {

    }

    private void OnWatcherChanged(object sender, FileSystemEventArgs e)
    {

    }

    private void OnWatcherCreated(object sender, FileSystemEventArgs e)
    {

    }

    private Node _root;
    private readonly FileSystemWatcher _watcher;
    private class Node
    {
        public string Name { get; }
        public List<Node> Child { get; } = new();
        public List<XFile> Files { get; } = new();
        public Node(string name) => Name = name;

        public static readonly Node Default = new(string.Empty);
    }
}