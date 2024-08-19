using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;

namespace MyAiTools.AiFun.plugins.MyPlugin;

/// <summary>
///     本地函数功能
/// </summary>
public class TestPlugin
{
    private readonly ILogger<TestPlugin> _logger;

    public TestPlugin(ILogger<TestPlugin> logger)
    {
        _logger = logger;
    }

    [KernelFunction("write_file")]
    [Description("write a new .txt file on the path")]
    [return: Description("return is sueccessful")]
    public async Task<string> WriteFile(
        [Description("The text to write in file")]
        string input
    )
    {
        _logger.LogInformation("WriteFile started");
        await Task.Delay(1000);
        return "WriteFileSuccessful";
    }
}