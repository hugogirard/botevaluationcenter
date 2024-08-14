using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace KnowledgeBot.Infrastructure;

public static class Extension
{
    /// <summary>
    /// This method register Semantic Kernel and all needed plugins
    /// </summary>    
    public static void RegisterSemanticKernel(this IServiceCollection services) 
    {
        //services.AddSingleton<IChatCompletionService>(sp =>
        //{
        //    // A custom HttpClient can be provided to this constructor
        //    return new AzureOpenAIChatCompletionService(options.ChatDeploymentName, options.Endpoint, options.ApiKey);
        //});
    }
}
