using KeyKeepers.BLL.Commands.Passwords.Delete;
using KeyKeepers.BLL.Constants;
using KeyKeepers.DAL.Entities;
using KeyKeepers.DAL.Repositories.Interfaces.Base;
using KeyKeepers.DAL.Repositories.Options;
using Moq;

namespace KeyKeepers.UnitTests.HandlerTests.Passwords
{
    public class DeletePasswordHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> repoMock;
        private readonly DeletePasswordHandler handler;

        public DeletePasswordHandlerTests()
        {
            repoMock = new Mock<IRepositoryWrapper>();
            handler = new DeletePasswordHandler(repoMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Fail_When_PasswordNotFound()
        {
            // Arrange
            var command = new DeletePasswordCommand(42);

            repoMock.Setup(r => r.PasswordRepository
                .GetFirstOrDefaultAsync(It.IsAny<QueryOptions<Credentials>>()))
                .ReturnsAsync((Credentials?)null);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains(PasswordConstants.NotFoundError, result.Errors[0].Message);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Fail_When_SaveChangesFailed()
        {
            // Arrange
            var command = new DeletePasswordCommand(42);
            var entity = new Credentials { Id = 42, AppName = "TestApp", Login = "user@test.com" };

            repoMock.Setup(r => r.PasswordRepository
                .GetFirstOrDefaultAsync(It.IsAny<QueryOptions<Credentials>>()))
                .ReturnsAsync(entity);

            repoMock.Setup(r => r.PasswordRepository.Delete(entity));

            repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0); // Не вдалось зберегти

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains(PasswordConstants.SaveDataBaseError, result.Errors[0].Message);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Success_When_PasswordDeleted()
        {
            // Arrange
            var command = new DeletePasswordCommand(42);
            var entity = new Credentials { Id = 42, AppName = "TestApp", Login = "user@test.com" };

            repoMock.Setup(r => r.PasswordRepository
                .GetFirstOrDefaultAsync(It.IsAny<QueryOptions<Credentials>>()))
                .ReturnsAsync(entity);

            repoMock.Setup(r => r.PasswordRepository.Delete(entity));

            repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1); // Успішне збереження

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(entity.Id, result.Value);
        }
    }
}
