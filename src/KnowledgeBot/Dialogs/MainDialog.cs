using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using System.Text.RegularExpressions;
using System.Threading;

namespace KnowledgeBot.Dialogs;

public class MainDialog : ComponentDialog
{
    private readonly ILogger<MainDialog> _logger;
    private readonly IStateService _stateService;

    private const string AGENT_FEEDBACK = "Cannot found the answer from your question, an agent will comeback to you soon";

    public MainDialog(ILogger<MainDialog> logger, 
                      GreetingDialog greetingDialog,
                      KnowledgeDialog knowledgeDialog,
                      ExtendedSearchDialog extendedSearchDialog,
                      ICosmosDbRepository cosmosDbRepository,
                      IStateService stateService) : base(nameof(MainDialog))
    {
        _logger = logger;        
        _stateService = stateService;

        var waterfallSteps = new WaterfallStep[]
        {
            InitialStepAsync,
            FindAnswerKnowledgeBase,
            FindAnswerRetrievalSource,
            FinalStepAsync
        };

        AddDialog(new TextPrompt(nameof(TextPrompt)));
        AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
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
    private async Task<DialogTurnResult> FindAnswerRetrievalSource(WaterfallStepContext stepContext, CancellationToken cancellationToken) 
    {
        var message = await _stateService.MessageAccessor.GetAsync(stepContext.Context);

        // Already found the answer in the KB no need to validate
        // from the retrieval
        if (message.FoundInKnowledgeDatabase)
            return await stepContext.EndDialogAsync(null, cancellationToken);

        return await stepContext.BeginDialogAsync(nameof(ExtendedSearchDialog), null, cancellationToken);
    }    
   
    private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var message = await _stateService.MessageAccessor.GetAsync(stepContext.Context);

        if (!message.FoundInKnowledgeDatabase && !message.FoundInRetrieval)
        {
            var prompt = MessageFactory.Text("Cannot find the answer to your question; an agent will get back to you soon.", inputHint: InputHints.IgnoringInput);
            await stepContext.Context.SendActivityAsync(prompt, cancellationToken);

        }
        return await stepContext.EndDialogAsync(null, cancellationToken);
    }

    //private async Task<DialogTurnResult> EndConversation(WaterfallStepContext stepContext, CancellationToken cancellationToken) 
    //{
    //    var endOfConversation = Activity.CreateEndOfConversationActivity();
    //    endOfConversation.Code = EndOfConversationCodes.CompletedSuccessfully;
    //    await stepContext.Context.SendActivityAsync(endOfConversation, cancellationToken);

    //    return await stepContext.EndDialogAsync(null, cancellationToken);        
    //}

}
