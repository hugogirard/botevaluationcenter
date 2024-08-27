using KnowledgeBot.KnowledgeBase;
using KnowledgeBot.Models;
using KnowledgeBot.Options;

using KnowledgeBot.RAG;
using KnowledgeBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace KnowledgeBot.Infrastructure;

public static class Extension
{
    public static void RegisterState(this IServiceCollection services) 
    {
#if DEBUG
        services.AddSingleton<IStorage, MemoryStorage>();
#endif

        services.AddSingleton<ConversationState>();

        services.AddSingleton<IStateService, StateService>();
    }

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
        KnowledgeBaseCollection knowledgeBaseCollection = new();

        // Load all KB from the configuration and create instance of KnowledgeService
        foreach (var kb in conf.KnowledgeConfiguration)
        {
            var serviceProvider = services.BuildServiceProvider();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var languageService = serviceProvider.GetService<ILanguageService>();
            var instance = new KnowledgeService(loggerFactory.CreateLogger<KnowledgeService>(), languageService, kb.name);
            knowledgeBaseCollection.AddKnowledgeBase(kb.name, instance);
        }

        if (knowledgeBaseCollection.Any())
        {
            services.AddSingleton(knowledgeBaseCollection);
        }

        // Now load all RAG implementation that come from the interface IRetrievalService
        var baseInterface = typeof(IRetrievalService);
        var interfaceTypes = Assembly.GetExecutingAssembly().GetTypes()
                             .Where(t => t.IsClass && !t.IsAbstract && baseInterface.IsAssignableFrom(t));

        var retrievalCollection = new RetrievalServiceCollection();
        var sp = services.BuildServiceProvider();
        foreach (var interfaceType in interfaceTypes)
        {
          
            var constructor = interfaceType.GetConstructors().First();
            var parameters = constructor.GetParameters()
                                        .Select(p => sp.GetService(p.ParameterType))
                                        .ToArray();
            var instance = Activator.CreateInstance(interfaceType, parameters);
            retrievalCollection.AddRetrivalService(interfaceType.Name, (IRetrievalService)instance);
        }
        if (retrievalCollection.Any()) 
        { 
            services.AddSingleton(retrievalCollection);
        }
               
        services.AddSingleton<Kernel>();
    }
}
