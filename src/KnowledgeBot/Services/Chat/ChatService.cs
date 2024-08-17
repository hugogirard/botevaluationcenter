using KnowledgeBot.Models;
using Microsoft.Bot.Builder;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Linq;
using System.Threading;
using System;

namespace KnowledgeBot.Services.Chat;

public class ChatService : IChatService
{
    private readonly Kernel _kernel;
    private readonly KernelFunction _chat;
    private readonly KnowledgeBaseConfiguration _knowledgeBaseConfiguration;
    private readonly IKnowledgeBaseService _knowledgeBaseService;

    public ChatService(Kernel kernel,
                       IKnowledgeBaseService knowledgeBaseService,
                       KnowledgeBaseConfiguration knowledgeBaseConfiguration)
    {
        _kernel = kernel;

        var skPrompt = @"You are an intelligent assistant helping employees with their questions.
                         Answer the following question using only the data provided in the context below.           
                         If you cannot answer using the sources below, say you don't know. Use below example to answer 
                         {{$context}}

                         User: {{$userInput}}
                         ChatBot:"
        ;


        _knowledgeBaseService = knowledgeBaseService;
        _knowledgeBaseConfiguration = knowledgeBaseConfiguration;

        OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new()
        {
            MaxTokens = 2000,
            Temperature = 0.7,
        };

        _kernel = kernel;
        _chat = kernel.CreateFunctionFromPrompt(skPrompt, openAIPromptExecutionSettings);
    }


    public async Task<string> GetCompletionAsync(string question)
    {

        _knowledgeBaseConfiguration.LoadConfiguration();
        string msgKb = string.Empty;
        var history = "";
        var arguments = new KernelArguments();
        arguments["userInput"] = question;

        bool answerFound = false;
        foreach (var kb in _knowledgeBaseConfiguration.KnowledgeConfiguration)
        {
            msgKb = $"Searching in {kb.displayName} ...";
            //await turnContext.SendActivityAsync(MessageFactory.Text(msgKb), cancellationToken);

            var answers = await _knowledgeBaseService.GetAnswersAsync(question, kb.name);

            if (answers.Any())
            {
                string context = string.Join(Environment.NewLine, answers);
                arguments["context"] = context;
                answerFound = true;
                return context;
              
            }
        }

        string chatAnswer = string.Empty;
        if (answerFound)
        {
            var response = await _chat.InvokeAsync(_kernel, arguments);
            chatAnswer = response.ToString();
        }
        else
        {
            chatAnswer = "I don't know";
        }

        return chatAnswer;

    }

}
