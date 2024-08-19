using System.Diagnostics.CodeAnalysis;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;

namespace MyAiTools.AiFun.Services;

public interface IKernelCreat
{
    /// <summary>
    ///     生成kernel
    /// </summary>
    /// <returns></returns>
    public Kernel KernelBuild();

    [Experimental("SKEXP0001")]
    public ISemanticTextMemory MemoryBuild();

    public MemoryServerless MemoryServerlessBuild();
}