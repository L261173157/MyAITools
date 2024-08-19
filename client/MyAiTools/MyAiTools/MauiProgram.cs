using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using MyAiTools.AiFun.Code;
using MyAiTools.AiFun.plugins.MyPlugin;
using MyAiTools.AiFun.Services;

namespace MyAiTools;

public static class MauiProgram
{
    public static IServiceProvider Services;

    [Experimental("SKEXP0001")]
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>().ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
        });
        builder.UseMauiCommunityToolkit();
        builder.Services.AddMauiBlazorWebView();


        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

        builder.Services.AddTransient<IKernelCreat, KernelCreat>();
        builder.Services.AddTransient<IGetBaseUrl, GetBaseUrl>();
        builder.Services.AddSingleton<ChatService>();
        builder.Services.AddSingleton<PluginService>();
        builder.Services.AddTransient<TestPlugin>();
        builder.Services.AddTransient<GenerateImagePlugin>();
        builder.Services.AddTransient<ToolsPlugin>();
        builder.Services.AddTransient<SearchPlugin>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif
        var app = builder.Build();

        // 保存服务提供者
        Services = app.Services;

        return app;
    }
}