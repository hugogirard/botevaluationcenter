using KnowledgeBot.Models;
using Microsoft.Bot.Builder;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Linq;
using System.Threading;
using System;
using Microsoft.SemanticKernel.ChatCompletion;
using KnowledgeBot.Plugins;

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
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
            MaxTokens = 2000,
            Temperature = 0.7,
            ChatSystemPrompt = _systemPromptPlugin
        };

        history.AddUserMessage(question);

        var response = await _chat.GetChatMessageContentAsync(history, openAIPromptExecutionSettings, _kernel);

        string answer = response.Items[0].ToString();
        
        // Validate function call


        if (answer.ToLower().Contains("i don't know"))
        {            
            // For now return I don't know but more to come
            return "I don't have this answer, an agent will comeback to you soon";
        }

        return answer;
    }

}
