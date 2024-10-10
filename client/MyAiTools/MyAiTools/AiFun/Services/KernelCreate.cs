using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.DocumentStorage.DevTools;
using Microsoft.KernelMemory.FileSystem.DevTools;
using Microsoft.KernelMemory.MemoryStorage.DevTools;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Memory;

namespace MyAiTools.AiFun.Services;

public class KernelCreate : IKernelCreat

{
    private const string OpenAiChatModelId = "gpt-4o";
    private const string OpenAiEmbeddingModelId = "text-embedding-ada-002";
    private readonly IGetBaseUrl _baseUrl;

    public KernelCreate(IGetBaseUrl baseUrl)
    {
        _baseUrl = baseUrl;
    }

    [Experimental("SKEXP0001")]
    public Kernel KernelBuild()
    {
        var handler = new OpenAiHttpClientHandler(_baseUrl,baseUrlType:BaseUrlType.OpenaiEndpoint);
        var openAiKey = _baseUrl.GetApiKey();
        var builder = Kernel.CreateBuilder();
        //builder.Plugins.AddFromType<MathPlugin>();
        //添加聊天模型
        builder.AddOpenAIChatCompletion(OpenAiChatModelId, openAiKey,
            httpClient: new HttpClient(handler));
        //添加文本转图片模型
        builder.AddOpenAITextToImage(openAiKey, httpClient: new HttpClient(handler));
        //添加嵌入模型
        builder.AddOpenAITextEmbeddingGeneration(OpenAiEmbeddingModelId, openAiKey,
            httpClient: new HttpClient(handler));
        //添加依赖注入服务
        builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddConsole());
        //builder.Plugins.AddFromType<TimePlugin>();
        var kernel = builder.Build();
        //添加优化过滤器
        //todo 该处过滤器未实现简化prompt的功能
        //var memoryStore = new VolatileMemoryStore();
        //var textEmbeddingGenerationService = kernel.GetRequiredService<ITextEmbeddingGenerationService>();
        //kernel.PromptRenderFilters.Add(new FewShotPromptOptimizationFilter(memoryStore, textEmbeddingGenerationService));

        return kernel;
    }


    [Experimental("SKEXP0001")]
    public ISemanticTextMemory MemoryBuild()
    {
        var handler = new OpenAiHttpClientHandler(_baseUrl,baseUrlType:BaseUrlType.OpenaiEndpoint);
        var openAiKey = _baseUrl.GetApiKey();
        var memoryBuilder = new MemoryBuilder();
        memoryBuilder.WithOpenAITextEmbeddingGeneration(OpenAiEmbeddingModelId, openAiKey,
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
        var mainDir = FileSystem.Current.AppDataDirectory;
        var handler1 = new OpenAiHttpClientHandler(_baseUrl, baseUrlType: BaseUrlType.OpenaiEndpoint);
        var handler2 = new OpenAiHttpClientHandler(_baseUrl, baseUrlType: BaseUrlType.OpenaiEndpoint2);
        var openAiKey1 = _baseUrl.GetApiKey();
        var openAiKey2 = _baseUrl.GetApiKey2();
        //var memory = new KernelMemoryBuilder().WithOpenAIDefaults(openAiKey, httpClient: new HttpClient(handler)).Build<MemoryServerless>();
        var memory = new KernelMemoryBuilder()
            .WithOpenAITextEmbeddingGeneration(
                new OpenAIConfig { APIKey = openAiKey2, EmbeddingModel = OpenAiEmbeddingModelId },
                httpClient: new HttpClient(handler2))
            .WithOpenAITextGeneration(new OpenAIConfig { APIKey = openAiKey1, TextModel = OpenAiChatModelId },
                httpClient: new HttpClient(handler1))
            .WithSimpleVectorDb(new SimpleVectorDbConfig { Directory = mainDir, StorageType = FileSystemTypes.Disk })
            .WithSimpleFileStorage(new SimpleFileStorageConfig
            { Directory = mainDir, StorageType = FileSystemTypes.Disk })
            .Build<MemoryServerless>();

        return memory;
    }
}