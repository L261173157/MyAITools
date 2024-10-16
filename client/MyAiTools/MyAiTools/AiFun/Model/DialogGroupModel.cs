using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAiTools.AiFun.Model
{
    public class DialogGroupModel
    {
        public List<DialogModel> Dialogs { get; set; }

        public DialogGroupModel(string defSystemMessage)
        {
            Dialogs = new();
            AddDialog(defSystemMessage);
        }

        /// <summary>
        /// 新增对话
        /// </summary>
        public void AddDialog(string systemMessage)
        {
            var dialog = new DialogModel(systemMessage)
            {
                Id = Dialogs.Count + 1,
                Title = $"Dialog {Dialogs.Count + 1}"
            };
            Dialogs.Add(dialog);
        }
    }
}
