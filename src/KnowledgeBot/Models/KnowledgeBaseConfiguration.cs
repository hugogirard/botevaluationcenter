using Microsoft.Extensions.Configuration;
using System.Configuration;

namespace KnowledgeBot.Models;

public class KnowledgeBaseConfiguration 
{
    private readonly IConfiguration _configuration;

    public KnowledgeBaseConfiguration(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void LoadConfiguration() 
    {
        var section = _configuration.GetSection("KnowledgeBase");
        _configuration.GetSection("KnowledgeBase").Bind(this);
    }

    public IEnumerable<KnowledgeBase> KnowledgeConfiguration { get; set; }
}

public record KnowledgeBase(string name, string appRoles, string displayName);
