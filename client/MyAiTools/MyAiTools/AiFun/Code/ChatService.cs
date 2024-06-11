namespace MyAiTools.AiFun.Code;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TextToImage;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using MyAiTools.AiFun.Services;
using MyAiTools.AiFun.plugins.MyPlugin;

public class ChatService
{
    private readonly Kernel _kernel;
    // private ITextToImageService dallE;
    private IChatCompletionService chatGPT;
#pragma warning disable SKEXP0002 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
#pragma warning disable SKEXP0001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
    private ITextToImageService dallE;
#pragma warning restore SKEXP0001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
#pragma warning restore SKEXP0002 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
    private string systemMessage;
    private ChatHistory chat;

    public ChatService(IKernelCreat kernel)
    {
        _kernel = kernel.KernelBuild();
        _kernel.ImportPluginFromType<MathPlugin>();
        chatGPT = _kernel.GetRequiredService<IChatCompletionService>();
#pragma warning disable SKEXP0002 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
#pragma warning disable SKEXP0001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
        dallE = _kernel.GetRequiredService<ITextToImageService>();
#pragma warning restore SKEXP0001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
#pragma warning restore SKEXP0002 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
        systemMessage = "你是一个有用的AI助手";
        chat = new ChatHistory(systemMessage);
    }
    
public async Task<string> Chat(string? ask)
    {
        try
        {
            //判断用户输入是否为要生成图片的指令
            if (ask?.Contains("生成图片") == true)
            {
#pragma warning disable SKEXP0002 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
#pragma warning disable SKEXP0001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
                var imageurl = await dallE.GenerateImageAsync(ask, 512, 512);
#pragma warning restore SKEXP0001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
#pragma warning restore SKEXP0002 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
                return imageurl;
            }
            
            chat.AddUserMessage(ask);
            var assistantReply = await chatGPT.GetChatMessageContentAsync(chat, new OpenAIPromptExecutionSettings(){ ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions });
            chat.AddAssistantMessage(assistantReply.Content);
            return assistantReply.Content;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            return "对不起,程序报错"+ex.Message;
        }
        
    }
}