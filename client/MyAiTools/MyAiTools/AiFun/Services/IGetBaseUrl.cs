using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAiTools.AiFun.Services
{
    public interface IGetBaseUrl
    {
        public string GetScheme();

        public string GetHost();

        //public string GetPath();
        public string GetApiKey();
    }
}