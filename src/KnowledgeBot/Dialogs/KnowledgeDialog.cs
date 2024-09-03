using Microsoft.Bot.Builder;
using System.Threading;

namespace KnowledgeBot.Dialogs;

public class KnowledgeDialog : ComponentDialog
{
    private readonly ILogger<KnowledgeDialog> _logger;
    private readonly IChatService _chatService;
    private readonly IStateService _stateService;

    public KnowledgeDialog(ILogger<KnowledgeDialog> logger, 
                           IChatService chatService, 
                           IStateService stateService) : base(nameof(KnowledgeDialog))
    {
        _logger = logger;
        _chatService = chatService;
        _stateService = stateService;

        InitializeWaterfall();
    }

    private void InitializeWaterfall() 
    {
        var waterfallStreps = new WaterfallStep[]
        {
                SearchInKnowledgeBase,
                FinalStepAsync
        };

        AddDialog(new TextPrompt(nameof(TextPrompt)));
        AddDialog(new WaterfallDialog(nameof(KnowledgeDialog), waterfallStreps));

        InitialDialogId = nameof(KnowledgeDialog);
    }

    private async Task<DialogTurnResult> SearchInKnowledgeBase(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        var data = await _stateService.ConversationDataAccessor.GetAsync(stepContext.Context, () => new ConversationData());

        var chatAnswer = await _chatService.GetAnswerFromKnowledgeBaseAsync(data.Question);

        if (!string.IsNullOrEmpty(chatAnswer))
        {
            //var promptMessage = MessageFactory.Text(chatAnswer);
            data.Answer = chatAnswer;
            data.FoundInKnowledgeBase = true;

            await _stateService.ConversationDataAccessor.SetAsync(stepContext.Context, data);
            return await stepContext.NextAsync(null, cancellationToken);
            //return await stepContext.PromptAsync($"{nameof(KnowledgeDialog)}.message", new PromptOptions { Prompt = promptMessage }, cancellationToken);
        }
        else 
        {
            data.FoundInKnowledgeBase = false;
            await _stateService.ConversationDataAccessor.SetAsync(stepContext.Context, data);
            return await stepContext.NextAsync(null, cancellationToken);
        }
    }

    private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
    {
        return await stepContext.EndDialogAsync(null, cancellationToken);
    }
}
