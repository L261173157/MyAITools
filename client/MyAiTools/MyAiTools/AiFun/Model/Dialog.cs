using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.ChatCompletion;
using MyAiTools.AiFun.Data;
using SQLite;

namespace MyAiTools.AiFun.Model
{
    public class Dialog
    {
        public int Id { get; set; }
        public string? Title { get; set; }

        private string _systemMessage;
        public List<Message> Messages { get; set; }
        public ChatHistory? ChatHistory { get; set; }

        private DataBase _dataBase;

        public Dialog(DataBase dataBase, int id, string title, List<Message> messages, ChatHistory chatHistory)
        {
            Id = id;
            Title = title;
            Messages = messages;
            ChatHistory = chatHistory;
            _dataBase = dataBase;
        }

        public Dialog(int id, string title, string systemMessage, DataBase dataBase)
        {
            Id = id;
            Title = title;
            _systemMessage = systemMessage;
            ChatHistory = new ChatHistory(_systemMessage);
            Messages = new();
            _dataBase = dataBase;
            //保存到数据库
            var messageModel = new MessageModel
            {
                Content = systemMessage,
                Role = ChatRole.System,
                DialogId = Id
            };
            _ = _dataBase.SaveItemAsync(messageModel);
        }

        /// <summary>
        /// 添加新消息
        /// </summary>
        /// <param name="content"></param>
        /// <param name="role"></param>
        public void AddMessage(string content, ChatRole role)
        {
            var message = new Message(content, role);
            Messages.Add(message);
        }

        /// <summary>
        /// 添加聊天记录
        /// </summary>
        /// <param name="content"></param>
        /// <param name="role"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void AddChatHistory(string content, ChatRole role)
        {
            switch (role)
            {
                case ChatRole.System:
                    break;
                case ChatRole.User:
                    ChatHistory?.AddUserMessage(content);
                    break;
                case ChatRole.Assistant:
                    ChatHistory?.AddAssistantMessage(content);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(role), role, null);
            }

            //保存到数据库
            var messageModel = new MessageModel
            {
                Content = content,
                Role = role,
                DialogId = Id
            };
            _ = _dataBase.SaveItemAsync(messageModel);
        }

        /// <summary>
        /// 清空对话
        /// </summary>
        public async Task Clear()
        {
            ChatHistory?.Clear();
            Messages.Clear();
            Title = string.Empty;
            await UpdateTitle(Title);
            var messageModels = await _dataBase.GetItemsAsync<MessageModel>(new MessageModel());
            foreach (var messageModel in messageModels)
            {
                if (messageModel.DialogId == Id)
                {
                    await DeleteMessage(messageModel.Id);
                }
            }

            ChatHistory.AddSystemMessage(_systemMessage);
            await SaveToDataBase();
        }

        /// <summary>
        /// 更新标题
        /// </summary>
        /// <param name="title"></param>
        public async Task UpdateTitle(string? title)
        {
            Title = title;
            var dialogModel = new DialogModel
            {
                Id = Id,
                Title = Title
            };
            await _dataBase.UpdateItemAsync(dialogModel);
        }

        /// <summary>
        /// 删除消息
        /// </summary>
        /// <param name="id"></param>
        private async Task DeleteMessage(int id)
        {
            var message = new MessageModel()
            {
                Id = id
            };
            await _dataBase.DeleteItemAsync(message);
        }

        /// <summary>
        /// chat history同步到数据库
        /// </summary>
        private async Task SaveToDataBase()
        {
            foreach (var message in ChatHistory)
            {
                var messageModel = new MessageModel
                {
                    Content = message.Content,
                    Role = MessageRole(),
                    DialogId = Id
                };
                await _dataBase.SaveItemAsync(messageModel);
                continue;

                ChatRole MessageRole()
                {
                    if (message.Role == AuthorRole.System)
                    {
                        return ChatRole.System;
                    }

                    if (message.Role == AuthorRole.Assistant)
                    {
                        return ChatRole.Assistant;
                    }

                    if (message.Role == AuthorRole.User)
                    {
                        return ChatRole.User;
                    }

                    return ChatRole.System;
                }
            }
        }
    }
}