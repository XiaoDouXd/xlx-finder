
namespace FinderCore.Find.IndexCollection;

internal partial class IndexCollection
{
    public void Add(in Guid id)
    {
        if (_info.ContainsKey(id))
            throw new ArgumentException("key already existed");
        CreateIndexStart(id);
    }
}