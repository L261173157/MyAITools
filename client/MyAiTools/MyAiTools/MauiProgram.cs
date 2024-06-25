using Microsoft.Extensions.Logging;
using MyAiTools.AiFun.Code;
using MyAiTools.AiFun.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Storage;
using CommunityToolkit.Maui;
using MyAiTools.AiFun.plugins.MyPlugin;

namespace MyAiTools;

public static class MauiProgram
{
    public static IServiceProvider Services;
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
        builder.Logging.AddDebug();
        
        
        builder.Services.AddSingleton<IKernelCreat, KernelCreat>();
        builder.Services.AddSingleton<IGetBaseUrl, GetBaseUrlZZZ>();
        builder.Services.AddSingleton<ChatService>();
        builder.Services.AddTransient<PluginService>();
        builder.Services.AddTransient<PlannerService>();
        builder.Services.AddTransient<TestPlugin>();

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