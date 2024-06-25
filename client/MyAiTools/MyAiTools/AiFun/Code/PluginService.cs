using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using MyAiTools.AiFun.plugins.MyPlugin;
using MyAiTools.AiFun.Services;
using Microsoft.SemanticKernel.Plugins.Core;
using System.Linq;


namespace MyAiTools.AiFun.Code;
#pragma warning disable SKEXP0001
//功能已作废
public class PluginService
{
    private readonly Kernel _kernel;

    private readonly ISemanticTextMemory _memory;

    private readonly KernelPlugin _pluginFunctions;

    public PluginService(IKernelCreat kernel)
    {
        _kernel = kernel.KernelBuild();

        _memory = kernel.MemoryBuild();

        //_pluginFunctions = _kernel.ImportPluginFromType<Microsoft.SemanticKernel.Plugins.Core.MathPlugin>();
        var pluginDirectoryPath =
            Path.Combine(AppContext.BaseDirectory, "AiFun", "plugins", "OfficePlugin", "WriterPlugin");
        _pluginFunctions = _kernel.ImportPluginFromPromptDirectory(pluginDirectoryPath);


        //添加本地函数功能
        //mathPlugin = _kernel.ImportPluginFromType<MathPlugin>();
    }

    public async Task<string> TranslateText(string text, string target)
    {
        var arguments = new KernelArguments() { ["input"] = text, ["target"] = target };
        object? result;
        try
        {
            //result = await _kernel.InvokeAsync(pluginFunctions["WriteFile"], arguments);
            result = await _kernel.InvokeAsync(_pluginFunctions["Add"], new() { { "value", text }, { "amount", 2 } });
            //result= await _kernel.InvokeAsync(mathPlugin["Add"], new() { { "number1", 12 }, { "number2", 13 } });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return e.Message;
        }

        return result.ToString();
    }

    //读取当前插件的所有功能和参数
    public List<string> GetPluginFunctions()
    {
        var result = new List<string>();
        foreach (var function in _pluginFunctions)
        {
            foreach (var parameter in function.Metadata.Parameters)
            {
                result.Add($"{function.Name}({parameter.Name})");
            }
        }

        return result;
    }

    //记忆功能
    public async Task<string> Remember(string memory, string query)
    {
        try
        {
            const string memoryCollectionName = "MyMemory";
            await _memory.SaveInformationAsync(memoryCollectionName, id: "info1", text: memory);
            var response = await _memory.SearchAsync(memoryCollectionName, query).FirstOrDefaultAsync();
            if (response == null)
            {
                return "没有找到相关信息";
            }

            var result = response?.Relevance.ToString() + "\n" + response?.Metadata.Text;
            Console.WriteLine(result);
            return result;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}