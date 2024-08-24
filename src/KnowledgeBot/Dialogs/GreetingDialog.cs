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
                FinalStepAsync
            };
            
            AddDialog(new TextPrompt($"{nameof(GreetingDialog)}.question"));
            AddDialog(new WaterfallDialog($"{nameof(GreetingDialog)}.mainFlow", waterfallStreps));

            InitialDialogId = $"{nameof(GreetingDialog)}.mainFlow";
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var data = await _stateService.ConversationDataAccessor.GetAsync(stepContext.Context, () => new ConversationData());

            if (string.IsNullOrEmpty(data.Question))
            {
                var promptMessage = MessageFactory.Text("Hi, I am a knowledge bot assistant, please ask me a question");
                return await stepContext.PromptAsync($"{nameof(GreetingDialog)}.question", new PromptOptions { Prompt = promptMessage }, cancellationToken);
            }
            else 
            {
                return await stepContext.NextAsync(null, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var data = await _stateService.ConversationDataAccessor.GetAsync(stepContext.Context, () => new ConversationData());

            if (string.IsNullOrEmpty(data.Question)) 
            {
                data.Question = (string)stepContext.Result;

                await _stateService.ConversationDataAccessor.SetAsync(stepContext.Context, data);
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Searching for an answer in our internal knowledge databases"), cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("This can take some times..."), cancellationToken);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
