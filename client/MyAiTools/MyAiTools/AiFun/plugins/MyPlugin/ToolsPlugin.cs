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
    public class ToolsPlugin
    {
        private readonly ILogger<ToolsPlugin> _logger;
        private readonly MemoryServerless _memory;

        public ToolsPlugin(ILogger<ToolsPlugin> logger, IKernelCreat kernel)
        {
            _logger = logger;
            _memory = kernel.MemoryServerlessBuild();
        }

        [KernelFunction("select_local_file")]
        [Description("selet a local file,return full path")]
        [return: Description("full path")]
        public async Task<string> SeletFile(
        )
        {
            var filePath = await FilePicker.Default.PickAsync();
            return filePath?.FullPath;
        }

       
    }
}