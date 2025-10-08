using KeyKeepers.DAL.Data;
using KeyKeepers.DAL.Entities;
using KeyKeepers.DAL.Repositories.Interfaces.Users;
using KeyKeepers.DAL.Repositories.Realizations.Base;

namespace KeyKeepers.DAL.Repositories.Realizations.Users;

public class UsersRepository : RepositoryBase<User>, IUserRepository
{
    public UsersRepository(KeyKeepersDbContext context)
        : base(context)
    {
    }
}
