using Microsoft.Bot.Builder;

namespace KnowledgeBot.Services.State;

public class StateService : IStateService
{
    private readonly ConversationState _conversationState;
    private readonly ICosmosDbRepository _cosmosDbRepository;
    private readonly ILogger<StateService> _logger;
    private readonly DialogState _dialogState;

    public ConversationState ConversationState => _conversationState;

    public IStatePropertyAccessor<Message> MessageAccessor { get; set; }

    public IStatePropertyAccessor<Session> SessionAccessor { get; set; }

    public IStatePropertyAccessor<DialogState> DialogStateAccessor { get; set; }

    private readonly string PrivateConversationStateId = $"{nameof(StateService)}.MessageConversationState";

    private readonly string SessionConversationStateId = $"{nameof(StateService)}.SessionConversationState";

    public StateService(ConversationState conversationState,
                        ILogger<StateService> logger,
                        ICosmosDbRepository cosmosDbRepository)
    {
        _conversationState = conversationState;
        _cosmosDbRepository = cosmosDbRepository;
        _logger = logger;

        InitializeAccessor();
    }

    private void InitializeAccessor()
    {
        MessageAccessor = _conversationState.CreateProperty<Message>(PrivateConversationStateId);
        DialogStateAccessor = _conversationState.CreateProperty<DialogState>("DialogState");
        SessionAccessor = _conversationState.CreateProperty<Session>(SessionConversationStateId);
    }

    public async Task SaveSessionAsync(Session session)
    {
        try
        {
            await _cosmosDbRepository.InsertAsync(session.MemberId, session);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }

    public async Task SaveMessageAsync(Message message)
    {
        try
        {
            await _cosmosDbRepository.InsertAsync(message.MemberId, message);
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, ex.Message);
        }
    }
}
