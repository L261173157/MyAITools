using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.ChatCompletion;
using MyAiTools.AiFun.Data;

namespace MyAiTools.AiFun.Model
{
    public class DialogGroup
    {
        public List<Dialog>? Dialogs { get; private set; }

        private readonly DataBase _dataBase;


        public DialogGroup(DataBase dataBase)
        {
            _dataBase = dataBase;

            // _ = InitDialogs();
        }

        /// <summary>
        /// 新增对话
        /// </summary>
        public async Task AddDialog(string? systemMessage)
        {
            var id = Dialogs.Count + 1;
            var title = string.Empty;
            var dialog = new Dialog(id, title, systemMessage, _dataBase);
            Dialogs.Add(dialog);
            var dialogModel = new DialogModel
            {
                Id = dialog.Id,
                Title = dialog.Title
            };
            await _dataBase.SaveItemAsync<DialogModel>(dialogModel);
        }

        /// <summary>
        /// 初始化对话组
        /// </summary>
        public async Task InitDialogs()
        {
            Dialogs = new();
            try
            {
                //读取数据库中的对话
                var dialogs = await _dataBase.GetItemsAsync<DialogModel>(new DialogModel());
                var allMessages = await _dataBase.GetItemsAsync<MessageModel>(new MessageModel());
                if (dialogs is null)
                {
                    return;
                }

                foreach (var dialogModel in dialogs)
                {
                    var messageModels = allMessages.Where(m => m.DialogId == dialogModel.Id).ToList();
                    List<Message> messages = new();
                    ChatHistory chatHistory = new ChatHistory();
                    foreach (var message in messageModels)
                    {
                        switch (message.Role)
                        {
                            case ChatRole.User:
                                messages.Add(new Message(message.Content, ChatRole.User));
                                chatHistory.AddUserMessage(message.Content);
                                break;
                            case ChatRole.Assistant:
                                messages.Add(new Message(message.Content, ChatRole.Assistant));
                                chatHistory.AddAssistantMessage(message.Content);
                                break;
                            case ChatRole.System:
                                chatHistory.AddSystemMessage(message.Content);
                                break;
                            default:
                                break;
                        }
                    }

                    var dialog = new Dialog(_dataBase, dialogModel.Id, dialogModel.Title, messages, chatHistory);
                    Dialogs.Add(dialog);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task Clear()
        {
            Dialogs.Clear();
            await _dataBase.DeleteAllAsync(new DialogModel());
            await _dataBase.DeleteAllAsync(new MessageModel());
            await InitDialogs();
        }
    }
}