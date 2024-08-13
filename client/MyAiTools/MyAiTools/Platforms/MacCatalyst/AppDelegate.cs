using Foundation;

namespace MyAiTools;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
#pragma warning disable SKEXP0001
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
#pragma warning restore SKEXP0001
}
