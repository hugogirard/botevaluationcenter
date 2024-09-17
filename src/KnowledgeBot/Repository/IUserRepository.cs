
namespace KnowledgeBot.Repository
{
    public interface IUserRepository
    {
        IReadOnlyList<UserInfo> Users { get; set; }

        UserInfo GetUserByEmail(string email);
    }
}