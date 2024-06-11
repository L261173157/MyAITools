using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAiTools.AiFun.Services;

public class OpenAIHttpClientHandler : HttpClientHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        //拦截地址聊天服务地址
        if (request.RequestUri.LocalPath == "/v1/chat/completions")
        {
            UriBuilder uriBuilder = new UriBuilder(request.RequestUri)
            {
                Scheme = "https://api.openai-all.com/",
                Host = "api.openai-all.com",
                Path = "/v1/chat/completions"
            };
            request.RequestUri = uriBuilder.Uri;
        }
        //拦截地址图片生成服务地址
        else if(request.RequestUri.LocalPath == "/v1/images/generations")
        {
            UriBuilder uriBuilder = new UriBuilder(request.RequestUri)
            {
                Scheme = "https://api.openai-all.com/",
                Host = "api.openai-all.com",
                Path = "/v1/images/generations"
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

