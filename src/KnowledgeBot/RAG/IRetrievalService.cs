namespace KnowledgeBot.RAG;

public interface IRetrievalService
{
    Task<IEnumerable<string>> GetAnswersAsync(string question);
}
