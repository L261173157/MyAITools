using Microsoft.Extensions.Logging;
using MyAiTools.AiFun.Code;
using MyAiTools.AiFun.Services;

namespace MyAiTools;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();
		builder.Services.AddSingleton<IKernelCreat, KernelCreat>();

		builder.Services.AddTransient<ChatService>();
		builder.Services.AddTransient<TranslateService>();
		builder.Services.AddTransient<PlannerService>();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
