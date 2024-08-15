using System.Linq;

namespace KnowledgeBot.Plugins;

public class KnowledgeBasePlugin
{
    private readonly IKnowledgeBaseService _service;
    private ILogger<KnowledgeBasePlugin> _logger;

    public KnowledgeBasePlugin(ILogger<KnowledgeBasePlugin> logger,IKnowledgeBaseService service)
    {
        _service = service;
        _logger = logger;
    }

    [KernelFunction("get_from_kb")]
    [Description("Search in AI Language Service from custom knowledge base")]
    [return: Description("Answers from the knowledge base from the question asked")]
    public async Task<IEnumerable<string>> GetAnswerKB([Description("question of the user")]string question) 
    {
        _logger.LogInformation($"Called plugin KnowledgeBase with parameter: {question}");

        var answers = await _service.GetAnswersAsync(question);
        
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
