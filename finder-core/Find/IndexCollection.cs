using FinderCore.File.FileCollection;
using Lucene.Net.Index;

namespace FinderCore.Find;

internal class IndexCollection
{
    private readonly Dictionary<Guid, Term> _indices = new();

    private void FindIndex(in Guid id)
    {

    }

    private Guid NewIndex(FileCollection.IFileInfo fileInfo)
    {
        throw new NotImplementedException();
    }

    private Guid NewIndex(FindResult result)
    {
        throw new NotImplementedException();
    }

    private void UpdateIndex(FileCollection.IFileInfo fileInfo)
    {

    }
}