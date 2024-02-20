using System;
using Microsoft.SemanticKernel;
namespace MyAiTools.AiFun.Code;

public class TranslateService
{
    private readonly Kernel kernel;
    private readonly KernelPlugin pluginFunctions ;
    public TranslateService()
    {
        var openAiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        var builder = Kernel.CreateBuilder();
        builder.AddOpenAIChatCompletion("gpt-3.5-turbo", openAiKey);
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