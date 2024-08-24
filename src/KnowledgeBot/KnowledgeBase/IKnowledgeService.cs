namespace KnowledgeBot.KnowledgeBase
{
    public interface IKnowledgeService
    {
        Task<IEnumerable<string>> GetAnswerKB(string question);
    }
}
