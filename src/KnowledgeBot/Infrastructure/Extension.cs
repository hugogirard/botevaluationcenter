using KnowledgeBot.Models;
using KnowledgeBot.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Configuration;
using System.Text.Json;

namespace KnowledgeBot.Infrastructure;

public static class Extension
{
    /// <summary>
    /// This method register Semantic Kernel and all needed plugins
    /// </summary>    
    public static void RegisterSemanticKernel(this IServiceCollection services, IConfiguration configuration) 
    {
        services.AddSingleton<KnowledgeBaseConfiguration>();

        var builder = Kernel.CreateBuilder();
        builder.AddAzureOpenAIChatCompletion(configuration["AzureOpenAI:ChatDeploymentName"],
                                             configuration["AzureOpenAI:Endpoint"],
                                             configuration["AzureOpenAI:ApiKey"]);

        // Register the Kernel singletone
        services.AddSingleton(builder.Build());
    }
}
