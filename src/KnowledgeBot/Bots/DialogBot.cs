using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Threading;

namespace KnowledgeBot.Bots;

public class DialogBot<T>: ActivityHandler where T : Dialog
{
    private readonly IStateService _stateService;
    protected readonly T Dialog;
    private readonly ILogger<DialogBot<T>> _logger;

    public DialogBot(IStateService stateService, T dialog, ILogger<DialogBot<T>> logger)
    {
        _stateService = stateService;
        Dialog = dialog;
        _logger = logger;
    }

    public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
    {
        await base.OnTurnAsync(turnContext, cancellationToken);

        await _stateService.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
    }

    protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
    {
        await Dialog.RunAsync(turnContext, _stateService.DialogStateAccessor, cancellationToken);
    }
}
