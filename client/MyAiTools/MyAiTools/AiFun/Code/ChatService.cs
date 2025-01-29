using Microsoft.Extensions.Logging;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Plugins.Core;
using MyAiTools.AiFun.Data;
using MyAiTools.AiFun.Model;
using MyAiTools.AiFun.plugins.MyPlugin;
using MyAiTools.AiFun.Services;

namespace MyAiTools.AiFun.Code;

public enum ChatMethod
{
    Streaming,
    Async
}

#pragma warning disable SKEXP0001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
#pragma warning disable SKEXP0050 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
public class ChatService
{
    private readonly IChatCompletionService _chatGpt;
    private readonly Kernel _kernel;

    private readonly ILogger<ChatService> _logger;
    private readonly MemoryServerless _memoryServerless;
    private readonly OpenAIPromptExecutionSettings _openAiPromptExecutionSettings;

    private DataBase _dataBase;

    private const string? SystemMessage =
        """
        You are a friendly assistant who likes to follow the rules.
        You will complete required steps and request approval before taking any consequential actions. 
        If the user doesn't provide enough information for you to complete a task or if you don't have enough information to complete a task, 
        you can try to get the information from search plugin, after that try to get the information from kernel memory plugin, 
        if you call some plugins,you can tell me which plugin you have called.
        for example: \"I have called the plugin named 'plugin name'\". 
        you will keep asking questions until you have enough information to complete the task.
        """;

    /// <summary>
    /// 对话组
    /// </summary>
    public readonly DialogGroup DialogGroup;

    /// <summary>
    /// 刷新消息
    /// </summary>
    public event Action? RefreshMessage;

    //聊天返回方式
    public ChatMethod ChatMethod { get; set; }

    public ChatService(IKernelCreat kernel, ILogger<ChatService> logger, DataBase dataBase)
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

        ChatMethod = ChatMethod.Streaming;
        //添加日志
        _logger = logger;
        //初始化对话数据库
        _dataBase = dataBase;
        //初始化对话组
        DialogGroup = new DialogGroup(_dataBase);
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
                var dialog = (DialogGroup.Dialogs ?? throw new InvalidOperationException()).First(d => d.Id == currentId);

                dialog.AddMessage(content: ask, role: ChatRole.User);
                dialog.AddChatHistory(content: ask, role: ChatRole.User);
                RefreshMessage?.Invoke();
                if (string.IsNullOrWhiteSpace(dialog.Title))
                {
                    var title = await Summary(dialog.Messages.First().Content);
                    await dialog.UpdateTitle(title);
                }

                RefreshMessage?.Invoke();
                switch (ChatMethod)
                {
                    case ChatMethod.Streaming:

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
                            RefreshMessage?.Invoke();
                        }

                        dialog.AddChatHistory(content: fullMessage, role: ChatRole.Assistant);

                        #endregion

                        break;
                    case ChatMethod.Async:

                        #region 异步调用模型

                        var chatMessageContentAsync = await _chatGpt.GetChatMessageContentAsync(dialog.ChatHistory,
                            _openAiPromptExecutionSettings, _kernel);

                        if (chatMessageContentAsync.Content != null)
                        {
                            dialog.AddMessage(content: chatMessageContentAsync.Content, role: ChatRole.Assistant);
                            RefreshMessage?.Invoke();
                            dialog.AddChatHistory(chatMessageContentAsync.Content, ChatRole.Assistant);
                        }

                        #endregion

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                var result = "请输入问题";
                foreach (var dialog in DialogGroup.Dialogs.Where(dialog => dialog.Id == currentId))
                {
                    dialog.AddMessage(content: result, role: ChatRole.Assistant);
                    RefreshMessage?.Invoke();
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
                RefreshMessage?.Invoke();
            }
        }
    }

    /// <summary>
    /// 清空聊天记录
    /// </summary>
    public async Task ClearChatHistory(int currentId)
    {
        await (DialogGroup.Dialogs ?? throw new InvalidOperationException()).First(d => d.Id == currentId).Clear();
        RefreshMessage?.Invoke();
    }

    /// <summary>
    /// 清空所有对话
    /// </summary>
    public async Task ClearAllDialog()
    {
        await DialogGroup.Clear();
        RefreshMessage?.Invoke();
    }

    /// <summary>
    /// 添加对话
    /// </summary>
    public async Task AddDialog()
    {
        await DialogGroup.AddDialog(SystemMessage);
        RefreshMessage?.Invoke();
    }

    /// <summary>
    /// 总结内容
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private async Task<string?> Summary(string? input)
    {
        try
        {
            var pluginDirectoryPath =
                Path.Combine(AppContext.BaseDirectory, "AiFun", "plugins", "OfficePlugin", "SummarizePlugin");
            var summaryFunction = _kernel.CreatePluginFromPromptDirectory(pluginDirectoryPath);
            var arguments = new KernelArguments() { ["input"] = input };
            var summaryResult = await _kernel.InvokeAsync(summaryFunction["Summarize"], arguments);
            return summaryResult.GetValue<string>();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    //初始化对话组
    public async Task InitDialogs()
    {
        await DialogGroup.InitDialogs();
        RefreshMessage?.Invoke();
    }

    //获取当前对话
    public Dialog? GetCurrentDialog(int currentId)
    {
        var dialog = (DialogGroup.Dialogs ?? throw new InvalidOperationException()).FirstOrDefault(d => d.Id == currentId);
        return dialog;
    }

    //获取所有对话
    public List<Dialog>? GetDialogs()
    {
        var dialogs = DialogGroup.Dialogs;
        return dialogs;
    }
}