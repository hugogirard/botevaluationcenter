using KnowledgeBot.KnowledgeBase;
using KnowledgeBot.Models;
using KnowledgeBot.Options;

using KnowledgeBot.RAG;
using KnowledgeBot.Services;
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
    /// <summary>
    /// This method register Semantic Kernel and all needed plugins
    /// </summary>    
    /// 
    public static object RegisterBasePlugin(Type pluginType, IServiceProvider sp) 
    {
        var constructor = pluginType.GetConstructors().First();
        var parameters = constructor.GetParameters()
                                    .Select(p => sp.GetService(p.ParameterType))
                                    .ToArray();
        var instance = Activator.CreateInstance(pluginType, parameters);
        return instance;
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

        // Now load all plugin that come from the baseclass PluginAdapter
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
