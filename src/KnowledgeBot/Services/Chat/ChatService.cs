using KnowledgeBot.Models;
using Microsoft.Bot.Builder;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Linq;
using System.Threading;
using System;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;
using Microsoft.SemanticKernel;
using System.Text.Json;
using Azure.AI.OpenAI;
using System.Net.NetworkInformation;
using KnowledgeBot.KnowledgeBase;
using Microsoft.Bot.Schema;

namespace KnowledgeBot.Services.Chat;

public class ChatService : IChatService
{
    private readonly Kernel _kernel;
    private readonly ILogger<ChatService> _logger;
    private readonly RetrievalServiceCollection _retrievalServiceCollection;
    private readonly KnowledgeBaseCollection _knowledgeBaseCollection;
    private readonly string _systemPromptKB;
    private readonly string _systemPrompt;
    private readonly IChatCompletionService _chat;
    private readonly string _systemPromptPlugin;


    public ChatService(Kernel kernel,
                       RetrievalServiceCollection retrievalServiceCollection,
                       KnowledgeBaseCollection knowledgeBaseCollection,
                       ILogger<ChatService> logger,
                       IChatCompletionService chat)
    {
        _kernel = kernel;
        _logger = logger;
        _retrievalServiceCollection = retrievalServiceCollection;
        _knowledgeBaseCollection = knowledgeBaseCollection;

        _systemPrompt = @"You are an intelligent assistant helping employees with their questions.
                          Answer ONLY with the facts listed in the list of sources below.
                          Don't provide the source of the information in the answer, only the fact.
                          If there isn't enough information below, say you don't know in the answer and nothing else. Do not generate answers that don't use the sources below. If asking a clarifying question to the user would help, ask the question.
                          Each source has a name followed by colon and the actual information.

                          sources: 
                          -----------
                          ";

        _kernel = kernel;
        _chat = chat;
    }

    public async Task<KnowledgeBaseResponse> GetAnswerFromKnowledgeBaseAsync(string question)
    {
        var history = new ChatHistory();

        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        {            
            MaxTokens = 2000,
            Temperature = 0.7,
        };

        try
        {
            // Loop all the KBs
            foreach (var kb in _knowledgeBaseCollection.GetKnowledgeBases())
            {
          
                var answers = await kb.Value.GetAnswerKB(question);
                if (answers.Any() && !answers.First().Contains("NA"))
                {
                    string context = string.Join(Environment.NewLine, answers);
                    var skPrompt = $"{_systemPrompt}{context}";
                        //_systemPrompt.Replace("{{$context}}", context);
                    history.AddSystemMessage(skPrompt);
                    history.AddUserMessage(question);

                    var response = await _chat.GetChatMessageContentAsync(history, openAIPromptExecutionSettings, _kernel);

                    history.AddAssistantMessage(response.Items[0].ToString());
                    return new KnowledgeBaseResponse(kb.Key, response.Items[0].ToString());
                }
            }

            return new KnowledgeBaseResponse(string.Empty,string.Empty);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return new KnowledgeBaseResponse(string.Empty, 
                                             "Oh no, our bot is out of office, an agent will comeback to you soon", 
                                             true);            
        }
    }

    public async Task<RetrievalPluginResponse> GetAnswerFromExtendedSourceAsync(string question)
    {
        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        {
            MaxTokens = 2000,
            Temperature = 1,
        };

        var history = new ChatHistory();

        foreach (var retrieval in _retrievalServiceCollection.GetRetrivalService())
        {
            var answers = await retrieval.Value.GetAnswersAsync(question);

            if (answers.Any())
            {
                string context = string.Join(Environment.NewLine, answers);
                var skPrompt = $"{_systemPrompt}{context}";
                //var skPrompt = _systemPrompt.Replace("{{$context}}", context);

                history.AddSystemMessage(skPrompt);
                history.AddUserMessage(question);
                
                var response = await _chat.GetChatMessageContentAsync(history, openAIPromptExecutionSettings, _kernel);

                history.AddAssistantMessage(response.Items[0].ToString());

                return new RetrievalPluginResponse(retrieval.Key,response.Items[0].ToString());
            }
        }

        return new RetrievalPluginResponse(string.Empty,string.Empty);
    }
}
