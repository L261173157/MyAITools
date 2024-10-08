﻿using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.TextToImage;
using MyAiTools.AiFun.Services;

namespace MyAiTools.AiFun.plugins.MyPlugin;

//图片生成插件
[Experimental("SKEXP0001")]
public class GenerateImagePlugin
{
    private readonly ITextToImageService _dallE;
    private readonly ILogger<GenerateImagePlugin> _logger;

    public GenerateImagePlugin(IKernelCreat kernel, ILogger<GenerateImagePlugin> logger)
    {
        _logger = logger;
        var kernel1 = kernel.KernelBuild();
        _dallE = kernel1.GetRequiredService<ITextToImageService>();
    }

    [KernelFunction("GenerateImage")]
    [Description("Input a description of an image and return a URL address of the image,image's size is 512*512")]
    [return: Description("return a URL address")]
    public async Task<string> GenerateImage(string text)
    {
        _logger.LogInformation("GenerateImage started");
        var imageUrl = await _dallE.GenerateImageAsync(text, 512, 512);
        StringBuilder sb = new();
        sb.Append("<img src=");
        sb.Append(imageUrl);
        sb.Append(">");
        return imageUrl;
    }
}