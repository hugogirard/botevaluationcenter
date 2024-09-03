using Microsoft.Bot.Builder;

namespace KnowledgeBot.Services.State;

public class StateService : IStateService
{
    private readonly ConversationState _conversationState;
    private readonly DialogState _dialogState;

    public ConversationData ConversationData { get; }

    public ConversationState ConversationState => _conversationState;

    public IStatePropertyAccessor<ConversationData> ConversationDataAccessor { get; set; }

    public IStatePropertyAccessor<Session> SessionAccessor { get; set; }

    public IStatePropertyAccessor<DialogState> DialogStateAccessor { get; set; }

    private readonly string PrivateConversationStateId = $"{nameof(StateService)}.PrivateConversationState";

    private readonly string SessionConversationStateId = $"{nameof(StateService)}.SessionConversationState";

    public StateService(ConversationState conversationState)
    {
        _conversationState = conversationState;        
        InitializeAccessor();
    }

    private void InitializeAccessor()
    {
        ConversationDataAccessor = _conversationState.CreateProperty<ConversationData>(PrivateConversationStateId);
        DialogStateAccessor = _conversationState.CreateProperty<DialogState>("DialogState");
        SessionAccessor = _conversationState.CreateProperty<Session>(SessionConversationStateId);
    }

}
