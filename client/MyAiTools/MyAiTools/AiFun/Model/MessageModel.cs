using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAiTools.AiFun.Model
{
    public enum ChatRole
    {
        System,
        User,
        Assistant
    }

    public class MessageModel
    {
        public string? Content { get; set; }

        public ChatRole Role { get; set; }
    }
}
