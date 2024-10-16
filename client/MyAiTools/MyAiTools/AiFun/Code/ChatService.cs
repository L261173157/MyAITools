using System.Security.Cryptography;
using System.Text;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.Core;
using MyAiTools.AiFun.Model;
using MyAiTools.AiFun.plugins.MyPlugin;
using MyAiTools.AiFun.Services;
using Newtonsoft.Json;
using UglyToad.PdfPig.Logging;

namespace MyAiTools.AiFun.Code;

#pragma warning disable SKEXP0001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
#pragma warning disable SKEXP0050 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
public class ChatService
{
    private readonly IChatCompletionService _chatGpt;
    private readonly Kernel _kernel;

    private readonly ILogger<ChatService> _logger;
    private readonly MemoryServerless _memoryServerless;
    private readonly OpenAIPromptExecutionSettings _openAiPromptExecutionSettings;

    private readonly KernelPlugin _conversationSummaryPlugin;

    private const string SystemMessage =
        """
        You are a friendly assistant who likes to follow the rules.
        You will complete required steps and request approval before taking any consequential actions. 
        If the user doesn't provide enough information for you to complete a task or if you don't have enough information to complete a task, 
        you can try to get the information from search plugin, after that try to get the information from kernel memory plugin, 
        if you call some plugins,you can tell me which plugin you have called.
        for example: \"I have called the plugin named 'plugin name'\". 
        you will keep asking questions until you have enough information to complete the task.
        """;


    public DialogGroupModel DialogGroup;

    //回复时触发
    public event Action? BeginNewReply;

    public ChatService(IKernelCreat kernel, ILogger<ChatService> logger)
    {
        _kernel = kernel.KernelBuild();
        _memoryServerless = kernel.MemoryServerlessBuild();
        //增加内置插件
        _kernel.ImportPluginFromType<TimePlugin>("Time");
        //_kernel.ImportPluginFromType<ConversationSummaryPlugin>("ConversationSummaryPlugin");
        _kernel.ImportPluginFromType<FileIOPlugin>("FileIO");
        //_kernel.ImportPluginFromType<HttpPlugin>("Http");
        _kernel.ImportPluginFromType<MathPlugin>("Math");
        //_kernel.ImportPluginFromType<TextPlugin>("Text");
        //_kernel.ImportPluginFromType<WaitPlugin>("Wait");

        _conversationSummaryPlugin= _kernel.ImportPluginFromType<ConversationSummaryPlugin>();

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
        _openAiPromptExecutionSettings = new OpenAIPromptExecutionSettings
            { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };

        DialogGroup = new DialogGroupModel(SystemMessage);

        //添加日志
        _logger = logger;
    }


    /// <summary>
    /// 聊天功能（聊天、调用函数、生成图片、调取记忆）
    /// </summary>
    /// <param name="ask">用户提问</param>
    /// <param name="currentId"></param>
    /// <returns></returns>
    public async Task Chat(string? ask, int currentId)
    {
        try
        {
            if (ask != null)
            {
                foreach (var dialog in DialogGroup.Dialogs.Where(dialog => dialog.Id == currentId))
                {
                    dialog.AddMessage(content: ask, role: ChatRole.User);
                    dialog.AddChatHistory(content: ask, role: ChatRole.User);
                    dialog.Title = await Summary(dialog.Messages.First().Content);
                    BeginNewReply?.Invoke();

                    #region 异步调用模型

                    //var assistantReply = await _chatGpt.GetChatMessageContentAsync(dialog.ChatHistory,
                    //    _openAiPromptExecutionSettings, _kernel);

                    //if (assistantReply.Content != null)
                    //{
                    //    dialog.AddMessage(content: assistantReply.Content, role: ChatRole.Assistant);
                    //    BeginNewReply?.Invoke();
                    //    dialog.AddChatHistory(assistantReply.Content, ChatRole.Assistant);
                    //}

                    #endregion


                    #region 流式调用模型

                    var assistantReply = _chatGpt.GetStreamingChatMessageContentsAsync(dialog.ChatHistory,
                        _openAiPromptExecutionSettings, _kernel);
                    var fullMessage = string.Empty;
                    dialog.AddMessage(content: fullMessage, role: ChatRole.Assistant);
                    await foreach (var message in assistantReply)
                    {
                        if (message.Content == null) continue;
                        if (message.Content is { Length: > 0 }) fullMessage += message.Content;
                        dialog.Messages[^1].Content = fullMessage;
                        BeginNewReply?.Invoke();
                    }

                    dialog.AddChatHistory(content: fullMessage, role: ChatRole.Assistant);

                    #endregion
                }
            }
            else
            {
                var result = "请输入问题";
                foreach (var dialog in DialogGroup.Dialogs.Where(dialog => dialog.Id == currentId))
                {
                    dialog.AddMessage(content: result, role: ChatRole.Assistant);
                    BeginNewReply?.Invoke();
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"An error occurred: {e.Message}");
            Console.WriteLine(e.StackTrace);
            _logger.LogError(e.Message);
            foreach (var dialog in DialogGroup.Dialogs.Where(dialog => dialog.Id == currentId))
            {
                dialog.AddMessage(content: e.Message, role: ChatRole.Assistant);
                BeginNewReply?.Invoke();
            }
        }
    }

    /// <summary>
    /// 清空聊天记录
    /// </summary>
    public void ClearChatHistory(int currentId)
    {
        foreach (var dialog in DialogGroup.Dialogs.Where(dialog => dialog.Id == currentId))
        {
            dialog.Clear();
            BeginNewReply?.Invoke();
        }
    }

    /// <summary>
    /// 添加对话
    /// </summary>
    public void AddDialog()
    {
        DialogGroup.AddDialog(SystemMessage);
    }

    public async Task<string?> Summary(string? message)
    {
        //try
        //{
        //    FunctionResult topics = await _kernel.InvokeAsync(
        //        _conversationSummaryPlugin["GetConversationTopics"], new() { ["input"] = message });
        //    string? temp = topics.GetValue<string>();
        //    //转换为c#类
        //    if (temp != null)
        //    {
        //        var resultJson = JsonConvert.DeserializeObject<Topics>(temp);
        //        var result = resultJson?.topics.First();
        //        return result ?? "没有找到相关信息";
        //    }
        //}
        //catch (Exception e)
        //{
        //    Console.WriteLine(e);
        //    throw;
        //}

        //return "对话";

        var result = message?.Substring(0, Math.Min(message.Length, 5));
        return result;
    }
}

internal class Topics
{
    public List<string> topics { get; set; }
}