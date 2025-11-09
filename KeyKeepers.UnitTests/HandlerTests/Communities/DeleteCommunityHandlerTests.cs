using FluentResults;
using KeyKeepers.BLL.Commands.Communities.Delete;
using KeyKeepers.BLL.Constants;
using KeyKeepers.DAL.Entities;
using KeyKeepers.DAL.Repositories.Interfaces.Base;
using KeyKeepers.DAL.Repositories.Options;
using Moq;
using Xunit;

namespace KeyKeepers.UnitTests.HandlerTests.Communities
{
    public class DeleteCommunityHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> repoMock;
        private readonly DeleteCommunityHandler handler;

        public DeleteCommunityHandlerTests()
        {
            repoMock = new Mock<IRepositoryWrapper>();
            handler = new DeleteCommunityHandler(repoMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Fail_When_CommunityNotFound()
        {
            // Arrange
            var command = new DeleteCommunityCommand(1);

            repoMock.Setup(r => r.CommunityRepository.GetFirstOrDefaultAsync(It.IsAny<QueryOptions<Community>>()))
                    .ReturnsAsync((Community?)null);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(CommunityConstants.CommunityNotFoundError, result.Errors[0].Message);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Fail_When_SaveChangesFails()
        {
            // Arrange
            var community = new Community { Id = 2, Name = "Test Club" };
            var command = new DeleteCommunityCommand(2);

            repoMock.Setup(r => r.CommunityRepository.GetFirstOrDefaultAsync(It.IsAny<QueryOptions<Community>>()))
                    .ReturnsAsync(community);

            repoMock.Setup(r => r.CommunityRepository.Delete(community));

            repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0); // simulate DB error

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(CommunityConstants.DbSaveError, result.Errors[0].Message);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Success_When_CommunityDeleted()
        {
            // Arrange
            var community = new Community { Id = 3, Name = "Dev Club" };
            var command = new DeleteCommunityCommand(3);

            repoMock.Setup(r => r.CommunityRepository.GetFirstOrDefaultAsync(It.IsAny<QueryOptions<Community>>()))
                    .ReturnsAsync(community);

            repoMock.Setup(r => r.CommunityRepository.Delete(community));

            repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(community.Id, result.Value);

            // Verify calls
            repoMock.Verify(r => r.CommunityRepository.Delete(community), Times.Once);
            repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}
