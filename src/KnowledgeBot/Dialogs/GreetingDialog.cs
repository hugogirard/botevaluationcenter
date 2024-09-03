using Microsoft.Bot.Builder;    
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Threading;

namespace KnowledgeBot.Dialogs
{
    public class GreetingDialog :ComponentDialog
    {
        private readonly ILogger<GreetingDialog> _logger;
        private readonly IStateService _stateService;
        private readonly ICosmosDbRepository _cosmosDbRepository;

        public GreetingDialog(ILogger<GreetingDialog> logger,
                              ICosmosDbRepository cosmosDbRepository,
                              IStateService stateService) : base(nameof(GreetingDialog))
        {
            _logger = logger;
            _stateService = stateService;
            _cosmosDbRepository = cosmosDbRepository;

            InitializeWaterfallDialog();
        }

        private void InitializeWaterfallDialog() 
        {
            var waterfallStreps = new WaterfallStep[]
            {
                InitialStepAsync,
                AckknowledgeMessageAsync,
                FinalStepAsync
            };

            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new WaterfallDialog(nameof(GreetingDialog), waterfallStreps));

            InitialDialogId = nameof(GreetingDialog);
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {            
            var message = await _stateService.MessageAccessor.GetAsync(stepContext.Context, () => new Message());

            if (string.IsNullOrEmpty(message.Prompt))
            {
                string msg = "Hi, I am a knowledge bot assistant, please ask me a question";
                var promptMessage = MessageFactory.Text(msg,msg,InputHints.ExpectingInput);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }
            else 
            {
                return await stepContext.NextAsync(null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> AckknowledgeMessageAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken) 
        {
            var session = await _stateService.SessionAccessor.GetAsync(stepContext.Context);

            var message = await _stateService.MessageAccessor.GetAsync(stepContext.Context, () => new Message 
            {
                MemberId = session.MemberId,
                SessionId = session.SessionId
            });

            if (string.IsNullOrEmpty(message.Prompt))
            {
                message.Prompt = (string)stepContext.Result;

                await _stateService.MessageAccessor.SetAsync(stepContext.Context, message);

                // Since a question was asked we can save the Session in CosmosDB and the
                // and a message object too
                await _stateService.SaveSessionAsync(session);
                await _stateService.SaveMessageAsync(message);

                var promptMessage = MessageFactory.Text("Searching for an answer in our internal knowledge databases", inputHint: InputHints.IgnoringInput); 
                await stepContext.Context.SendActivityAsync(promptMessage, cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);                
            }

            return await stepContext.NextAsync(null, cancellationToken);
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
