using System.Collections.Generic;
using System.Threading.Tasks;

namespace KnowledgeBot.Services;

public interface ILanguageService
{
    Task<IEnumerable<string>> GetAnswersAsync(string question, string projectName);
}