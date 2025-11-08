using KeyKeepers.DAL.Data;
using KeyKeepers.DAL.Entities;
using KeyKeepers.DAL.Repositories.Interfaces.JoinRequests;
using KeyKeepers.DAL.Repositories.Realizations.Base;

namespace KeyKeepers.DAL.Repositories.Realizations.JoinRequests;

public class JoinRequestRepository : RepositoryBase<JoinRequest>, IJoinRequestRepository
{
    public JoinRequestRepository(KeyKeepersDbContext context)
        : base(context)
    {
    }
}
