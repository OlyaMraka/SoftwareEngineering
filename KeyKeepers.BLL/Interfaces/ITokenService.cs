using KeyKeepers.DAL.Entities;

namespace KeyKeepers.BLL.Interfaces;

public interface ITokenService
{
    string GenerateJwtToken(User user);

    RefreshToken GenerateRefreshToken(User user);

    Task RevokeRefreshToken(RefreshToken token);
}
