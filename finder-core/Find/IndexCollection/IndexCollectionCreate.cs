using FinderCore.CommonUtils.Math;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using OfficeOpenXml;
using Directory = System.IO.Directory;

namespace FinderCore.Find.IndexCollection;

internal partial class IndexCollection
{
    public class CreateProcessController
    {
        public bool IsCancel;
    }

    private async void CreateOrUpdate(Guid id, string path, ExcelPackage package, CreateProcessController controller)
    {
        while (_info.TryGetValue(id, out var info) && !info.IsLocked) await Task.Delay(1000);
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        var dir = FSDirectory.Open(path);
        using var writer = new IndexWriter(dir, _analyzer.Value, IndexWriter.MaxFieldLength.LIMITED);
        foreach (var sheet in package.Workbook.Worksheets)
        {
            if (sheet == null) continue;
            var rowCount = sheet.Dimension.End.Row;
            var colCount = sheet.Dimension.End.Column;

            for (var i = 1; i <= rowCount; i++)
            {
                for (var j = 1; j <= colCount; j++)
                {
                    var obj = sheet.Cells[i, j].Text;
                    if (string.IsNullOrEmpty(obj)) continue;
                    if (controller.IsCancel) goto Cancel;
                    var doc = new Document();

                    if (controller.IsCancel) goto Cancel;
                    doc.Add(new Field("content",
                        true,
                        obj,
                        Field.Store.NO,
                        Field.Index.ANALYZED,
                        Field.TermVector.WITH_POSITIONS_OFFSETS));

                    if (controller.IsCancel) goto Cancel;
                    var number = new NumericField("pos", Field.Store.NO, false);
                    number.SetLongValue((long)new Vec2I(i, j));
                    doc.Add(number);

                    if (controller.IsCancel) goto Cancel;
                    lock (_writerLockObj) writer.AddDocument(doc);
                    if (controller.IsCancel) goto Cancel;
                }
            }
        }

        return;
        Cancel:
        if (Directory.Exists(path)) Directory.Delete(path);
    }

    private readonly object _writerLockObj = new();
    private readonly ThreadLocal<Analyzer> _analyzer = new(
        () => new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30));
}