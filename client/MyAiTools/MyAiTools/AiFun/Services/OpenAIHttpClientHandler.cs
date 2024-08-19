namespace MyAiTools.AiFun.Services;

public class OpenAiHttpClientHandler(IGetBaseUrl baseUrl) : HttpClientHandler
{
    private readonly string _Host = baseUrl.GetHost();

    //拦截请求地址
    private readonly string _Scheme = baseUrl.GetScheme();
    //private string _Path;

    //_Path = baseUrl.GetPath();

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        //拦截地址聊天服务地址
        if (request.RequestUri.LocalPath == "/v1/chat/completions")
        {
            var uriBuilder = new UriBuilder(request.RequestUri)
            {
                Scheme = _Scheme,
                Host = _Host,
                Path = "/v1/chat/completions"
            };
            request.RequestUri = uriBuilder.Uri;
        }
        //拦截地址图片生成服务地址
        else if (request.RequestUri.LocalPath == "/v1/images/generations")
        {
            var uriBuilder = new UriBuilder(request.RequestUri)
            {
                Scheme = _Scheme,
                Host = _Host,
                Path = "/v1/images/generations"
            };
            request.RequestUri = uriBuilder.Uri;
        }
        //拦截地址文本嵌入服务地址
        else if (request.RequestUri.LocalPath == "/v1/embeddings")
        {
            var uriBuilder = new UriBuilder(request.RequestUri)
            {
                Scheme = _Scheme,
                Host = _Host,
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