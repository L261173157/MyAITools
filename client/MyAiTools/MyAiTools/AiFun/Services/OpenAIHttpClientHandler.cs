namespace MyAiTools.AiFun.Services;

public enum BaseUrlType
{
    OpenaiEndpoint,
    OpenaiEndpoint2
}

public class OpenAiHttpClientHandler: HttpClientHandler
{
    private readonly string _host;
    //拦截请求地址
    private readonly string _scheme;
    public OpenAiHttpClientHandler(IGetBaseUrl baseUrl, BaseUrlType baseUrlType)
    {
        switch (baseUrlType)
        {
            case BaseUrlType.OpenaiEndpoint:
                _host= baseUrl.GetHost();
                _scheme = baseUrl.GetScheme();
                break;
            case BaseUrlType.OpenaiEndpoint2:
                _host = baseUrl.GetHost2();
                _scheme = baseUrl.GetScheme2();
                break;
            default:
                throw new Exception("未知的请求地址");
        }
    }
    

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        //拦截地址聊天服务地址
        if (request.RequestUri.LocalPath == "/v1/chat/completions")
        {
            var uriBuilder = new UriBuilder(request.RequestUri)
            {
                Scheme = _scheme,
                Host = _host,
                Path = "/v1/chat/completions"
            };
            request.RequestUri = uriBuilder.Uri;
        }
        //拦截地址图片生成服务地址
        else if (request.RequestUri.LocalPath == "/v1/images/generations")
        {
            var uriBuilder = new UriBuilder(request.RequestUri)
            {
                Scheme = _scheme,
                Host = _host,
                Path = "/v1/images/generations"
            };
            request.RequestUri = uriBuilder.Uri;
        }
        //拦截地址文本嵌入服务地址
        else if (request.RequestUri.LocalPath == "/v1/embeddings")
        {
            var uriBuilder = new UriBuilder(request.RequestUri)
            {
                Scheme = _scheme,
                Host = _host,
                Path = "/v1/embeddings"
            };
            request.RequestUri = uriBuilder.Uri;
        }
        else
        {
            throw new Exception("未知的请求地址");
        }

        return await base.SendAsync(request, cancellationToken);
    }
}