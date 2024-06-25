using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;
using MyAiTools.AiFun.Code;
using MyAiTools.AiFun.Services;
using static System.Net.Mime.MediaTypeNames;

namespace MyAiTools.AiFun.plugins.MyPlugin
{
    public class TestPlugin
    {
        [KernelFunction, Description("write a new .txt file on the path")]
        public static void WriteFile(
            [Description("The text to write in file")]
            string input
        )
        {
            string filename = "example.txt";
            string text = input;
            string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                filename);
            File.WriteAllText(filePath, text);
            string readText = File.ReadAllText(filePath);
        }
    }
}