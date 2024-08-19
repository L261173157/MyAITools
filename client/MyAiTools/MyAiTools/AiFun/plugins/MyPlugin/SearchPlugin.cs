using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Plugins.Web;
using Microsoft.SemanticKernel.Plugins.Web.Bing;

namespace MyAiTools.AiFun.plugins.MyPlugin;

[Experimental("SKEXP0050")]
internal class SearchPlugin
{
    private readonly ILogger<SearchPlugin> _logger;
    public WebSearchEnginePlugin Bing;

    public SearchPlugin(ILogger<SearchPlugin> logger)
    {
        _logger = logger;
        var bingApiKey = Environment.GetEnvironmentVariable("BING_SEARCH_V7_SUBSCRIPTION_KEY");
        if (bingApiKey == null) return;
        var bingConnector = new BingConnector(bingApiKey);
        Bing = new WebSearchEnginePlugin(bingConnector);
    }
}