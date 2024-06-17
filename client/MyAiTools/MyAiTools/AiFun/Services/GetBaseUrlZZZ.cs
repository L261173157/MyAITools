using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAiTools.AiFun.Services
{
    public class GetBaseUrlZZZ : IGetBaseUrl
    {
        //智增增接口供应商
        //https://zhizengzeng.com/
        public string GetScheme()
        {
            string baseUrl = "https://api.zhizengzeng.com/";
            return baseUrl;
        }

        public string GetHost()
        {
            string baseUrl = "api.zhizengzeng.com";
            return baseUrl;
        }

        public string GetPath()
        {
            throw new NotImplementedException();
        }

        public string GetApiKey()
        {
            string apiKey = Environment.GetEnvironmentVariable("Openai_api_key_zzz");
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("未找到智增增的API Key");
            }
            return apiKey;
            
        }
    }
}
