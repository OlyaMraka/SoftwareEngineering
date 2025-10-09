using FluentResults;
using KeyKeepers.BLL.Constants;
using KeyKeepers.BLL.DTOs.Users;
using KeyKeepers.BLL.Interfaces;
using KeyKeepers.DAL.Entities;
using KeyKeepers.DAL.Repositories.Interfaces.Base;
using KeyKeepers.DAL.Repositories.Options;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace KeyKeepers.BLL.Commands.Users.LogIn;

public class UserLogInHandler : IRequestHandler<UserLogInCommand, Result<UserLogInResponseDto>>
{
    private readonly ITokenService tokenService;
    private readonly IRepositoryWrapper repositoryWrapper;
    private readonly IPasswordHasher<User> passwordHasher;

    public UserLogInHandler(
        ITokenService tokenServiceObj,
        IRepositoryWrapper repositoryWrapperObj,
        IPasswordHasher<User> passwordHasherObj)
    {
        tokenService = tokenServiceObj;
        repositoryWrapper = repositoryWrapperObj;
        passwordHasher = passwordHasherObj;
    }

    public async Task<Result<UserLogInResponseDto>> Handle(UserLogInCommand request, CancellationToken cancellationToken)
    {
        try
        {
            User? user = await repositoryWrapper.UserRepository.GetFirstOrDefaultAsync(new QueryOptions<User>()
            {
                Filter = user => user.UserName == request.UserLogInDto.Username,
            });

            if (user == null)
            {
                return Result.Fail<UserLogInResponseDto>(UserConstants.UserLogInError);
            }

            var verificationResult =
                passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.UserLogInDto.Password);

            if (verificationResult == PasswordVerificationResult.Failed)
            {
                return Result.Fail<UserLogInResponseDto>(UserConstants.UserLogInError);
            }

            var accessToken = tokenService.GenerateJwtToken(user);

            var responseDto = new UserLogInResponseDto
            {
                Id = user.Id,
                AccessToken = accessToken,
            };

            return Result.Ok(responseDto);
        }
        catch
        {
            return Result.Fail<UserLogInResponseDto>(UserConstants.UserLogInError);
        }
    }
}
