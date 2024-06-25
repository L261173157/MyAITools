using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyAiTools.AiFun.plugins.MyPlugin;
using Microsoft.Maui.Storage;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Plugins.Core;
using Microsoft.SemanticKernel.Plugins.Memory;

namespace MyAiTools.AiFun.Services
{
    public class KernelCreat : IKernelCreat

    {
        private readonly IGetBaseUrl _baseUrl;

        public KernelCreat(IGetBaseUrl baseUrl)
        {
            _baseUrl = baseUrl;
        }

        [Experimental("SKEXP0001")]
        public Kernel KernelBuild()
        {
            var handler = new OpenAiHttpClientHandler(_baseUrl);
            var openAiKey = _baseUrl.GetApiKey();
            var builder = Kernel.CreateBuilder();
            //builder.Plugins.AddFromType<MathPlugin>();
            //添加聊天模型，这里使用gpt-4o
            builder.AddOpenAIChatCompletion(modelId: "gpt-4o", apiKey: openAiKey,
                httpClient: new HttpClient(handler));
            //添加文本转图片模型
            builder.AddOpenAITextToImage(apiKey: openAiKey, httpClient: new HttpClient(handler));
            //添加插件

            var kernel = builder.Build();
            return kernel;
        }

       
        [Experimental("SKEXP0001")]
        public ISemanticTextMemory MemoryBuild()
        {
            var handler = new OpenAiHttpClientHandler(_baseUrl);
            var openAiKey = _baseUrl.GetApiKey();
            var memoryBuilder = new MemoryBuilder();
            memoryBuilder.WithOpenAITextEmbeddingGeneration(modelId: "text-embedding-3-small", apiKey: openAiKey,
                httpClient: new HttpClient(handler));
            //记忆存在位置，这里使用内存
            memoryBuilder.WithMemoryStore(new VolatileMemoryStore());
            var memory = memoryBuilder.Build();
            return memory;
        }
    }
}