using System.ComponentModel.DataAnnotations;

namespace KnowledgeBot.Options;

public class AzureOpenAI
{
    [Required]
    public string ChatDeploymentName { get; set; }

    [Required]
    public string Endpoint { get; set; }

    [Required]
    public string ApiKey { get; set; }
}
