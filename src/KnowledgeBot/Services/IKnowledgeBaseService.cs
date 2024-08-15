using System.Collections.Generic;
using System.Threading.Tasks;

namespace KnowledgeBot.Services;

public interface IKnowledgeBaseService
{
    Task<IEnumerable<string>> GetAnswersAsync(string question);
}