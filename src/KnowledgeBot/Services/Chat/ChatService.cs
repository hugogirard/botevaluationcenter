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
    private readonly RetrievalServiceCollection _retrievalServiceCollection;
    private readonly KnowledgeBaseCollection _knowledgeBaseCollection;
    private readonly string _systemPromptKB;
    private readonly IChatCompletionService _chat;
    private readonly string _systemPromptPlugin;


    public ChatService(Kernel kernel,
                       RetrievalServiceCollection retrievalServiceCollection,
                       KnowledgeBaseCollection knowledgeBaseCollection,
                       IChatCompletionService chat)
    {
        _kernel = kernel;
        _retrievalServiceCollection = retrievalServiceCollection;
        _knowledgeBaseCollection = knowledgeBaseCollection;

        _systemPromptKB = @"You are an intelligent assistant helping employees with their questions.
                             Answer the following question using only the data provided in the context below.           
                             Just here the context provided to answer but make the sentence better. Don't add any more information. 
                             context: {{$context}}";

        _kernel = kernel;
        _chat = chat;
    }


    public async Task<string> GetCompletionAsync(string question, ITurnContext<IMessageActivity> turnContext)
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
                    var skPrompt = _systemPromptKB.Replace("{{$context}}", context);
                    history.AddSystemMessage(skPrompt);
                    history.AddUserMessage(question);

                    var response = await _chat.GetChatMessageContentAsync(history, openAIPromptExecutionSettings, _kernel);

                    history.AddAssistantMessage(response.Items[0].ToString());

                    return response.Items[0].ToString();
                }
            }

            return "I don't know";
        }
        catch (Exception ex)
        {

            return "Oh no, our bot is out of office, an agent will comeback to you soon";
        }
    }
}
