using AutoMapper;
using MediatR;
using FluentResults;
using FluentValidation;
using KeyKeepers.BLL.Constants;
using KeyKeepers.BLL.DTOs.Passwords;
using KeyKeepers.DAL.Entities;
using KeyKeepers.DAL.Repositories.Options;
using KeyKeepers.DAL.Repositories.Interfaces.Base;

namespace KeyKeepers.BLL.Commands.Passwords.Update;

public class UpdatePasswordHandler : IRequestHandler<UpdatePasswordCommand, Result<PasswordResponse>>
{
    private readonly IMapper mapper;
    private readonly IRepositoryWrapper repositoryWrapper;
    private readonly IValidator<UpdatePasswordCommand> validator;

    public UpdatePasswordHandler(
        IMapper mapperObj,
        IRepositoryWrapper repositoryWrapperObj,
        IValidator<UpdatePasswordCommand> validatorObj)
    {
        mapper = mapperObj;
        repositoryWrapper = repositoryWrapperObj;
        validator = validatorObj;
    }

    public async Task<Result<PasswordResponse>> Handle(
        UpdatePasswordCommand request,
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
                Filter = x => x.Id == request.Request.Id,
                AsNoTracking = false,
            });

        if (entity == null)
        {
            return Result.Fail<PasswordResponse>(PasswordConstants.NotFoundError);
        }

        mapper.Map(request.Request, entity);

        repositoryWrapper.PasswordRepository.Update(entity);

        if (await repositoryWrapper.SaveChangesAsync() < 1)
        {
            return Result.Fail<PasswordResponse>(PasswordConstants.SaveDataBaseError);
        }

        PasswordResponse response = mapper.Map<PasswordResponse>(entity);

        return Result.Ok(response);
    }
}
