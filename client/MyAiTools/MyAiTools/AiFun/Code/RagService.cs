using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.KernelMemory;
using MyAiTools.AiFun.Services;

namespace MyAiTools.AiFun.Code;

#pragma warning disable SKEXP0001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
#pragma warning disable SKEXP0050 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
public class RagService
{
    private readonly ILogger<RagService> _logger;
    private readonly MemoryServerless _memoryServerless;

    public RagService(IKernelCreat kernel, ILogger<RagService> logger)
    {
        _memoryServerless = kernel.MemoryServerlessBuild();

        //添加日志
        _logger = logger;
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
                var answer = await _memoryServerless.AskAsync(ask);
                result = answer.Result;
                _logger.LogInformation("模型回复成功");
            }
            else
            {
                result = "请输入问题";
            }

            return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            _logger.LogError(ex.Message);
        }

        return null;
    }

    /// <summary>
    /// 存储文件到记忆
    /// </summary>
    /// <returns></returns>
    public async Task<string> MemorySaveFile()
    {
        try
        {
            var docId = "";
            var filePath = await FilePicker.Default.PickAsync();
            using var md5 = MD5.Create();
            if (filePath?.FileName is not null)
            {
                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(filePath?.FileName));
                //var documentId = BitConverter.ToString(hash).ToLower();
                var documentId = filePath.FileName;
                if (!await _memoryServerless.IsDocumentReadyAsync(documentId))
                    docId = await _memoryServerless.ImportDocumentAsync(filePath.FullPath, documentId);
            }

            return docId;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }


    /// <summary>
    /// 删除记忆中的文件
    /// </summary>
    /// <param name="documentId"></param>
    /// <returns></returns>
    public async Task MemoryDelFile(string? documentId)
    {
        if (documentId == null)
        {
            foreach (var index in await _memoryServerless.ListIndexesAsync())
                await _memoryServerless.DeleteIndexAsync(index.Name);
        }
        else
        {
            if (await _memoryServerless.IsDocumentReadyAsync(documentId))
                await _memoryServerless.DeleteDocumentAsync(documentId);
        }
    }
}