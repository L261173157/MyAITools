using System;
using Microsoft.SemanticKernel;
namespace MyAiTools.AiFun.Code;

public class TranslateService
{
    private readonly string _openAiKey ;
    private IKernelBuilder _builder ;
    private Kernel _kernel;
    private string _pluginDirectoryPath ;
    private KernelPlugin PluginFunctions ;
    public TranslateService()
    {
        _openAiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        _builder = Kernel.CreateBuilder();
        _builder.AddOpenAIChatCompletion("gpt-3.5-turbo", _openAiKey);
        _kernel = _builder.Build();
        _pluginDirectoryPath = Path.Combine(AppContext.BaseDirectory, "AiFun", "plugins", "TranslatePlugin");
        PluginFunctions = _kernel.ImportPluginFromPromptDirectory(_pluginDirectoryPath);
    }
    
    public async Task<string> TranslateText(string text, string target)
    {
        var arguments = new KernelArguments() { ["input"] = text, ["target"] = target };
        object result = null;
        try
        {
            result = await _kernel.InvokeAsync(PluginFunctions["Translate"], arguments);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return e.Message;
        }
        return result.ToString();
    }
    
    
}