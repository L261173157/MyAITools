using System.ComponentModel;
using System.Text;
using CommunityToolkit.Maui.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using MyAiTools.AiFun.Services;

namespace MyAiTools.AiFun.plugins.MyPlugin;

public class ToolsPlugin
{
    private readonly ILogger<ToolsPlugin> _logger;

    public ToolsPlugin(ILogger<ToolsPlugin> logger, IKernelCreat kernel)
    {
        _logger = logger;
    }

    [KernelFunction("select_local_file")]
    [Description("select a local file,return full path")]
    [return: Description("the file full path")]
    public async Task<string?> SelectFile(
    )
    {
        var result = await FilePicker.Default.PickAsync();
        _logger.LogInformation("SelectFile started");
        return result?.FullPath;
    }

    [KernelFunction("read_file_content")]
    [Description("read a txt/docx/pdf file,return the full content")]
    [return: Description("the full content")]
    public async Task<string?> ReadFile([Description("the file full path")] string? filePath
    )
    {
        try
        {
            if (filePath != null)
            {
                await using var stream = File.OpenRead(filePath);
                if (filePath.EndsWith("txt", StringComparison.OrdinalIgnoreCase))
                {
                    var reader = new StreamReader(stream);
                    _logger.LogInformation("ReadTxtFile started");
                    return await reader.ReadToEndAsync();
                }

                if (filePath.EndsWith("docx", StringComparison.OrdinalIgnoreCase))
                {
                    var result = await Task.Run(() =>
                    {
                        var reader = NopiHandler.ReadWordText(filePath);
                        return reader;
                    });
                    _logger.LogInformation("ReadDocxFile started");
                    return result;
                }

                if (filePath.EndsWith("pdf", StringComparison.OrdinalIgnoreCase))
                {
                    var result = await Task.Run(() =>
                    {
                        var reader = PdfHelper.ExtractText(filePath);
                        var result = new StringBuilder();
                        foreach (var item in reader) result.Append(item);

                        _logger.LogInformation("ReadPdfFile started");
                        return result.ToString();
                    });
                    return result;
                }

                return "can not read the file";
            }

            return "the path of selected file is wrong";
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    //[KernelFunction("export_file")]
    //[Description("Export the content to a specified directory and specified format(default is txt) file")]
    //[return: Description("export is successful or not")]
    //public async Task<string?> ExportFile([Description("the content")] string content
    //)
    //{
    //    //获取当前年月日时分秒
    //    var now = DateTime.Now;
    //    var time = now.ToString("yyyyMMddHHmmss");
    //    var fileName = $"export_{time}.txt";
    //    using var stream = new MemoryStream(Encoding.Default.GetBytes(content));
    //    var fileSaverResult = await FileSaver.Default.SaveAsync(fileName, stream);
    //    return fileSaverResult.IsSuccessful ? "ExportFileSuccessful" : "ExportFileFailed";
    //}

    [KernelFunction("save_file_path")]
    [Description("provide a full path to save file,include folder and file's name")]
    [return: Description("the full path")]
    public async Task<string?> SaveFilePath(
    )
    {
        //获取当前年月日时分秒
        var now = DateTime.Now;
        var time = now.ToString("yyyyMMddHHmmss");
        var fileName = $"export_{time}.txt";
        var result = await FolderPicker.Default.PickAsync();
        _logger.LogInformation("SaveFilePath started");
        return result.IsSuccessful ? result.Folder.Path + "\\" + fileName : "Save File Failed";
    }
}