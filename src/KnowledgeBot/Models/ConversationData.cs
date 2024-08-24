namespace KnowledgeBot.Models;

public class ConversationData
{
    public string Question { get; set; }

    public string Answer { get; set; }

    public  string KnowledgeBaseName { get; set; }

    public bool FoundInKnowledgeBase { get; set; }
    public bool FoundInExtendedSource { get; internal set; }
}
