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
    public class KnowledgeBot<T> : DialogBot<T> where T: Dialog
    {
        private readonly IStateService _stateService;

        public KnowledgeBot(IStateService stateService, T dialog, ILogger<DialogBot<T>> logger) : base(stateService, dialog, logger)
        {
            _stateService = stateService;
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {            
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await Dialog.RunAsync(turnContext, _stateService.ConversationState.CreateProperty<DialogState>("DialogState"), cancellationToken);
                }
            }
        }
    }
}
