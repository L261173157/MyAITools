using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.ChatCompletion;
using SQLite;

namespace MyAiTools.AiFun.Model
{
    /// <summary>
    /// message的sqlite数据库模型
    /// </summary>
    public class MessageModel : BaseModel
    {
        [PrimaryKey, AutoIncrement] public new int Id { get; set; }
        public string? Content { get; set; }

        public ChatRole Role { get; set; }

        public int DialogId { get; set; }
    }
}