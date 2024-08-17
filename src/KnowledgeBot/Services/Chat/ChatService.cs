using KnowledgeBot.Models;
using Microsoft.Bot.Builder;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Linq;
using System.Threading;
using System;
using Microsoft.SemanticKernel.ChatCompletion;
using KnowledgeBot.Plugins;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;
using Microsoft.SemanticKernel;
using System.Text.Json;
using Azure.AI.OpenAI;

namespace KnowledgeBot.Services.Chat;

public class ChatService : IChatService
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _chat;
    private readonly string _systemPromptPlugin;


    public ChatService(Kernel kernel,
                       IChatCompletionService chat)
    {
        _kernel = kernel;

        //var skPrompt = @"You are an intelligent assistant helping employees with their questions.
        //                 Answer the following question using only the data provided in the context below.           
        //                 If you cannot answer using the sources below, say you don't know. Use below example to answer 
        //                 {{$context}}

        //                 User: {{$userInput}}
        //                 ChatBot:";

        _systemPromptPlugin = @"You are an intelligent assistant helping employees with their questions.
                                Answer the following question using only the native function get_from_kb.   
                                If you cannot find the answer from the native function return I don't know, don't make answer";

        _kernel = kernel;
        _chat = chat;
    }


    public async Task<string> GetCompletionAsync(string question)
    {

        string msgKb = string.Empty;
        var history = new ChatHistory();

        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        {
            ToolCallBehavior = ToolCallBehavior.EnableKernelFunctions,
            MaxTokens = 2000,
            Temperature = 0.7,
            ChatSystemPrompt = _systemPromptPlugin
        };

        history.AddUserMessage(question);

        // This won't call OpenAI but determine if one plugin needs to be called
        // if it's the case we call manually, we cannot trust OpenAI even with
        // or system prompt since it can hallucinate
        var manualResult = (OpenAIChatMessageContent)await _chat.GetChatMessageContentAsync(history, openAIPromptExecutionSettings, _kernel);

        List<ChatCompletionsFunctionToolCall> toolCalls = manualResult.ToolCalls.OfType<ChatCompletionsFunctionToolCall>().ToList();
        if (toolCalls.Count == 0)
        {
            return "I don't have this answer, an agent will comeback to you soon";
        }
        history.Add(manualResult);

        bool answerFoundFromKb = false;
        foreach (var toolCall in toolCalls)
        {

            KernelFunction pluginFunction;
            KernelArguments arguments;
            _kernel.Plugins.TryGetFunctionAndArguments(toolCall, out pluginFunction, out arguments);
            var functionResult = await _kernel.InvokeAsync(pluginFunction!, arguments!);
            var jsonResponse = functionResult.GetValue<object>();
            var json = JsonSerializer.Serialize(jsonResponse);

            // This is what or Language Service return 
            // when it found nothing
            if (!json.ToLower().Contains("no answer found"))
            {
                answerFoundFromKb = true;
                history.Add(new ChatMessageContent(AuthorRole.Tool,
                                                   json,
                                                   metadata: new Dictionary<string, object?>(1) { { OpenAIChatMessageContent.ToolIdProperty, toolCall.Id } }));
            }
        }
        
        try
        {
            if (answerFoundFromKb)
            {
                var response = await _chat.GetChatMessageContentAsync(history, openAIPromptExecutionSettings, _kernel);

                // Still possible language service found some answer that make no sense
                // the ChatGTP will return I don't know
                if (response.Items[0].ToString().ToLower().Contains("i don't know")) 
                {
                    return "No answer found from our Knowledge Base, an agent will comeback to you soon";
                }

                return response.Items[0].ToString();
            }
            else 
            {
                // Do something else here
                return "No answer found from our Knowledge Base, an agent will comeback to you soon";
            }

        }
        catch (Exception ex)
        {

            return "Oh no, our bot is out of office, an agent will comeback to you soon";
        }
    }

}
