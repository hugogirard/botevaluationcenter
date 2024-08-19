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
        private readonly IChatService _chatService;

        public KnowledgeBot(IChatService chatService,
                            ILogger<KnowledgeBot> logger)
        {
            _logger = logger;
            _chatHistories = new Dictionary<string,ChatHistory>();
            _chatService = chatService;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var question = turnContext.Activity.Text;

            string memberId = turnContext.Activity.Recipient.Id;

            var chatAnswer = await _chatService.GetCompletionAsync(question, turnContext);

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
