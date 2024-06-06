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
        //AOT更换拦截地址
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

        return await base.SendAsync(request, cancellationToken);

    }
}

