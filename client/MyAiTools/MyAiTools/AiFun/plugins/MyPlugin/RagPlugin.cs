using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Memory;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using MyAiTools.AiFun.Services;
using Microsoft.KernelMemory;

namespace MyAiTools.AiFun.plugins.MyPlugin
{
    public class RagPlugin
    {
        private readonly ILogger<RagPlugin> _logger;
        private readonly MemoryServerless _memory;

        public RagPlugin(ILogger<RagPlugin> logger, IKernelCreat kernel)
        {
            _logger = logger;
            _memory = kernel.MemoryServerlessBuild();
        }

        [KernelFunction("QuestionInMemory")]
        [Description("ask some question in the memory")]
        [return: Description("return a answer in memory")]
        public async Task<string> SearchMemoryAsync(
            [Description("The question to ask in memory")]
            string question
        )
        {
            _logger.LogInformation("SearchMemoryAsync started");
            var result =await  _memory.AskAsync(question);
            return result.Result;
        }

        [KernelFunction("StoreFileContentMemory")]
        [Description("pick some file ,then store the picked file's content in the memory")]
        [return: Description("return is sueccessful")]
        public async Task<string> StoreMemoryAsync()
        {
            _logger.LogInformation("StoreMemoryAsync started");
            var filePath = await FilePicker.Default.PickAsync();
            if (filePath != null) await _memory.ImportDocumentAsync(filePath.FullPath);
            return "StoreMemorySuccessful";
        }
    }
}