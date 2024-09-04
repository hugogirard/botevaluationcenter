namespace KnowledgeBot.Models;

public record KnowledgeBaseResponse(string KbName, string Answer, bool Error = false);
