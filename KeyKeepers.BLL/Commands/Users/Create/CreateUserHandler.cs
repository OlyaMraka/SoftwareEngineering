using AutoMapper;
using FluentResults;
using FluentValidation;
using KeyKeepers.BLL.Constants;
using KeyKeepers.BLL.DTOs.Users;
using KeyKeepers.BLL.Interfaces;
using KeyKeepers.DAL.Entities;
using KeyKeepers.DAL.Repositories.Interfaces.Base;
using KeyKeepers.DAL.Repositories.Options;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace KeyKeepers.BLL.Commands.Users.Create;

public class CreateUserHandler : IRequestHandler<CreateUserCommand, Result<AuthResponseDto>>
{
    private readonly IPasswordHasher<User> passwordHasher;
    private readonly IMapper mapper;
    private readonly IRepositoryWrapper repositoryWrapper;
    private readonly ITokenService tokenService;
    private readonly IValidator<CreateUserCommand> validator;

    public CreateUserHandler(
        IMapper mapperObj,
        IRepositoryWrapper repositoryWrapperObj,
        ITokenService tokenServiceObj,
        IValidator<CreateUserCommand> validatorObj,
        IPasswordHasher<User> passwordHasherObj)
    {
        mapper = mapperObj;
        repositoryWrapper = repositoryWrapperObj;
        tokenService = tokenServiceObj;
        validator = validatorObj;
        passwordHasher = passwordHasherObj;
    }

    public async Task<Result<AuthResponseDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken);

            User? existingUser = await repositoryWrapper.UserRepository.GetFirstOrDefaultAsync(new QueryOptions<User>
            {
                Filter = user =>
                    user.Email == request.RegisterDto.Email || user.UserName == request.RegisterDto.UserName,
            });

            if (existingUser != null)
            {
                return Result.Fail<AuthResponseDto>(UserConstants.UserCreationError);
            }

            User user = mapper.Map<User>(request.RegisterDto);
            user.PasswordHash = passwordHasher.HashPassword(user, request.RegisterDto.Password);
            await repositoryWrapper.UserRepository.CreateAsync(user);

            if (await repositoryWrapper.SaveChangesAsync() <= 0)
            {
                return Result.Fail<AuthResponseDto>(UserConstants.UserCreationError);
            }

            RefreshToken refreshToken = tokenService.GenerateRefreshToken(user);
            await repositoryWrapper.RefreshTokenRepository.CreateAsync(refreshToken);
            user.TokenId = refreshToken.Id;

            if (await repositoryWrapper.SaveChangesAsync() <= 0)
            {
                return Result.Fail<AuthResponseDto>(UserConstants.UserCreationError);
            }

            UserResponseDto userResponseDto = mapper.Map<UserResponseDto>(user);

            AuthResponseDto authResponseDto = new AuthResponseDto()
            {
                AccessToken = tokenService.GenerateJwtToken(user),
                RefreshToken = refreshToken.Token,
                ExpiresAt = refreshToken.ExpiresAt,
                UserResponseDto = userResponseDto,
            };

            return Result.Ok(authResponseDto);
        }
        catch
        {
            return Result.Fail<AuthResponseDto>(UserConstants.UserCreationError);
        }
    }
}
