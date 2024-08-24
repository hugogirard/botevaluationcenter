using Microsoft.Bot.Builder;
using System;
using System.Threading;

namespace KnowledgeBot.Dialogs;

public class ExtendedSearchDialog : ComponentDialog
{
    private readonly IStateService _stateService;
    private readonly ILogger<ExtendedSearchDialog> _logger;
    private readonly IChatService _chatService;

    public ExtendedSearchDialog(IStateService stateService, 
                                ILogger<ExtendedSearchDialog> logger,
                                IChatService chatService) : base(nameof(ExtendedSearchDialog))
    {
        _stateService = stateService;
        _logger = logger;
        _chatService = chatService;

        InitializeWaterfall();
    }

    private void InitializeWaterfall()
    {
        var waterfallStreps = new WaterfallStep[]
        {
                SearchExtendedSource,
                FinalStepAsync
        };

        AddDialog(new TextPrompt($"{nameof(ExtendedSearchDialog)}.message"));
        AddDialog(new WaterfallDialog($"{nameof(ExtendedSearchDialog)}.mainFlow", waterfallStreps));

        InitialDialogId = $"{nameof(ExtendedSearchDialog)}.mainFlow";
    }

    private async Task<DialogTurnResult> SearchExtendedSource(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var data = await _stateService.ConversationDataAccessor.GetAsync(stepContext.Context);

        var answer = await _chatService.GetAnswerFromExtendedSourceAsync(data.Question);

        if (!string.IsNullOrEmpty(answer))
        {
            data.Answer = answer;
            data.FoundInExtendedSource = true;

            await _stateService.ConversationDataAccessor.SetAsync(stepContext.Context, data);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = MessageFactory.Text(answer) }, cancellationToken);
        }

        return await stepContext.EndDialogAsync(null, cancellationToken);
    }

    private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        return await stepContext.EndDialogAsync(null, cancellationToken);
    }
}
