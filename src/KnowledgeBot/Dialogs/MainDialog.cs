using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Text.RegularExpressions;
using System.Threading;

namespace KnowledgeBot.Dialogs;

public class MainDialog : ComponentDialog
{
    private readonly ILogger<MainDialog> _logger;
    private readonly IStateService _stateService;

    public MainDialog(ILogger<MainDialog> logger, 
                      GreetingDialog greetingDialog,
                      KnowledgeDialog knowledgeDialog,
                      ExtendedSearchDialog extendedSearchDialog,
                      IStateService stateService) : base(nameof(MainDialog))
    {
        _logger = logger;        
        _stateService = stateService;

        var waterfallSteps = new WaterfallStep[]
        {
            InitialStepAsync,
            FindAnswerKnowledgeBase,
            EvaluateAnswerKnowledgeBase,
            FinalStepAsync
        };

        AddDialog(new TextPrompt(nameof(TextPrompt)));
        AddDialog(greetingDialog);
        AddDialog(knowledgeDialog);
        AddDialog(extendedSearchDialog);
        AddDialog(new WaterfallDialog($"{nameof(MainDialog)}.mainFlow", waterfallSteps));

        InitialDialogId = $"{nameof(MainDialog)}.mainFlow";
    }

    private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {         
        return await stepContext.BeginDialogAsync(nameof(GreetingDialog), null, cancellationToken);
    }

    private async Task<DialogTurnResult> FindAnswerKnowledgeBase(WaterfallStepContext stepContext, CancellationToken cancellationToken) 
    {
        return await stepContext.BeginDialogAsync(nameof(KnowledgeDialog), null, cancellationToken);        
    }

    private async Task<DialogTurnResult> EvaluateAnswerKnowledgeBase(WaterfallStepContext stepContext, CancellationToken cancellationToken) 
    {
        var data = await _stateService.ConversationDataAccessor.GetAsync(stepContext.Context);

        if (data.FoundInKnowledgeBase)
        {
            var promptMessage = MessageFactory.Text(data.Answer);
            await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            return await stepContext.NextAsync(null, cancellationToken);
        }
        else
        {
            var promptMessage = MessageFactory.Text("Cannot find answer in our knowledge base, searching in extended source...");
            await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            
            return await stepContext.BeginDialogAsync(nameof(ExtendedSearchDialog), null, cancellationToken);
        }
    }

    private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        return await stepContext.EndDialogAsync(null, cancellationToken);
    }

}
