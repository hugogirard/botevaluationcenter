
namespace KnowledgeBot.Services.Chat
{
    public interface IChatService
    {
        Task<string> GetCompletionAsync(string question);
    }
}