using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.KernelMemory;

namespace MyAiTools.AiFun.Services
{
    public interface IKernelCreat
    {
        /// <summary>
        /// 生成kernel
        /// </summary>
        /// <returns></returns>
        public Kernel KernelBuild();

        [Experimental("SKEXP0001")]
        public ISemanticTextMemory MemoryBuild();

        public MemoryServerless MemoryServerlessBuild();
    }
}
