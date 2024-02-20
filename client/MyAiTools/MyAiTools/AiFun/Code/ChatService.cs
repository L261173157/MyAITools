namespace MyAiTools.AiFun.Code;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TextToImage;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

public class ChatService
{
    private readonly string openAiKey ;
    private IKernelBuilder builder ;
    private Kernel kernel;
    // private ITextToImageService dallE;
    private IChatCompletionService chatGPT;
    private string systemMessage;
    private ChatHistory chat;

    public ChatService()
    {
        openAiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion("gpt-3.5-turbo", openAiKey);
        kernel = builder.Build();
        // dallE = kernel.GetRequiredService<ITextToImageService>();
        chatGPT = kernel.GetRequiredService<IChatCompletionService>();
        systemMessage = "You're chatting with a user. ";
        chat = new ChatHistory(systemMessage);
    }
    
public async Task<string> Chat(string? userMessage)
    {
        chat.AddUserMessage(userMessage);
        var assistantReply = await chatGPT.GetChatMessageContentAsync(chat, new OpenAIPromptExecutionSettings());
        chat.AddAssistantMessage(assistantReply.Content);
        return assistantReply.Content;
    }
}