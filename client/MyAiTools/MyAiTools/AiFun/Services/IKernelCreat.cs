using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAiTools.AiFun.Services
{
    public interface IKernelCreat
    {
        public Kernel KernelBuild();
    }
}
