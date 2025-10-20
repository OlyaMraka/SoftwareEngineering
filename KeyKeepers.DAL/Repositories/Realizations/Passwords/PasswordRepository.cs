using KeyKeepers.DAL.Data;
using KeyKeepers.DAL.Entities;
using KeyKeepers.DAL.Repositories.Realizations.Base;
using KeyKeepers.DAL.Repositories.Interfaces.Passwords;

namespace KeyKeepers.DAL.Repositories.Realizations.Passwords;

public class PasswordRepository : RepositoryBase<Credentials>, IPasswordRepository
{
    public PasswordRepository(KeyKeepersDbContext context)
        : base(context)
    {
    }
}
