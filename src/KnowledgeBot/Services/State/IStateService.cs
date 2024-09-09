using Microsoft.Bot.Builder;

namespace KnowledgeBot.Services.State
{
    public interface IStateService
    {
        IStatePropertyAccessor<Message> MessageAccessor { get; set; }

        IStatePropertyAccessor<DialogState> DialogStateAccessor { get; set; }

        IStatePropertyAccessor<Session> SessionAccessor { get; set; }

        UserState UserState { get; }

        Task SaveSessionAsync(Session session);

        Task SaveMessageAsync(Message message);
    }
}