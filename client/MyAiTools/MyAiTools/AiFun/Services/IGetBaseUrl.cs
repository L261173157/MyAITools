namespace MyAiTools.AiFun.Services;

public interface IGetBaseUrl
{
    public string GetScheme();

    public string GetHost();

    //public string GetPath();
    public string GetApiKey();
}