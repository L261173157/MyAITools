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
    private readonly ITextToImageService _dallE;
    private readonly OpenAIPromptExecutionSettings _openAiPromptExecutionSettings;
    private readonly ISemanticTextMemory _memory;
    private const string MemoryCollectionName = "Knowledge";

    private readonly ILogger<ChatService> _logger;

    public ChatHistory ChatHistory;

    //现聊天记录变化时触发
    public event Action<string>? ChatHistoryChanged;

    //开始新回复时触发
    public event Action? BeginNewReply;

    public ChatService(IKernelCreat kernel, ILogger<ChatService> logger)
    {
        _memory = kernel.MemoryBuild();
        _kernel = kernel.KernelBuild();
        //增加插件功能
        _kernel.ImportPluginFromType<TimePlugin>();
        //_kernel.ImportPluginFromType<TestPlugin>();
        _chatGpt = _kernel.GetRequiredService<IChatCompletionService>();
        //设置调用行为，自动调用内核函数
        _openAiPromptExecutionSettings = new OpenAIPromptExecutionSettings()
        { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions };
        _dallE = _kernel.GetRequiredService<ITextToImageService>();

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
                //if (ask?.Contains("生成图片") == true)
                //{
                //    ChatHistory.AddUserMessage(ask);

                //    var imageUrl = await _dallE.GenerateImageAsync(ask, 512, 512);
                //    ChatHistory.Add(new()
                //    {
                //        Role = AuthorRole.Assistant,
                //        Items = [new ImageContent{Uri = new Uri(imageUrl) }]
                //    } );
                //    ChatHistoryChanged(imageUrl);
                //    result = "图片地址:" + imageUrl;
                //    _logger.LogInformation("生成图片成功");
                //}
                //else
                //{
                //搜索记忆
                //var memory = await _memory.SearchAsync(MemoryCollectionName, ask).FirstOrDefaultAsync();
                //if (memory != null)
                //{
                //    ChatHistory.AddUserMessage(memory.Metadata.Text);
                //    result = "搜索到记忆" + memory.Relevance.ToString();
                //    _logger.LogInformation("搜索到记忆");
                //}

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
                    executionSettings: _openAiPromptExecutionSettings,kernel:_kernel);
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

    //加载记忆，加载jsonl文件，格式与Openai微调的格式一致
    public async Task<string?> LoadMemory()
    {
        var filePath = await FilePicker.Default.PickAsync();
        var knowledges = new Knowledges();
        if (filePath != null)
        {
            if (filePath.FileName.EndsWith("jsonl", StringComparison.OrdinalIgnoreCase))
            {
                await using var stream = await filePath.OpenReadAsync();

                //读取jsonl文件
                using (var reader = new StreamReader(stream))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        // 解析每一行 JSON 数据
                        var knowledge = JsonConvert.DeserializeObject<KnowledgeMessages>(line);
                        knowledges.KnowledgesList.Add(knowledge);
                    }
                }
            }
        }


        for (int i = 0; i < knowledges.KnowledgesList.Count; i++)
        {
            var id = knowledges.KnowledgesList[i].KnowledgeItems[0].Content + "id" + i.ToString();
            var text = knowledges.KnowledgesList[i].KnowledgeItems[1].Content +
                       knowledges.KnowledgesList[i].KnowledgeItems[2].Content;
            await _memory.SaveInformationAsync(MemoryCollectionName, id: id, text: text);
        }

        _logger.LogInformation("加载记忆成功");
        return "加载记忆成功";
    }
}

// 记忆格式采用json格式，每行一个json对象，与Openai微调的格式一致
public class KnowledgeItem
{
    [JsonProperty("role")] public string Role { get; set; }
    [JsonProperty("content")] public string Content { get; set; }
}

public class KnowledgeMessages
{
    [JsonProperty("messages")] public List<KnowledgeItem> KnowledgeItems { get; set; }
}

public class Knowledges
{
    public Knowledges()
    {
        KnowledgesList = new List<KnowledgeMessages>();
    }

    public List<KnowledgeMessages> KnowledgesList { get; set; }
}