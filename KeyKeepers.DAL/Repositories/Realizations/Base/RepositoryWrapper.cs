using KeyKeepers.DAL.Data;
using KeyKeepers.DAL.Repositories.Interfaces.Base;
using KeyKeepers.DAL.Repositories.Interfaces.Communities;
using KeyKeepers.DAL.Repositories.Interfaces.CommunityUsers;
using KeyKeepers.DAL.Repositories.Interfaces.JoinRequests;
using KeyKeepers.DAL.Repositories.Interfaces.PasswordCategories;
using KeyKeepers.DAL.Repositories.Interfaces.Passwords;
using KeyKeepers.DAL.Repositories.Interfaces.RefreshTokens;
using KeyKeepers.DAL.Repositories.Interfaces.Users;
using KeyKeepers.DAL.Repositories.Realizations.Communities;
using KeyKeepers.DAL.Repositories.Realizations.CommunityUsers;
using KeyKeepers.DAL.Repositories.Realizations.JoinRequests;
using KeyKeepers.DAL.Repositories.Realizations.PasswordCategories;
using KeyKeepers.DAL.Repositories.Realizations.Passwords;
using KeyKeepers.DAL.Repositories.Realizations.RefreshTokens;
using KeyKeepers.DAL.Repositories.Realizations.Users;

namespace KeyKeepers.DAL.Repositories.Realizations.Base;

public class RepositoryWrapper : IRepositoryWrapper
{
    private readonly KeyKeepersDbContext dbContext;

    private ICommunityRepository? communityRepository;
    private IUserRepository? userRepository;
    private IRefreshTokenRepository? refreshTokenRepository;
    private ICommunityUserRepository? communityUserRepository;
    private IPasswordCategoryRepository? passwordCategoryRepository;
    private IPasswordRepository? passwordRepository;
    private IJoinRequestRepository? joinRequestRepository;

    public RepositoryWrapper(KeyKeepersDbContext context)
    {
        dbContext = context;
    }

    public ICommunityRepository CommunityRepository => communityRepository ??= new CommunityRepository(dbContext);

    public IUserRepository UserRepository => userRepository ??= new UsersRepository(dbContext);

    public IRefreshTokenRepository RefreshTokenRepository => refreshTokenRepository
        ??= new RefreshTokenRepository(dbContext);

    public ICommunityUserRepository CommunityUserRepository => communityUserRepository
        ??= new CommunityUserRepository(dbContext);

    public IPasswordCategoryRepository PrivatePasswordCategoryRepository => passwordCategoryRepository
        ??= new PasswordCategoryRepository(dbContext);

    public IPasswordRepository PasswordRepository => passwordRepository
        ??= new PasswordRepository(dbContext);

    public IJoinRequestRepository JoinRequestRepository => joinRequestRepository
        ??= new JoinRequestRepository(dbContext);

    public int SaveChanges()
    {
        return dbContext.SaveChanges();
    }

    public async Task<int> SaveChangesAsync()
    {
        return await dbContext.SaveChangesAsync();
    }

    public void ClearChangeTracker()
    {
        dbContext.ChangeTracker.Clear();
    }
}
