using FluentResults;
using KeyKeepers.BLL.Commands.Users.LogOut;
using KeyKeepers.BLL.Constants;
using KeyKeepers.BLL.DTOs.Users;
using KeyKeepers.DAL.Entities;
using KeyKeepers.DAL.Repositories.Interfaces.Base;
using KeyKeepers.DAL.Repositories.Interfaces.RefreshTokens;
using KeyKeepers.DAL.Repositories.Options;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace KeyKeepers.UnitTests.HandlerTests
{
    public class UserLogOutHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> repoMock;
        private readonly Mock<IRepositoryWrapper> refreshTokenRepoMock;
        private readonly UserLogOutHandler handler;

        public UserLogOutHandlerTests()
        {
            repoMock = new Mock<IRepositoryWrapper>();
            refreshTokenRepoMock = new Mock<IRepositoryWrapper>();

            // Настроюємо RefreshTokenRepository у мок
            var refreshTokenRepo = new Mock<IRefreshTokenRepository>();
            repoMock.SetupGet(r => r.RefreshTokenRepository).Returns(refreshTokenRepo.Object);

            handler = new UserLogOutHandler(repoMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Fail_When_RefreshTokenNotFound()
        {
            var command = new UserLogOutCommand(new UserLogOutDto
            {
                RefreshToken = "nonExistingToken",
            });

            repoMock.Setup(r => r.RefreshTokenRepository.GetFirstOrDefaultAsync(It.IsAny<QueryOptions<RefreshToken>>()))
                    .ReturnsAsync((RefreshToken?)null);

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(UserConstants.UserLogOutError, result.Errors[0].Message);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Fail_When_SaveChangesFails()
        {
            var refreshToken = new RefreshToken { Token = "token" };
            var command = new UserLogOutCommand(new UserLogOutDto { RefreshToken = "token" });

            repoMock.Setup(r => r.RefreshTokenRepository.GetFirstOrDefaultAsync(It.IsAny<QueryOptions<RefreshToken>>()))
                    .ReturnsAsync(refreshToken);

            repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(UserConstants.UserLogOutError, result.Errors[0].Message);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Success_When_RefreshTokenDeleted()
        {
            var refreshToken = new RefreshToken { Token = "token" };
            var command = new UserLogOutCommand(new UserLogOutDto { RefreshToken = "token" });

            repoMock.Setup(r => r.RefreshTokenRepository.GetFirstOrDefaultAsync(It.IsAny<QueryOptions<RefreshToken>>()))
                    .ReturnsAsync(refreshToken);

            repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsSuccess);
        }
    }
}
