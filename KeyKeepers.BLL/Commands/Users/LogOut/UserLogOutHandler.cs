using FluentResults;
using KeyKeepers.BLL.Constants;
using KeyKeepers.DAL.Entities;
using KeyKeepers.DAL.Repositories.Interfaces.Base;
using KeyKeepers.DAL.Repositories.Options;
using MediatR;

namespace KeyKeepers.BLL.Commands.Users.LogOut;

public class UserLogOutHandler : IRequestHandler<UserLogOutCommand, Result>
{
    private readonly IRepositoryWrapper repositoryWrapper;

    public UserLogOutHandler(IRepositoryWrapper repositoryWrapperObj)
    {
        repositoryWrapper = repositoryWrapperObj;
    }

    public async Task<Result> Handle(UserLogOutCommand request, CancellationToken cancellationToken)
    {
        RefreshToken? token = await repositoryWrapper.RefreshTokenRepository.GetFirstOrDefaultAsync(
            new QueryOptions<RefreshToken>()
            {
                Filter = token => token.Token == request.UserLogOutDto.RefreshToken,
            });

        if (token == null)
        {
            return Result.Fail(UserConstants.UserLogOutError);
        }

        repositoryWrapper.RefreshTokenRepository.Delete(token);

        if (await repositoryWrapper.SaveChangesAsync() <= 0)
        {
            return Result.Fail(UserConstants.UserLogOutError);
        }

        return Result.Ok();
    }
}
