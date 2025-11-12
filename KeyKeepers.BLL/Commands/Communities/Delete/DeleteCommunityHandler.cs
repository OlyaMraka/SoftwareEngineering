using MediatR;
using FluentResults;
using KeyKeepers.BLL.Constants;
using KeyKeepers.DAL.Entities;
using KeyKeepers.DAL.Repositories.Interfaces.Base;
using KeyKeepers.DAL.Repositories.Options;

namespace KeyKeepers.BLL.Commands.Communities.Delete;

public class DeleteCommunityHandler : IRequestHandler<DeleteCommunityCommand, Result<long>>
{
    private readonly IRepositoryWrapper repositoryWrapper;

    public DeleteCommunityHandler(IRepositoryWrapper repositoryWrapperObj)
    {
        repositoryWrapper = repositoryWrapperObj;
    }

    public async Task<Result<long>> Handle(DeleteCommunityCommand request, CancellationToken cancellationToken)
    {
        QueryOptions<Community> options = new QueryOptions<Community>
        {
            Filter = c => c.Id == request.CommunityId,
            AsNoTracking = false,
        };

        Community? entity = await repositoryWrapper.CommunityRepository.GetFirstOrDefaultAsync(options);

        if (entity == null)
        {
            return Result.Fail<long>(CommunityConstants.CommunityNotFoundError);
        }

        repositoryWrapper.CommunityRepository.Delete(entity);

        if (await repositoryWrapper.SaveChangesAsync() < 1)
        {
            return Result.Fail<long>(CommunityConstants.DbSaveError);
        }

        repositoryWrapper.ClearChangeTracker();

        return Result.Ok(entity.Id);
    }
}
