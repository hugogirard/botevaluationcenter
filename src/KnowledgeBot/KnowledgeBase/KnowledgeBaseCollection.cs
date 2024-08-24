using KnowledgeBot.RAG;
using System.Linq;

namespace KnowledgeBot.KnowledgeBase;

public class KnowledgeBaseCollection
{
    private readonly Dictionary<string, IKnowledgeService> _dict = new();

    public void AddKnowledgeBase(string key, IKnowledgeService service) => _dict.Add(key, service);

    public IReadOnlyDictionary<string, IKnowledgeService> GetKnowledgeBases() => _dict;

    public bool Any() => _dict.Any();
}
