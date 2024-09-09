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
                SearchExtendedSource                
        };

        AddDialog(new WaterfallDialog(nameof(ExtendedSearchDialog), waterfallStreps));

        InitialDialogId = nameof(ExtendedSearchDialog);
    }

    private async Task<DialogTurnResult> SearchExtendedSource(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var message = await _stateService.MessageAccessor.GetAsync(stepContext.Context);

        var response = await _chatService.GetAnswerFromExtendedSourceAsync(message.Prompt);

        if (!string.IsNullOrEmpty(response.Answer))
        {
            message.Completion = response.Answer;
            message.FoundInRetrieval = true;
            message.RetrievalPluginName = response.RetrievalPluginName;

            await _stateService.SaveMessageAsync(message);

            await _stateService.MessageAccessor.SetAsync(stepContext.Context, message);    
        }

        return await stepContext.EndDialogAsync(null, cancellationToken);
    }
}
