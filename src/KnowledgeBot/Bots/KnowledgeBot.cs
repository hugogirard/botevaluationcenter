using Azure.AI.Language.QuestionAnswering;
using KnowledgeBot.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Identity.Client;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.SemanticKernel;

namespace KnowledgeBot.Bots
{
    public class KnowledgeBot : ActivityHandler
    {        
        private readonly ILogger<KnowledgeBot> _logger;
        private Dictionary<string,ChatHistory> _chatHistories;
        private readonly KnowledgeBaseConfiguration _knowledgeBaseConfiguration;
        private readonly IKnowledgeBaseService _knowledgeBaseService;
        private readonly string _systemPrompt;
        private readonly OpenAIPromptExecutionSettings _openAIPromptExecutionSettings;
        private readonly Kernel _kernel;
        private readonly KernelFunction _chat;

        public KnowledgeBot(Kernel kernel,
                            IKnowledgeBaseService knowledgeBaseService,
                            KnowledgeBaseConfiguration knowledgeBaseConfiguration,
                            ILogger<KnowledgeBot> logger)
        {
            _logger = logger;
            _chatHistories = new Dictionary<string,ChatHistory>();
            _knowledgeBaseConfiguration = knowledgeBaseConfiguration;
            _knowledgeBaseService = knowledgeBaseService;

             _systemPrompt = @"ChatBot can have a conversation with you about any topic.
                               You answer the question based on the context provided otherwise you say 'I don't know' if it does not have an answer.
                               {{$context}}

                               {{$history}}
                               User: {{$userInput}}
                               ChatBot:";



            _openAIPromptExecutionSettings = new()
            {
                MaxTokens = 2000,
                Temperature = 0.7,
            };

            _kernel = kernel;
            _chat = kernel.CreateFunctionFromPrompt(_systemPrompt, _openAIPromptExecutionSettings);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var question = turnContext.Activity.Text;

            string memberId = turnContext.Activity.Recipient.Id;

            // This should happen but in case we need to create the system prompt
            if (!_chatHistories.ContainsKey(memberId))
            {
                _logger.LogError($"Cannot find member in the history chat: {memberId}");
                _chatHistories.Add(memberId, new ChatHistory());
            }

            // Load all Knowledge base
            _knowledgeBaseConfiguration.LoadConfiguration();
            string msgKb = string.Empty;
            var history = "";
            var arguments = new KernelArguments()
            {
                ["history"] = history
            };
            arguments["userInput"] = question;
            bool answerFound = false;
            foreach (var kb in _knowledgeBaseConfiguration.KnowledgeConfiguration) 
            {
                msgKb = $"Searching in {kb.displayName} ...";
                await turnContext.SendActivityAsync(MessageFactory.Text(msgKb), cancellationToken);

                var answers = await _knowledgeBaseService.GetAnswersAsync(question, kb.name);

                if (answers.Any())
                {
                    string context = string.Join(Environment.NewLine, answers);
                    arguments["context"] = context;
                    answerFound = true;
                    break;
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

            await turnContext.SendActivityAsync(MessageFactory.Text(chatAnswer), cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello, please ask your question!";
            
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    _logger.LogInformation($"Member joined: {member.Id}-{member.Name}");

                    _chatHistories.Add(member.Id, new ChatHistory());
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }

        protected override async Task OnMembersRemovedAsync(IList<ChannelAccount> membersRemoved, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersRemoved)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    try
                    {
                        // We don't want to crash if we cannot remove the history
                        _chatHistories.Remove(member.Id);
                    }
                    catch
                    {                        
                    }                    
                }
            }

            await Task.FromResult(0);
        }
    }
}
