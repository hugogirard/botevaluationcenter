using KnowledgeBot.Models;
using KnowledgeBot.Options;
using KnowledgeBot.Plugins;
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
        services.AddSingleton<IChatCompletionService>(sp =>
        {            
            return new AzureOpenAIChatCompletionService(configuration["AzureOpenAI:ChatDeploymentName"],
                                                        configuration["AzureOpenAI:Endpoint"],
                                                        configuration["AzureOpenAI:ApiKey"]);
        });

        KnowledgeBaseConfiguration conf = new();
        var section = configuration.GetSection("KnowledgeBase");
        configuration.GetSection("KnowledgeBase").Bind(conf);

        foreach (var kb in conf.KnowledgeConfiguration)
        {
            services.AddKeyedSingleton(kb.name, (sp, key) =>
            {
                var loggerFactory = sp.GetService<ILoggerFactory>();
                var knowledgeBaseService = sp.GetService<IKnowledgeBaseService>();

                return new KnowledgeBasePlugin(loggerFactory.CreateLogger<KnowledgeBasePlugin>(), knowledgeBaseService, kb.name);
            });
        }

        // Register the Kernel singletone
        services.AddSingleton((sp) =>
        {
            KernelPluginCollection plugins = [];

            // Register all plugins of KB in the kernel
            foreach (var kb in conf.KnowledgeConfiguration)
            {
                plugins.AddFromObject(sp.GetRequiredKeyedService<KnowledgeBasePlugin>(kb.name), kb.name);
            }

            //plugins.AddFromObject(sp.GetRequiredService<KnowledgeBasePlugin>());

            return new Kernel(sp, plugins);
        });



        //var chatCompletion = new AzureOpenAIChatCompletionService(configuration["AzureOpenAI:ChatDeploymentName"],
        //                                                          configuration["AzureOpenAI:Endpoint"],
        //                                                          configuration["AzureOpenAI:ApiKey"]);

        //services.AddSingleton<IChatCompletionService>(chatCompletion);

        //var builder = Kernel.CreateBuilder();
        ////builder.AddAzureOpenAIChatCompletion(configuration["AzureOpenAI:ChatDeploymentName"],
        ////                                     configuration["AzureOpenAI:Endpoint"],
        ////                                     configuration["AzureOpenAI:ApiKey"]);

        //// Register the Kernel singletone
        //services.AddSingleton(builder.Build());
    }
}
