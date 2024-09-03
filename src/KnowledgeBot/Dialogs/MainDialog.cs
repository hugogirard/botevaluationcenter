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
                      ICosmosDbRepository cosmosDbRepository,
                      IStateService stateService) : base(nameof(MainDialog))
    {
        _logger = logger;        
        _stateService = stateService;

        var waterfallSteps = new WaterfallStep[]
        {
            InitialStepAsync,
            FindAnswerKnowledgeBase,
            EvaluateAnswerKnowledgeBase,
            EvaluateAnswerRetrieval,
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
        var message = await _stateService.MessageAccessor.GetAsync(stepContext.Context);

        if (message.FoundInKnowledgeDatabase)
        {
            var promptMessage = MessageFactory.Text(message.Completion, inputHint: InputHints.IgnoringInput);

            // Update the message in the database
            await _stateService.SaveMessageAsync(message);

            await stepContext.Context.SendActivityAsync(promptMessage, cancellationToken);
            return await stepContext.EndDialogAsync(null, cancellationToken);            
        }
        else
        {            
            var promptMessage = MessageFactory.Text("Cannot find answer in our knowledge base, searching in extended source...",
                                                    inputHint: InputHints.IgnoringInput);
            await stepContext.Context.SendActivityAsync(promptMessage, cancellationToken);
            return await stepContext.BeginDialogAsync(nameof(ExtendedSearchDialog), null, cancellationToken);
        }
    }

    private async Task<DialogTurnResult> EvaluateAnswerRetrieval(WaterfallStepContext stepContext, CancellationToken cancellationToken) 
    {
        var message = await _stateService.MessageAccessor.GetAsync(stepContext.Context);

        if (message.FoundInKnowledgeDatabase)
            return await stepContext.EndDialogAsync(null, cancellationToken);

        Activity promptMessage;

        if (message.FoundInRetrieval) 
        {
            promptMessage = MessageFactory.Text(message.Completion);
            
            // Update the message in the database
            await _stateService.SaveMessageAsync(message);

            await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        promptMessage = MessageFactory.Text("Cannot found the answer from your question, an agent will comeback to you soon",
                                            inputHint: InputHints.IgnoringInput);
        await stepContext.Context.SendActivityAsync(promptMessage, cancellationToken);

        message.QuestionNotAnswered = true;
        await _stateService.SaveMessageAsync(message);

        return await stepContext.EndDialogAsync(null, cancellationToken);
    }

    private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        return await stepContext.EndDialogAsync(null, cancellationToken);
    }

}
