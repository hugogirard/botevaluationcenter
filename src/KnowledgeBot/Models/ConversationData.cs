using System;

namespace KnowledgeBot.Models;

public class ConversationData
{
    public string SessionId { get; set; }

    public string MessageId { get; internal set; }

    public string Question { get; set; }

    public string Answer { get; set; }

    public  string KnowledgeBaseName { get; set; }

    public bool FoundInKnowledgeBase { get; set; }

    public bool FoundInExtendedSource { get; internal set; }

    public ConversationData()
    {
        MessageId = Guid.NewGuid().ToString();
    }
}
