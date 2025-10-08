using KeyKeepers.DAL.Data;
using KeyKeepers.DAL.Entities;
using KeyKeepers.DAL.Repositories.Interfaces.CommunityUsers;
using KeyKeepers.DAL.Repositories.Realizations.Base;

namespace KeyKeepers.DAL.Repositories.Realizations.CommunityUsers;

public class CommunityUserRepository : RepositoryBase<CommunityUser>, ICommunityUserRepository
{
    public CommunityUserRepository(KeyKeepersDbContext context)
        : base(context)
    {
    }
}
