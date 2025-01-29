using System.Configuration;
using System.Text;
using Microsoft.Extensions.Configuration;
using NPOI.XWPF.UserModel;

namespace MyAiTools.AiFun.plugins.MyPlugin;

public static class NpoiHandler
{
    private static IConfiguration Configuration { get; } = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .Build();

    /// <summary>
    ///     读取Word内容
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static string ReadWordText(string fileName)
    {
        var WordTableCellSeparator = Configuration["WordTableCellSeparator"];
        var WordTableRowSeparator = Configuration["WordTableRowSeparator"];
        var WordTableSeparator = Configuration["WordTableSeparator"];
        //
        var CaptureWordHeader = Configuration["CaptureWordHeader"];
        var CaptureWordFooter = Configuration["CaptureWordFooter"];
        var CaptureWordTable = Configuration["CaptureWordTable"];
        var CaptureWordImage = Configuration["CaptureWordImage"];
        //
        var CaptureWordImageFileName = Configuration["CaptureWordImageFileName"];
        //
        var fileText = string.Empty;
        var sbFileText = new StringBuilder();

        #region 打开文档

        XWPFDocument document = null;
        try
        {
            using (var file = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                document = new XWPFDocument(file);
            }
        }
        catch (Exception e)
        {
            Console.Write("文件{0}打开失败，错误：{1}", new[] { fileName, e.ToString() });
        }

        #endregion

        #region 页眉、页脚

        //页眉
        if (CaptureWordHeader == "true")
        {
            sbFileText.AppendLine("Capture Header Begin");
            foreach (var xwpfHeader in document.HeaderList)
                sbFileText.AppendLine(string.Format("{0}", new[] { xwpfHeader.Text }));

            sbFileText.AppendLine("Capture Header End");
        }

        //页脚
        if (CaptureWordFooter == "true")
        {
            sbFileText.AppendLine("Capture Footer Begin");
            foreach (var xwpfFooter in document.FooterList)
                sbFileText.AppendLine(string.Format("{0}", new[] { xwpfFooter.Text }));

            sbFileText.AppendLine("Capture Footer End");
        }

        #endregion

        #region 表格

        if (CaptureWordTable == "true")
        {
            sbFileText.AppendLine("Capture Table Begin");
            foreach (var table in document.Tables)
            {
                //循环表格行
                foreach (var row in table.Rows)
                {
                    foreach (var cell in row.GetTableCells())
                    {
                        sbFileText.Append(cell.GetText());
                        //
                        sbFileText.Append(WordTableCellSeparator);
                    }

                    sbFileText.Append(WordTableRowSeparator);
                }

                sbFileText.Append(WordTableSeparator);
            }

            sbFileText.AppendLine("Capture Table End");
        }

        #endregion

        #region 图片

        if (CaptureWordImage == "true")
        {
            sbFileText.AppendLine("Capture Image Begin");
            foreach (var pictureData in document.AllPictures)
            {
                var picExtName = pictureData.SuggestFileExtension();
                var picFileName = pictureData.FileName;
                var picFileContent = pictureData.Data;
                //
                var picTempName = string.Format(CaptureWordImageFileName,
                    new[] { Guid.NewGuid() + "_" + picFileName + "." + picExtName });
                //
                using (var fs = new FileStream(picTempName, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(picFileContent, 0, picFileContent.Length);
                    fs.Close();
                }

                //
                sbFileText.AppendLine(picTempName);
            }

            sbFileText.AppendLine("Capture Image End");
        }

        #endregion

        //正文段落
        sbFileText.AppendLine("Capture Paragraph Begin");
        foreach (var paragraph in document.Paragraphs) sbFileText.AppendLine(paragraph.ParagraphText);

        sbFileText.AppendLine("Capture Paragraph End");
        //

        //
        fileText = sbFileText.ToString();
        return fileText;
    }
}
