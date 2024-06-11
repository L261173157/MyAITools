using Microsoft.SemanticKernel;
using MyAiTools.AiFun.Services;
using Microsoft.SemanticKernel.Planning.Handlebars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.Connectors.OpenAI;
//using MyAiTools.AiFun.plugins.MyPlugin;
using Microsoft.SemanticKernel.Plugins.Core;


namespace MyAiTools.AiFun.Code
{
    internal class PlannerService
    {
        private readonly Kernel _kernel;
#pragma warning disable SKEXP0060 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
        private readonly HandlebarsPlanner planner;
#pragma warning restore SKEXP0060 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。

        public PlannerService(IKernelCreat kernel)
        {
            _kernel = kernel.KernelBuild();
            //var pluginDirectoryPath = Path.Combine(AppContext.BaseDirectory, "AiFun", "plugins", "OfficePlugin");
            //_kernel.ImportPluginFromPromptDirectory(Path.Combine(pluginDirectoryPath, "FunPlugin"));


#pragma warning disable SKEXP0050 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
            _kernel.ImportPluginFromType<MathPlugin>();
            _kernel.ImportPluginFromType<TimePlugin>();
            _kernel.ImportPluginFromType<FileIOPlugin>();
            _kernel.ImportPluginFromType<ConversationSummaryPlugin>();
            _kernel.ImportPluginFromType<TextPlugin>();
#pragma warning restore SKEXP0050 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。

            ////增加功能
            //CreateFunctionFromPrompt();
#pragma warning disable SKEXP0060 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
            planner = new HandlebarsPlanner(new HandlebarsPlannerOptions() { AllowLoops = true });
#pragma warning restore SKEXP0060 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
        }

        public async Task<string> Plan(string? target)
        {
            try
            {
#pragma warning disable SKEXP0060 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
                var plan = await planner.CreatePlanAsync(_kernel, target);
#pragma warning restore SKEXP0060 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。

#pragma warning disable SKEXP0060 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
                var result = (await plan.InvokeAsync(_kernel)).Trim();
#pragma warning restore SKEXP0060 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。

                return "计划:" + '\n' + plan.ToString() + '\n' + "执行:" + '\n' + result.ToString();
                //return "计划:" + '\n' + plan.ToString() + '\n';
            }
            catch (Exception ex)
            {
                return "对不起，程序报错：" + ex.Message;
            }
        }

        private void CreateFunctionFromPrompt()
        {
            string skPrompt = """
                              {{$input}}
                              Rewrite the above in the style of Shakespeare.
                              """;

            var executionSettings = new OpenAIPromptExecutionSettings
            {
                MaxTokens = 2000,
                Temperature = 0.7,
                TopP = 0.5
            };
            var shakespeareFunction = _kernel.CreateFunctionFromPrompt(skPrompt, executionSettings, "Shakespeare");
        }
    }
}