using MediatR;
using FluentResults;
using KeyKeepers.BLL.Constants;
using KeyKeepers.DAL.Entities;
using KeyKeepers.DAL.Repositories.Interfaces.Base;
using KeyKeepers.DAL.Repositories.Options;

namespace KeyKeepers.BLL.Commands.Passwords.Delete;

public class DeletePasswordHandler : IRequestHandler<DeletePasswordCommand, Result<long>>
{
    private readonly IRepositoryWrapper repositoryWrapper;

    public DeletePasswordHandler(IRepositoryWrapper repositoryWrapperObj)
    {
        repositoryWrapper = repositoryWrapperObj;
    }

    public async Task<Result<long>> Handle(DeletePasswordCommand request, CancellationToken cancellationToken)
    {
        Credentials? entity = await repositoryWrapper.PasswordRepository.GetFirstOrDefaultAsync(
            new QueryOptions<Credentials>
            {
                Filter = x => x.Id == request.Id,
                AsNoTracking = false,
            });

        if (entity == null)
        {
            return Result.Fail<long>(PasswordConstants.NotFoundError);
        }

        repositoryWrapper.PasswordRepository.Delete(entity);

        if (await repositoryWrapper.SaveChangesAsync() < 1)
        {
            return Result.Fail<long>(PasswordConstants.SaveDataBaseError);
        }

        return Result.Ok(entity.Id);
    }
}
