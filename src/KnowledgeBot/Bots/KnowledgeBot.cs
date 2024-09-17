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
using Microsoft.Extensions.Configuration;

namespace KnowledgeBot.Bots
{
    public class KnowledgeBot<T> : DialogBot<T> where T: Dialog
    {
        private readonly IStateService _stateService;
        private readonly string _defaultUsername;
        private readonly string _defaultMemberAadObjectId;

        public KnowledgeBot(IConfiguration configuration,
                            IStateService stateService, 
                            T dialog, 
                            ILogger<DialogBot<T>> logger) : base(stateService, dialog, logger)
        {
            _stateService = stateService;
            _defaultUsername = configuration["DefaultUsername"] ?? "TestUser";
            _defaultMemberAadObjectId = configuration["DefaultMemberAadObjectId"];
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {            
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    // Create a new session
                    var session = new Session();
                    session.ConversationId = turnContext.Activity.Conversation.Id;

                    await _stateService.SessionAccessor.SetAsync(turnContext, session, cancellationToken);

                    await Dialog.RunAsync(turnContext, _stateService.UserState.CreateProperty<DialogState>("DialogState"), cancellationToken);
                }
            }
        }
    }
}
