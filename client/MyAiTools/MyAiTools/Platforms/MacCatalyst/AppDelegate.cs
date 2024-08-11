using Foundation;

namespace MyAiTools;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
#pragma warning disable Roslyn.SKEXP0001
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
#pragma warning restore Roslyn.SKEXP0001
}
