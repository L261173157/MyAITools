namespace MyAiTools.AiFun.Services;

public class GetBaseUrl : IGetBaseUrl
{
    //Openai_endpoint 是智增增地址https://zhizengzeng.com/
    //Openai_endpoint2 是token地址https://api.token-ai.cn/
    public string GetScheme()
    {
        var endpoint = Environment.GetEnvironmentVariable("Openai_endpoint");
        if (string.IsNullOrEmpty(endpoint)) throw new Exception("未找到API Endpoint");
        var scheme = endpoint;
        return scheme;
    }

    public string GetHost()
    {
        var url = Environment.GetEnvironmentVariable("Openai_endpoint");
        var uri = new Uri(url ?? throw new Exception("未找到API Endpoint"));
        var host = uri.Host;
        return host;
    }

    public string GetApiKey()
    {
        var apiKey = Environment.GetEnvironmentVariable("Openai_api_key");
        if (string.IsNullOrEmpty(apiKey)) throw new Exception("未找到API Key");

        return apiKey;
    }

    public string GetScheme2()
    {
        var endpoint = Environment.GetEnvironmentVariable("Openai_endpoint2");
        if (string.IsNullOrEmpty(endpoint)) throw new Exception("未找到API Endpoint");
        var scheme = endpoint;
        return scheme;
    }

    public string GetHost2()
    {
        var url = Environment.GetEnvironmentVariable("Openai_endpoint2");
        var uri = new Uri(url ?? throw new Exception("未找到API Endpoint"));
        var host = uri.Host;
        return host;
    }

    public string GetApiKey2()
    {
        var apiKey = Environment.GetEnvironmentVariable("Openai_api_key2");
        if (string.IsNullOrEmpty(apiKey)) throw new Exception("未找到API Key");

        return apiKey;
    }
}