using Microsoft.Bot.Builder;

namespace KnowledgeBot.Services.State;

public class StateService : IStateService
{
    private readonly UserState _userState;
    private readonly ICosmosDbRepository _cosmosDbRepository;
    private readonly ILogger<StateService> _logger;
    private readonly DialogState _dialogState;

    public UserState UserState => _userState;

    public IStatePropertyAccessor<Message> MessageAccessor { get; set; }

    public IStatePropertyAccessor<Session> SessionAccessor { get; set; }

    public IStatePropertyAccessor<DialogState> DialogStateAccessor { get; set; }

    private readonly string PrivateConversationStateId = $"{nameof(StateService)}.MessageConversationState";

    private readonly string SessionConversationStateId = $"{nameof(StateService)}.SessionConversationState";

    public StateService(UserState conversationState,
                        ILogger<StateService> logger,
                        ICosmosDbRepository cosmosDbRepository)
    {
        _userState = conversationState;
        _cosmosDbRepository = cosmosDbRepository;
        _logger = logger;

        InitializeAccessor();
    }

    private void InitializeAccessor()
    {
        MessageAccessor = _userState.CreateProperty<Message>(PrivateConversationStateId);
        DialogStateAccessor = _userState.CreateProperty<DialogState>("DialogState");
        SessionAccessor = _userState.CreateProperty<Session>(SessionConversationStateId);
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
