using Microsoft.Bot.Builder;

namespace KnowledgeBot.Services.State
{
    public interface IStateService
    {
        ConversationData ConversationData { get; }
        IStatePropertyAccessor<ConversationData> ConversationDataAccessor { get; set; }
        
        IStatePropertyAccessor<DialogState> DialogStateAccessor { get; set; }

        IStatePropertyAccessor<Session> SessionAccessor { get; set; }

        ConversationState ConversationState { get; }
    }
}