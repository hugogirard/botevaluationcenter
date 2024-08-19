
using KnowledgeBot.Plugins;
using System.Linq;

namespace KnowledgeBot.KnowledgeBase;

public class KnowledgeService : IKnowledgeService
{
    private readonly ILanguageService _service;
    private ILogger<KnowledgeService> _logger;
    private readonly string _kbName;

    public KnowledgeService(ILogger<KnowledgeService> logger, ILanguageService service, string kbName)
    {
        _service = service;
        _logger = logger;
        _kbName = kbName;
    }

    public async Task<IEnumerable<string>> GetAnswerKB(string question)
    {
        _logger.LogInformation($"Called plugin KnowledgeBase with parameter: {question}");

        var answers = await _service.GetAnswersAsync(question, _kbName);

        if (answers.Count() == 0)
        {
            _logger.LogInformation("No answer found from the KB in the KnowledgePlugin");
        }
        else
        {
            _logger.LogInformation("Answers retrieved from the Knowledge Plugin");
            foreach (var answer in answers)
            {
                _logger.LogInformation(answer);
            }
        }

        return answers;
    }
}
