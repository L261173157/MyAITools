namespace MyAiTools.AiFun.Code;

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TextToImage;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using MyAiTools.AiFun.Services;

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
        var handler = new OpenAIHttpClientHandler();
        openAiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion(modelId:"gemini-pro", apiKey:openAiKey,httpClient: new HttpClient(handler));
        kernel = builder.Build();
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