using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAiTools.AiFun.Services
{
    public class GetBaseUrlALong: IGetBaseUrl
    {
        //接口供应商 阿龙
        //https://0pena1.com/
        public string GetScheme()
        {
            string baseUrl = "https://api.openai-all.com/";
            return baseUrl;
        }

        public string GetHost()
        {
            string baseUrl = "api.openai-all.com";
            return baseUrl;
        }

        //public string GetPath()
        //{
        //    throw new NotImplementedException();
        //}

        public string GetApiKey()
        {
            string apiKey = Environment.GetEnvironmentVariable("Openai_api_key_along");
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("未找到阿龙的API Key");
            }
            return apiKey;

        }
    }
}
