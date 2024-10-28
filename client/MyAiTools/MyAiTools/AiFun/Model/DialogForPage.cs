namespace MyAiTools.AiFun.Model;

public class DialogForPage : DialogModel
{
    public List<Message>? CurrentMessages { get; set; } = new();

    public List<Dialog>? Dialogs { get; set; } = new();
}