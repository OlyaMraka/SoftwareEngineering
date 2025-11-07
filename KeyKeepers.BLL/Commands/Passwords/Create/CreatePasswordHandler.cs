using AutoMapper;
using FluentResults;
using FluentValidation;
using KeyKeepers.BLL.Constants;
using KeyKeepers.BLL.DTOs.Passwords;
using KeyKeepers.BLL.Interfaces;
using KeyKeepers.DAL.Entities;
using KeyKeepers.DAL.Repositories.Interfaces.Base;
using KeyKeepers.DAL.Repositories.Options;
using MediatR;

namespace KeyKeepers.BLL.Commands.Passwords.Create;

public class CreatePasswordHandler : IRequestHandler<CreatePasswordCommand, Result<PasswordResponse>>
{
    private readonly IMapper mapper;
    private readonly IRepositoryWrapper repositoryWrapper;
    private readonly IEncryptionService encryptionService;
    private readonly IValidator<CreatePasswordCommand> validator;

    public CreatePasswordHandler(
        IMapper mapperObj,
        IRepositoryWrapper repositoryWrapperObj,
        IEncryptionService encryptionServiceObj,
        IValidator<CreatePasswordCommand> validatorObj)
    {
        mapper = mapperObj;
        encryptionService = encryptionServiceObj;
        repositoryWrapper = repositoryWrapperObj;
        validator = validatorObj;
    }

    public async Task<Result<PasswordResponse>> Handle(
        CreatePasswordCommand request,
        CancellationToken cancellationToken)
    {
        var result = await validator.ValidateAsync(request, cancellationToken);
        if (!result.IsValid)
        {
            return Result.Fail<PasswordResponse>(result.Errors.First().ErrorMessage);
        }

        Credentials? entity = await repositoryWrapper.PasswordRepository.GetFirstOrDefaultAsync(
            new QueryOptions<Credentials>
            {
                Filter = x => x.AppName == request.Request.AppName && x.Login == request.Request.Login,
                AsNoTracking = false,
            });

        if (entity != null)
        {
            return Result.Fail<PasswordResponse>(PasswordConstants.AlreadyExistsError);
        }

        Credentials newEntity = mapper.Map<Credentials>(request.Request);
        newEntity.PasswordHash = encryptionService.Encrypt(request.Request.Password);

        await repositoryWrapper.PasswordRepository.CreateAsync(newEntity);

        if (await repositoryWrapper.SaveChangesAsync() < 1)
        {
            return Result.Fail<PasswordResponse>(PasswordConstants.SaveDataBaseError);
        }

        PasswordResponse response = mapper.Map<PasswordResponse>(newEntity);

        return Result.Ok(response);
    }
}
