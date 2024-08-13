using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.KernelMemory;
using MyAiTools.AiFun.plugins.MyPlugin;
using Newtonsoft.Json;

namespace MyAiTools.AiFun.Code;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TextToImage;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using MyAiTools.AiFun.Services;
using Microsoft.SemanticKernel.Plugins.Core;
using Microsoft.SemanticKernel.Memory;
using System;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Net;

#pragma warning disable SKEXP0001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
#pragma warning disable SKEXP0050 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
public class ChatService
{
    private readonly IChatCompletionService _chatGpt;
    private readonly Kernel _kernel;
    private readonly MemoryServerless _memoryServerless;
    private readonly OpenAIPromptExecutionSettings _openAiPromptExecutionSettings;

    private readonly ILogger<ChatService> _logger;

    public ChatHistory ChatHistory;

    //现聊天记录变化时触发
    public event Action<string>? ChatHistoryChanged;

    //开始新回复时触发
    public event Action? BeginNewReply;

    public ChatService(IKernelCreat kernel, ILogger<ChatService> logger)
    {
        _kernel = kernel.KernelBuild();
        // _memoryServerless = kernel.MemoryServerlessBuild();
        //增加内置插件
        _kernel.ImportPluginFromType<TimePlugin>("Time");
        _kernel.ImportPluginFromType<ConversationSummaryPlugin>("ConversationSummary");
        _kernel.ImportPluginFromType<FileIOPlugin>("FileIO");
        _kernel.ImportPluginFromType<HttpPlugin>("Http");
        _kernel.ImportPluginFromType<MathPlugin>("Math");
        _kernel.ImportPluginFromType<TextPlugin>("Text");
        _kernel.ImportPluginFromType<WaitPlugin>("Wait");

        //增加图片生成插件
        var generateImage = MauiProgram.Services.GetService(typeof(GenerateImagePlugin));
        if (generateImage != null) _kernel.ImportPluginFromObject(generateImage, "Generate_Image");
        //增加KM插件
        //_kernel.ImportPluginFromObject(new MemoryPlugin(_memoryServerless), "kernel_memory");
        //增加本地工具插件
        var tools = MauiProgram.Services.GetService(typeof(ToolsPlugin));
        if (tools != null) _kernel.ImportPluginFromObject(tools, "Local_Tools");
        //增加搜索插件
        var searchPlugin = MauiProgram.Services.GetService(typeof(SearchPlugin));
        if (searchPlugin is SearchPlugin search) _kernel.ImportPluginFromObject(search.Bing, "Search");

        _chatGpt = _kernel.GetRequiredService<IChatCompletionService>();
        //设置调用行为，自动调用内核函数
        _openAiPromptExecutionSettings = new OpenAIPromptExecutionSettings()
            { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };


        var systemMessage =
            """
            You are a friendly assistant who likes to follow the rules.
            You will complete required steps and request approval before taking any consequential actions. 
            If the user doesn't provide enough information for you to complete a task or if you don't have enough information to complete a task, 
            you can try to get the information from search plugin,
            after that try to get the information from kernel memory plugin,
            you will keep asking questions until you have enough information to complete the task.
            """;
        ChatHistory = new ChatHistory(systemMessage);

        //添加日志
        _logger = logger;
    }

    /// <summary>
    /// 聊天功能（聊天、调用函数、生成图片、调取记忆）
    /// </summary>
    /// <param name="ask">用户提问</param>
    /// <returns></returns>
    public async Task<string?> Chat(string? ask)
    {
        try
        {
            string? result;
            if (ask != null)
            {
                ChatHistory.AddUserMessage(ask);

                ////异步调用模型
                //var assistantReply = await _chatGpt.GetChatMessageContentAsync(ChatHistory,
                //    executionSettings: _openAiPromptExecutionSettings, kernel: _kernel);

                //if (assistantReply.Content != null)
                //{
                //    BeginNewReply?.Invoke();
                //    ChatHistoryChanged?.Invoke(assistantReply.Content);
                //    ChatHistory.AddAssistantMessage(assistantReply.Content);
                //}


                //流式调用模型
                var assistantReply = _chatGpt.GetStreamingChatMessageContentsAsync(ChatHistory,
                    executionSettings: _openAiPromptExecutionSettings, kernel: _kernel);
                var fullMessage = string.Empty;
                BeginNewReply?.Invoke();
                await foreach (var message in assistantReply)
                {
                    if (message.Content == null) continue;
                    ChatHistoryChanged?.Invoke(message.Content);
                    Console.Write(message.Content);
                    if (message.Content is { Length: > 0 })
                    {
                        fullMessage += message.Content;
                    }
                }

                ChatHistory.AddAssistantMessage(fullMessage);

                result = "模型回复成功";
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
    /// 清空聊天记录
    /// </summary>
    public void ClearChatHistory()
    {
        ChatHistory.Clear();
    }

    /// <summary>
    /// 存储文件到记忆
    /// </summary>
    /// <returns></returns>
    public async Task MemorySaveFile()
    {
        try
        {
            var filePath = await FilePicker.Default.PickAsync();
            using var md5 = MD5.Create();
            if (filePath?.FileName != null)
            {
                var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(filePath?.FileName));
                var documentId = BitConverter.ToString(hash).ToLower();
                if (!await _memoryServerless.IsDocumentReadyAsync(documentId))
                {
                    await _memoryServerless.ImportDocumentAsync(filePath.FullPath, documentId);
                }
            }
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
            {
                if (index.Name == "default")
                {
                    await _memoryServerless.DeleteIndexAsync("default");
                }
            }
        }
        else
        {
            if (await _memoryServerless.IsDocumentReadyAsync(documentId))
            {
                await _memoryServerless.DeleteDocumentAsync(documentId);
            }
        }
    }
}