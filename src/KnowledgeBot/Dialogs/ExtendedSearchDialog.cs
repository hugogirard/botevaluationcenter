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

        AddDialog(new WaterfallDialog(nameof(ExtendedSearchDialog), waterfallStreps));

        InitialDialogId = $"{nameof(ExtendedSearchDialog)}.mainFlow";
    }

    private async Task<DialogTurnResult> SearchExtendedSource(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var message = await _stateService.MessageAccessor.GetAsync(stepContext.Context);

        var answer = await _chatService.GetAnswerFromExtendedSourceAsync(message.Prompt);

        if (!string.IsNullOrEmpty(answer))
        {
            message.Completion = answer;
            message.FoundInRetrieval = true;

            await _stateService.MessageAccessor.SetAsync(stepContext.Context, message);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        return await stepContext.EndDialogAsync(null, cancellationToken);
    }

    private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        return await stepContext.EndDialogAsync(null, cancellationToken);
    }
}
