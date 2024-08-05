using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
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


#pragma warning disable SKEXP0001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
#pragma warning disable SKEXP0050 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
public class ChatService
{
    private readonly IChatCompletionService _chatGpt;
    private readonly Kernel _kernel;
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
        //增加插件功能
        _kernel.ImportPluginFromType<TimePlugin>("Time");
        //增加图片生成插件
        var generateImage = MauiProgram.Services.GetService(typeof(GenerateImagePlugin));
        if (generateImage != null) _kernel.ImportPluginFromObject(generateImage, "GenerateImage");
        //增加RAG插件
        //var rag = MauiProgram.Services.GetService(typeof(RagPlugin));
        //if (rag != null) _kernel.ImportPluginFromObject(rag, "RagPlugin");

        _chatGpt = _kernel.GetRequiredService<IChatCompletionService>();
        //设置调用行为，自动调用内核函数
        _openAiPromptExecutionSettings = new OpenAIPromptExecutionSettings()
            { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };


        var systemMessage =
            """
            You are a friendly assistant who likes to follow the rules.
            You will complete required steps and request approval before taking any consequential actions. 
            If the user doesn't provide enough information for you to complete a task,
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

}