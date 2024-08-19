using KnowledgeBot.RAG;
using System.Linq;

namespace KnowledgeBot.Services;

public class RetrievalServiceCollection
{
    private readonly Dictionary<string, IRetrievalService> _dict = new();

    public void AddRetrivalService(string key, IRetrievalService service) => _dict.Add(key, service);

    public IReadOnlyDictionary<string,IRetrievalService> GetRetrivalService() => _dict;
    
    public bool Any() => _dict.Any();
}
