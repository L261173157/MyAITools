using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace MyAiTools.AiFun.Model
{
    public enum ChatRole
    {
        System,
        User,
        Assistant
    }

    public class Message(string content, ChatRole role)
    {
        public string? Content { get; set; } = content;

        public ChatRole Role { get; set; } = role;
    }
}