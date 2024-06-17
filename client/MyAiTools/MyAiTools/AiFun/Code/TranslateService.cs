using System;
using Microsoft.SemanticKernel;
using MyAiTools.AiFun.plugins.MyPlugin;
using MyAiTools.AiFun.Services;
using Microsoft.SemanticKernel.Plugins.Core;


namespace MyAiTools.AiFun.Code;

public class TranslateService
{
    private readonly Kernel _kernel;
    private readonly KernelPlugin pluginFunctions ;
    private readonly KernelPlugin mathPlugin;

    public TranslateService(IKernelCreat kernel)
    {
        _kernel = kernel.KernelBuild();
        //var pluginDirectoryPath = Path.Combine(AppContext.BaseDirectory,"AiFun", "plugins", "MyPlugin", "TranslatePlugin");
        //pluginFunctions = _kernel.ImportPluginFromPromptDirectory(pluginDirectoryPath);
        var pluginDirectoryPath = Path.Combine(AppContext.BaseDirectory, "AiFun", "plugins", "OfficePlugin", "WriterPlugin");
        //pluginFunctions = _kernel.ImportPluginFromPromptDirectory(pluginDirectoryPath);
#pragma warning disable SKEXP0050
        pluginFunctions = _kernel.ImportPluginFromType<Microsoft.SemanticKernel.Plugins.Core.MathPlugin>();
#pragma warning restore SKEXP0050


        //添加本地函数功能
        //mathPlugin = _kernel.ImportPluginFromType<MathPlugin>();
    }
    
    public async Task<string> TranslateText(string text, string target)
    {
        var arguments = new KernelArguments() { ["input"] = text, ["target"] = target };
        object? result;
        try
        {
            //result = await _kernel.InvokeAsync(pluginFunctions["Translate"], arguments);
            result= await _kernel.InvokeAsync(pluginFunctions["Add"], new() { { "value", text },{ "amount",2 } });
            //result= await _kernel.InvokeAsync(mathPlugin["Add"], new() { { "number1", 12 }, { "number2", 13 } });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return e.Message;
        }
        return result.ToString();
    }
    
    
}