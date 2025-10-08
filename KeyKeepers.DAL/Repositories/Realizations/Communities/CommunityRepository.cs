using KeyKeepers.DAL.Data;
using KeyKeepers.DAL.Entities;
using KeyKeepers.DAL.Repositories.Interfaces.Communities;
using KeyKeepers.DAL.Repositories.Realizations.Base;

namespace KeyKeepers.DAL.Repositories.Realizations.Communities;

public class CommunityRepository : RepositoryBase<Community>, ICommunityRepository
{
    public CommunityRepository(KeyKeepersDbContext context)
        : base(context)
    {
    }
}
