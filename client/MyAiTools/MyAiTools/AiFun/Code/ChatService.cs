namespace MyAiTools.AiFun.Code;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TextToImage;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using MyAiTools.AiFun.Services;
using MyAiTools.AiFun.plugins.MyPlugin;

#pragma warning disable SKEXP0002 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
#pragma warning disable SKEXP0001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
public class ChatService
{
    private readonly Kernel _kernel;

    // private ITextToImageService dallE;
    private IChatCompletionService chatGPT;

    private ITextToImageService dallE;

    private string systemMessage;
    public ChatHistory chatHistory;

    public ChatService(IKernelCreat kernel)
    {
        _kernel = kernel.KernelBuild();
        chatGPT = _kernel.GetRequiredService<IChatCompletionService>();

        dallE = _kernel.GetRequiredService<ITextToImageService>();

        systemMessage = "你是一个有用的AI助手，尽量用中文回答";
        chatHistory = new ChatHistory(systemMessage);
    }

    public async Task Chat(string? ask)
    {
        try
        {
            if (ask != null)
            {
                chatHistory.AddUserMessage(ask);
                if (ask?.Contains("生成图片") == true)
                {
                    var imageurl = await dallE.GenerateImageAsync(ask, 512, 512);
                    chatHistory.AddAssistantMessage(imageurl);
                }
                else
                {
                    var assistantReply = await chatGPT.GetChatMessageContentAsync(chatHistory,
                        new OpenAIPromptExecutionSettings()
                            { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions });
                    chatHistory.AddAssistantMessage(assistantReply.Content);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }

//chatHistory的清空方法
    public void ClearChatHistory()
    {
        chatHistory.Clear();
    }
}
#pragma warning restore SKEXP0001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
#pragma warning restore SKEXP0002 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。