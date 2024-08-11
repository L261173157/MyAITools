using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.XWPF.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPOI.POIFS.FileSystem;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.XWPF.UserModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;

namespace MyAiTools.AiFun.plugins.MyPlugin
{
    public static class NopiHandler
    {
        /// <summary>
        /// 读取Word内容
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string ReadWordText(string fileName)
        {
            string WordTableCellSeparator = ConfigurationManager.AppSettings["WordTableCellSeparator"];
            string WordTableRowSeparator = ConfigurationManager.AppSettings["WordTableRowSeparator"];
            string WordTableSeparator = ConfigurationManager.AppSettings["WordTableSeparator"];
            //
            string CaptureWordHeader = ConfigurationManager.AppSettings["CaptureWordHeader"];
            string CaptureWordFooter = ConfigurationManager.AppSettings["CaptureWordFooter"];
            string CaptureWordTable = ConfigurationManager.AppSettings["CaptureWordTable"];
            string CaptureWordImage = ConfigurationManager.AppSettings["CaptureWordImage"];
            //
            string CaptureWordImageFileName = ConfigurationManager.AppSettings["CaptureWordImageFileName"];
            //
            string fileText = string.Empty;
            StringBuilder sbFileText = new StringBuilder();

            #region 打开文档
            XWPFDocument document = null;
            try
            {
                using (FileStream file = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    document = new XWPFDocument(file);
                }
            }
            catch (Exception e)
            {
               Console.Write(string.Format("文件{0}打开失败，错误：{1}", new string[] { fileName, e.ToString() }));
            }
            #endregion

            #region 页眉、页脚
            //页眉
            if (CaptureWordHeader == "true")
            {
                sbFileText.AppendLine("Capture Header Begin");
                foreach (XWPFHeader xwpfHeader in document.HeaderList)
                {
                    sbFileText.AppendLine(string.Format("{0}", new string[] { xwpfHeader.Text }));
                }
                sbFileText.AppendLine("Capture Header End");
            }
            //页脚
            if (CaptureWordFooter == "true")
            {
                sbFileText.AppendLine("Capture Footer Begin");
                foreach (XWPFFooter xwpfFooter in document.FooterList)
                {
                    sbFileText.AppendLine(string.Format("{0}", new string[] { xwpfFooter.Text }));
                }
                sbFileText.AppendLine("Capture Footer End");
            }
            #endregion

            #region 表格
            if (CaptureWordTable == "true")
            {
                sbFileText.AppendLine("Capture Table Begin");
                foreach (XWPFTable table in document.Tables)
                {
                    //循环表格行
                    foreach (XWPFTableRow row in table.Rows)
                    {
                        foreach (XWPFTableCell cell in row.GetTableCells())
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
                foreach (XWPFPictureData pictureData in document.AllPictures)
                {
                    string picExtName = pictureData.SuggestFileExtension();
                    string picFileName = pictureData.FileName;
                    byte[] picFileContent = pictureData.Data;
                    //
                    string picTempName = string.Format(CaptureWordImageFileName, new string[] { Guid.NewGuid().ToString() + "_" + picFileName + "." + picExtName });
                    //
                    using (FileStream fs = new FileStream(picTempName, FileMode.Create, FileAccess.Write))
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
            foreach (XWPFParagraph paragraph in document.Paragraphs)
            {
                sbFileText.AppendLine(paragraph.ParagraphText);

            }
            sbFileText.AppendLine("Capture Paragraph End");
            //

            //
            fileText = sbFileText.ToString();
            return fileText;
        }
    }
}
