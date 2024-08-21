using System.ComponentModel;
using System.Text;
using CommunityToolkit.Maui.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using MyAiTools.AiFun.Services;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;

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

    [KernelFunction("provide_save_file_path")]
    [Description("provide a full folder path to save file")]
    [return: Description("the full path")]
    public async Task<string?> SaveFilePath(
    )
    {
        var result = await FolderPicker.Default.PickAsync();
        _logger.LogInformation("SaveFilePath started");
        return result.IsSuccessful ? result.Folder.Path : "Save File Failed";
    }

    [KernelFunction("save_content")]
    [Description("save the content to a json file and the file name can be auto generated")]
    [return: Description("the full path of the json file")]
    public async Task<string?> SaveChatHistory([Description("the content to save")] string chatHistory,
        [Description("the full folder folderPath to save")]
        string folderPath)
    {
        var serializer = new JsonSerializer();
        var now = DateTime.Now;
        var time = now.ToString("yyyyMMddHHmmss");
        var fileName = $"chatHistory_{time}.json";
        var path = folderPath + "\\" + fileName;
        await using var sw = new StreamWriter(path);
        await using JsonWriter writer = new JsonTextWriter(sw);
        serializer.Serialize(writer, chatHistory);

        return path;
    }
}