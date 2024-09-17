using System;
using System.Linq;

namespace KnowledgeBot.Repository;

public class UserRepository : IUserRepository
{
    public IReadOnlyList<UserInfo> Users { get; set; }

    public UserRepository()
    {
        Users = new List<UserInfo>
        {
            new UserInfo("d2719b1e-1c4b-4b8e-9f1e-1d2b1e1c4b8e", "John", "john@contoso.com"),
            new UserInfo("e2719b1e-1c4b-4b8e-9f1e-1d2b1e1c4b8e", "Mary", "mary@contoso.com"),
            new UserInfo("f2719b1e-1c4b-4b8e-9f1e-1d2b1e1c4b8e", "Peter", "peter@consoto.com")
        };
    }

    public UserInfo GetUserByEmail(string email)
    {
        return Users.SingleOrDefault(u => u.Email == email);
    }
}
