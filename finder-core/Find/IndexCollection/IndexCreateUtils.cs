using FinderCore.CommonUtils;
using FinderCore.CommonUtils.Math;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Store;
using OfficeOpenXml;
using Directory = System.IO.Directory;

namespace FinderCore.Find.IndexCollection;

public static class IndexCreateUtils
{
    public class CreateProcessController
    {
        public bool IsCancel;
    }

    private static readonly ThreadLocal<Analyzer> Analyzer = new(
        () => new StandardAnalyzer(Lucene.Net.Util.Version.LUCENE_30));

    public static void CreateOrUpdate(string path, ExcelPackage package, CreateProcessController controller)
    {
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        var dir = FSDirectory.Open(path);
        using var writer = new IndexWriter(dir, Analyzer.Value, IndexWriter.MaxFieldLength.LIMITED);
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
                    lock (WriterLockObj) writer.AddDocument(doc);
                    if (controller.IsCancel) goto Cancel;
                }
            }
        }

        return;
        Cancel:
        if (Directory.Exists(path)) Directory.Delete(path);
    }

    private static readonly object WriterLockObj = new();
}