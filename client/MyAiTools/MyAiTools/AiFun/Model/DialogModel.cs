using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.ChatCompletion;

namespace MyAiTools.AiFun.Model
{
    public class DialogModel
    {
        public required int Id { get; set; }
        public required string? Title { get; set; }
        public List<MessageModel> Messages { get; set; } 

        public ChatHistory ChatHistory { get; set; } 

        public DialogModel(string systemMessage)
        {
            ChatHistory = new ChatHistory(systemMessage);
            Messages = new();
        }

        public DialogModel()
        {
            Messages = new();
        }

        /// <summary>
        /// 添加新消息
        /// </summary>
        /// <param name="content"></param>
        /// <param name="role"></param>
        public void AddMessage(string content, ChatRole role)
        {
            var message = new MessageModel
            {
                Content = content,
                Role = role
            };
            Messages.Add(message);
        }

        public void AddChatHistory(string content, ChatRole role)
        {
            switch (role)
            {
                case ChatRole.System:
                    break;
                case ChatRole.User:
                    ChatHistory.AddUserMessage(content);
                    break;
                case ChatRole.Assistant:
                    ChatHistory.AddAssistantMessage(content);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(role), role, null);
            }
        }

        public void Clear()
        {
            ChatHistory.Clear();
            Messages.Clear();
        }
    }
}