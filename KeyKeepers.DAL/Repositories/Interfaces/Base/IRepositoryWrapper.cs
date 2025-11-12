using KeyKeepers.DAL.Repositories.Interfaces.Communities;
using KeyKeepers.DAL.Repositories.Interfaces.CommunityUsers;
using KeyKeepers.DAL.Repositories.Interfaces.JoinRequests;
using KeyKeepers.DAL.Repositories.Interfaces.PasswordCategories;
using KeyKeepers.DAL.Repositories.Interfaces.Passwords;
using KeyKeepers.DAL.Repositories.Interfaces.RefreshTokens;
using KeyKeepers.DAL.Repositories.Interfaces.Users;

namespace KeyKeepers.DAL.Repositories.Interfaces.Base;

public interface IRepositoryWrapper
{
    ICommunityUserRepository CommunityUserRepository { get; }

    ICommunityRepository CommunityRepository { get; }

    IUserRepository UserRepository { get; }

    IRefreshTokenRepository RefreshTokenRepository { get; }

    IPasswordCategoryRepository PrivatePasswordCategoryRepository { get; }

    IPasswordRepository PasswordRepository { get; }

    IJoinRequestRepository JoinRequestRepository { get; }

    int SaveChanges();

    Task<int> SaveChangesAsync();

    void ClearChangeTracker();
}
