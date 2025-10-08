using KeyKeepers.DAL.Data;
using KeyKeepers.DAL.Entities;
using KeyKeepers.DAL.Repositories.Interfaces.RefreshTokens;
using KeyKeepers.DAL.Repositories.Realizations.Base;

namespace KeyKeepers.DAL.Repositories.Realizations.RefreshTokens;

public class RefreshTokenRepository : RepositoryBase<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(KeyKeepersDbContext context)
        : base(context)
    {
    }
}
