namespace MyAiTools.AiFun.Services;

public interface IGetBaseUrl
{
    public string GetScheme();

    public string GetHost();

    //public string GetPath();
    public string GetApiKey();

    //根据配置获取基础地址
    public string GetScheme2();

    public string GetHost2();

    public string GetApiKey2();
}