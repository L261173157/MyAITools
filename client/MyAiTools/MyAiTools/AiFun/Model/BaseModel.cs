using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace MyAiTools.AiFun.Model
{
    /// <summary>
    /// sqlite数据库模型基类
    /// </summary>
    public class BaseModel
    {
        public BaseModel()
        {
        }

        [PrimaryKey] public int Id { get; set; }
    }
}