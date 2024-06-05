using System;
using Microsoft.SemanticKernel;
using MyAiTools.AiFun.Services;
namespace MyAiTools.AiFun.Code;

public class TranslateService
{
    private readonly Kernel kernel;
    private readonly KernelPlugin pluginFunctions ;
    public TranslateService()
    {
        var handler = new OpenAIHttpClientHandler();
        var openAiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion(modelId:"gemini-pro", apiKey:openAiKey,httpClient: new HttpClient(handler));
        kernel = builder.Build();
        var pluginDirectoryPath = Path.Combine(AppContext.BaseDirectory, "AiFun", "plugins", "TranslatePlugin");
        pluginFunctions = kernel.ImportPluginFromPromptDirectory(pluginDirectoryPath);
    }
    
    public async Task<string> TranslateText(string text, string target)
    {
        var arguments = new KernelArguments() { ["input"] = text, ["target"] = target };
        object? result;
        try
        {
            result = await kernel.InvokeAsync(pluginFunctions["Translate"], arguments);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return e.Message;
        }
        return result.ToString();
    }
    
    
}