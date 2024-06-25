using Microsoft.SemanticKernel;
using MyAiTools.AiFun.Services;
using Microsoft.SemanticKernel.Planning.Handlebars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.OpenAI;
//using MyAiTools.AiFun.plugins.MyPlugin;
using Microsoft.SemanticKernel.Plugins.Core;
using MyAiTools.AiFun.plugins.MyPlugin;


namespace MyAiTools.AiFun.Code
{
#pragma warning disable SKEXP0060 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
#pragma warning disable SKEXP0050 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
    internal class PlannerService
    {
        private readonly Kernel _kernel;

        private readonly HandlebarsPlanner planner;


        private readonly ILogger<PlannerService> _logger;

        public PlannerService(IKernelCreat kernel, ILogger<PlannerService> logger)
        {
            _kernel = kernel.KernelBuild();
            //var pluginDirectoryPath = Path.Combine(AppContext.BaseDirectory, "AiFun", "plugins", "MyPlugin");
            //_kernel.ImportPluginFromPromptDirectory(Path.Combine(pluginDirectoryPath, "TranslatePlugin"));
            //_kernel.ImportPluginFromPromptDirectory(Path.Combine(pluginDirectoryPath, "WriterPlugin"));


            _kernel.ImportPluginFromType<TestPlugin>();
            //_kernel.ImportPluginFromType<TimePlugin>();
            //_kernel.ImportPluginFromType<FileIOPlugin>();
            //_kernel.ImportPluginFromType<ConversationSummaryPlugin>();
            //_kernel.ImportPluginFromType<TextPlugin>();


            ////增加功能
            //CreateFunctionFromPrompt();

            planner = new HandlebarsPlanner(new HandlebarsPlannerOptions() { AllowLoops = true });


            //添加日志
            _logger = logger;
        }

        public async Task<string> Plan(string target)
        {
            if (target == null) throw new ArgumentNullException(nameof(target));
            try
            {
                var plan = await planner.CreatePlanAsync(_kernel, target);

                //增加到日志
                _logger.LogInformation("计划:" + '\n' + plan.ToString());


                var result = (await plan.InvokeAsync(_kernel, new KernelArguments())).Trim();

                //增加到日志
                _logger.LogInformation("执行:" + '\n' + result.ToString());

                return "计划:" + '\n' + plan.ToString() + '\n' + "执行:" + '\n' + result.ToString();
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