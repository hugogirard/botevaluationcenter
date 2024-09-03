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

        public GreetingDialog(ILogger<GreetingDialog> logger, IStateService stateService) : base(nameof(GreetingDialog))
        {
            _logger = logger;
            _stateService = stateService;

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
            var data = await _stateService.ConversationDataAccessor.GetAsync(stepContext.Context, () => new ConversationData());

            if (string.IsNullOrEmpty(data.Question))
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
            var data = await _stateService.ConversationDataAccessor.GetAsync(stepContext.Context, () => new ConversationData());

            if (string.IsNullOrEmpty(data.Question))
            {
                data.Question = (string)stepContext.Result;

                await _stateService.ConversationDataAccessor.SetAsync(stepContext.Context, data);

                var promptMessage = MessageFactory.Text("Searching for an answer in our internal knowledge databases");
                //await stepContext.PromptAsync($"{nameof(GreetingDialog)}.question", new PromptOptions { Prompt = promptMessage }, cancellationToken);
                //promptMessage = MessageFactory.Text("This can take some times...");
                await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
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
