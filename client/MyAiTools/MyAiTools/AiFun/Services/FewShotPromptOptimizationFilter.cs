using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Memory;

namespace MyAiTools.AiFun.Services
{
#pragma warning disable SKEXP0001
    public sealed class FewShotPromptOptimizationFilter(
        IMemoryStore memoryStore,
        ITextEmbeddingGenerationService textEmbeddingGenerationService) : IPromptRenderFilter
#pragma warning restore SKEXP0001
    {
        /// <summary>
        /// Maximum number of examples to use which are similar to original request.
        /// </summary>
        private const int TopN = 5;

        /// <summary>
        /// Collection name to use in memory store.
        /// </summary>
        private const string CollectionName = "examples";

        [Experimental("SKEXP0001")]
        public async Task OnPromptRenderAsync(PromptRenderContext context, Func<PromptRenderContext, Task> next)
        {
            // Get examples and original request from arguments.
            var examples = context.Arguments["Examples"] as List<string>;
            var request = context.Arguments["Request"] as string;

            if (examples is { Count: > 0 } && !string.IsNullOrEmpty(request))
            {
                var memoryRecords = new List<MemoryRecord>();

                // Generate embedding for each example.
                var embeddings = await textEmbeddingGenerationService.GenerateEmbeddingsAsync(examples);

                // Create memory record instances with example text and embedding.
                for (var i = 0; i < examples.Count; i++)
                {
                    memoryRecords.Add(MemoryRecord.LocalRecord(Guid.NewGuid().ToString(), examples[i], "description",
                        embeddings[i]));
                }

                // Create collection and upsert all memory records for search.
                // It's possible to do it only once and re-use the same examples for future requests.
                await memoryStore.CreateCollectionAsync(CollectionName);
                await memoryStore.UpsertBatchAsync(CollectionName, memoryRecords).ToListAsync();

                // Generate embedding for original request.
                var requestEmbedding = await textEmbeddingGenerationService.GenerateEmbeddingAsync(request);

                // Find top N examples which are similar to original request.
                var topNExamples = await memoryStore.GetNearestMatchesAsync(CollectionName, requestEmbedding, TopN)
                    .ToListAsync();

                // Override arguments to use only top N examples, which will be sent to LLM.
                context.Arguments["Examples"] = topNExamples.Select(l => l.Item1.Metadata.Text);
            }

            // Continue prompt rendering operation.
            await next(context);
        }
    }
}