﻿using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elastic.Transport;
using MyAiTools.AiFun.plugins.MyPlugin;
using Microsoft.Maui.Storage;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Plugins.Core;
using Microsoft.SemanticKernel.Plugins.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.DocumentStorage.DevTools;
using Microsoft.KernelMemory.FileSystem.DevTools;
using Microsoft.KernelMemory.MemoryStorage.DevTools;
using Microsoft.SemanticKernel.Embeddings;

namespace MyAiTools.AiFun.Services
{
    public class KernelCreat : IKernelCreat

    {
        private readonly IGetBaseUrl _baseUrl;
        private const string OpenAiChatModelId = "gpt-4o-mini";
        private const string OpenAiEmbeddingModelId = "text-embedding-3-small";

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
            //添加聊天模型
            builder.AddOpenAIChatCompletion(modelId: OpenAiChatModelId, apiKey: openAiKey,
                httpClient: new HttpClient(handler));
            //添加文本转图片模型
            builder.AddOpenAITextToImage(apiKey: openAiKey, httpClient: new HttpClient(handler));
            //添加嵌入模型
            builder.AddOpenAITextEmbeddingGeneration(modelId: OpenAiEmbeddingModelId, apiKey: openAiKey,
                httpClient: new HttpClient(handler));
            //添加依赖注入服务
            builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddConsole());
            //builder.Plugins.AddFromType<TimePlugin>();
            var kernel = builder.Build();
            //添加优化过滤器
            //todo 该处过滤器未实现
            //var memoryStore = new VolatileMemoryStore();
            //var textEmbeddingGenerationService = kernel.GetRequiredService<ITextEmbeddingGenerationService>();
            //kernel.PromptRenderFilters.Add(new FewShotPromptOptimizationFilter(memoryStore, textEmbeddingGenerationService));
            
            return kernel;
        }


        [Experimental("SKEXP0001")]
        public ISemanticTextMemory MemoryBuild()
        {
            var handler = new OpenAiHttpClientHandler(_baseUrl);
            var openAiKey = _baseUrl.GetApiKey();
            var memoryBuilder = new MemoryBuilder();
            memoryBuilder.WithOpenAITextEmbeddingGeneration(modelId: OpenAiEmbeddingModelId, apiKey: openAiKey,
                httpClient: new HttpClient(handler));
            //记忆存在位置，这里使用内存
            memoryBuilder.WithMemoryStore(new VolatileMemoryStore());
            var memory = memoryBuilder.Build();
            return memory;
        }

        //用Kernel Memory的无服务器版本
        public MemoryServerless MemoryServerlessBuild()
        {
            //本地存储位置
            string mainDir = FileSystem.Current.AppDataDirectory;
            var handler = new OpenAiHttpClientHandler(_baseUrl);
            var openAiKey = _baseUrl.GetApiKey();
            //var memory = new KernelMemoryBuilder().WithOpenAIDefaults(openAiKey, httpClient: new HttpClient(handler)).Build<MemoryServerless>();
            var memory = new KernelMemoryBuilder()
                .WithOpenAITextEmbeddingGeneration(
                    new OpenAIConfig() { APIKey = openAiKey, EmbeddingModel = OpenAiEmbeddingModelId },
                    httpClient: new HttpClient(handler))
                .WithOpenAITextGeneration(new OpenAIConfig() { APIKey = openAiKey, TextModel = OpenAiChatModelId },
                    httpClient: new HttpClient(handler))
                .WithSimpleVectorDb(new SimpleVectorDbConfig()
                    { Directory = mainDir, StorageType = FileSystemTypes.Disk })
                .WithSimpleFileStorage(new SimpleFileStorageConfig()
                    { Directory = mainDir, StorageType = FileSystemTypes.Disk })
                .Build<MemoryServerless>();

            return memory;
        }

    }
}