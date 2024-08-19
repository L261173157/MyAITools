using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace MyAiTools.AiFun.plugins.MyPlugin;

public static class PdfHelper
{
    /// <summary>
    ///     读取PDF内容
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static IEnumerable<string> ExtractText(string filePath)
    {
        using var r = new PdfReader(filePath);
        using var doc = new PdfDocument(r);
        for (var i = 1; i <= doc.GetNumberOfPages(); i++)
        {
            ITextExtractionStrategy strategy = new LocationTextExtractionStrategy();
            var text = PdfTextExtractor.GetTextFromPage(doc.GetPage(i), strategy);
            yield return text;
        }
    }
}