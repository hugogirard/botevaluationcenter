
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace KnowledgeBot.Services.Chat
{
    public interface IChatService
    {
        Task<KnowledgeBaseResponse> GetAnswerFromKnowledgeBaseAsync(string question);

        Task<string> GetAnswerFromExtendedSourceAsync(string question);
    }
}