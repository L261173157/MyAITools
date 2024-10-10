using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.AI.Ollama;
using Microsoft.KernelMemory.AI.OpenAI;
using Microsoft.KernelMemory.DocumentStorage.DevTools;
using Microsoft.KernelMemory.FileSystem.DevTools;
using Microsoft.KernelMemory.MemoryStorage.DevTools;
using MyAiTools.AiFun.Services;

namespace MyAiTools.AiFun.Code;

#pragma warning disable SKEXP0001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
#pragma warning disable SKEXP0050 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
public class RagService
{
    private readonly ILogger<RagService> _logger;

    //建立MemoryServerless的服务
    private readonly MemoryServerless _memoryServerless;

    //建立链接Ollama的服务
    private readonly IKernelMemory _localMemory;

    private readonly IKernelMemory _kernelMemory;
    private const string MainDir = "D:/Memory";

    public RagService(IKernelCreat kernel, ILogger<RagService> logger)
    {
        //添加日志
        _logger = logger;
        //建立memory serverless服务
        _memoryServerless = kernel.MemoryServerlessBuild();
        //建立链接Ollama的服务
        var config = new OllamaConfig
        {
            Endpoint = "http://localhost:11434",
            TextModel = new OllamaModelConfig("phi3:medium-128k", 131072),
            EmbeddingModel = new OllamaModelConfig("nomic-embed-text", 2048)
        };
        _localMemory = new KernelMemoryBuilder()
            .WithOllamaTextGeneration(config, new GPT4oTokenizer())
            .WithOllamaTextEmbeddingGeneration(config, new GPT4oTokenizer()).WithSimpleVectorDb(new SimpleVectorDbConfig
                { Directory = MainDir, StorageType = FileSystemTypes.Disk })
            .WithSimpleFileStorage(new SimpleFileStorageConfig
                { Directory = MainDir, StorageType = FileSystemTypes.Disk }).Build();

        _kernelMemory = _memoryServerless as IKernelMemory;
        //_kernelMemory = _localMemory;
    }


    /// <summary>
    /// 聊天功能（聊天、调用函数、生成图片、调取记忆）
    /// </summary>
    /// <param name="ask">用户提问</param>
    /// <returns></returns>
    public async Task<string?> AskAsync(string? ask)
    {
        try
        {
            string? result;
            if (ask != null)
            {
                var answer = await _kernelMemory.AskAsync(ask);
                result = answer.Result;
                _logger.LogInformation("模型回复成功");
            }
            else
            {
                result = "请输入问题";
            }

            return result;
        }
        catch (Exception e)
        {
            Console.WriteLine($"An error occurred: {e.Message}");
            Console.WriteLine(e.StackTrace);
            _logger.LogError(e.Message);
            return "报错信息:"+ e.Message;
        }
    }

    /// <summary>
    /// 存储文件到记忆
    /// </summary>
    /// <returns></returns>
    public async Task<string> MemorySaveFile()
    {
        try
        {
            var docId = "未存储";
            var filePath = await FilePicker.Default.PickAsync();
            using var md5 = MD5.Create();
            if (filePath?.FileName is not null)
            {
                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(filePath?.FileName));
                //var documentId = BitConverter.ToString(hash).ToLower();
                var filename = filePath.FileName;
                var documentId = FilterAllowedCharacters(filename);
                if (!await _kernelMemory.IsDocumentReadyAsync(documentId))
                    docId = await _kernelMemory.ImportDocumentAsync(filePath.FullPath, documentId);
            }

            return "文件ID:"+ docId;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return "错误信息:"+ e.Message;
        }
    }


    /// <summary>
    /// 删除记忆中的文件
    /// </summary>
    /// <param name="documentId"></param>
    /// <returns></returns>
    public async Task<string> MemoryDelFile(string? documentId)
    {
        if (documentId == null)
        {
            foreach (var index in await _kernelMemory.ListIndexesAsync())
                await _kernelMemory.DeleteIndexAsync(index.Name);
        }
        else
        {
            if (await _kernelMemory.IsDocumentReadyAsync(documentId))
                await _kernelMemory.DeleteDocumentAsync(documentId);
        }
        return "删除成功";
    }

    static string FilterAllowedCharacters(string input)
    {
        // 使用正则表达式替换所有不允许的字符
        return Regex.Replace(input, @"[^a-zA-Z0-9._-]", string.Empty);
    }
}