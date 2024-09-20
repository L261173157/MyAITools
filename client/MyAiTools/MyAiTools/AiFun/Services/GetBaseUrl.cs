namespace MyAiTools.AiFun.Services;

public class GetBaseUrl : IGetBaseUrl
{
    //智增增接口供应商
    //https://zhizengzeng.com/
    public string GetScheme()
    {
        var endpoint = Environment.GetEnvironmentVariable("Openai_endpoint");
        if (string.IsNullOrEmpty(endpoint)) throw new Exception("未找到API Endpoint");
        var baseUrl = endpoint;
        return baseUrl;
    }

    public string GetHost()
    {
        var baseUrl = "api.zhizengzeng.com";
        return baseUrl;
    }

    //public string GetPath()
    //{
    //    throw new NotImplementedException();
    //}

    public string GetApiKey()
    {
        var apiKey = Environment.GetEnvironmentVariable("Openai_api_key");
        if (string.IsNullOrEmpty(apiKey)) throw new Exception("未找到API Key");

        return apiKey;
    }
}