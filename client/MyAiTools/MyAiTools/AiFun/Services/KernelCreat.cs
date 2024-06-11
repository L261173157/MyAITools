using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyAiTools.AiFun.plugins.MyPlugin;

namespace MyAiTools.AiFun.Services
{
    public class KernelCreat : IKernelCreat

    {
        public Kernel KernelBuild()
        {
            var handler = new OpenAIHttpClientHandler();
            var openAiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            var builder = Kernel.CreateBuilder();
            //添加聊天模型
            builder.AddOpenAIChatCompletion(modelId: "gemini-1.5-pro", apiKey: openAiKey, httpClient: new HttpClient(handler));
            //builder.AddOpenAIChatCompletion(modelId: "gpt-4o", apiKey: openAiKey, httpClient: new HttpClient(handler));
            //添加文本转图片模型
#pragma warning disable SKEXP0012 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
#pragma warning disable SKEXP0010 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
            builder.AddOpenAITextToImage(apiKey: openAiKey, httpClient: new HttpClient(handler));
#pragma warning restore SKEXP0010 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
#pragma warning restore SKEXP0012 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。

            //添加插件
            //builder.Plugins.AddFromType<MathPlugin>();

            var kernel = builder.Build();
            return kernel;
        }
    }
}
