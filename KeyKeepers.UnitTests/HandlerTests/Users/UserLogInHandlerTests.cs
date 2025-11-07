using KeyKeepers.BLL.Commands.Users.LogIn;
using KeyKeepers.BLL.Constants;
using KeyKeepers.BLL.DTOs.Users;
using KeyKeepers.BLL.Interfaces;
using KeyKeepers.DAL.Entities;
using KeyKeepers.DAL.Repositories.Interfaces.Base;
using KeyKeepers.DAL.Repositories.Options;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace KeyKeepers.UnitTests.HandlerTests.Users
{
    public class UserLogInHandlerTests
    {
        private readonly Mock<ITokenService> tokenServiceMock;
        private readonly Mock<IRepositoryWrapper> repoMock;
        private readonly Mock<IPasswordHasher<User>> passwordHasherMock;
        private readonly UserLogInHandler handler;

        public UserLogInHandlerTests()
        {
            tokenServiceMock = new Mock<ITokenService>();
            repoMock = new Mock<IRepositoryWrapper>();
            passwordHasherMock = new Mock<IPasswordHasher<User>>();

            handler = new UserLogInHandler(tokenServiceMock.Object, repoMock.Object, passwordHasherMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Fail_When_UserNotFound()
        {
            var command = new UserLogInCommand(new UserLogInDto
            {
                Username = "olga123",
                Password = "Password1!",
            });

            repoMock.Setup(r => r.UserRepository.GetFirstOrDefaultAsync(It.IsAny<QueryOptions<User>>()))
                    .ReturnsAsync((User?)null);

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(UserConstants.UserLogInError, result.Errors[0].Message);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Fail_When_PasswordIncorrect()
        {
            var command = new UserLogInCommand(new UserLogInDto
            {
                Username = "olga123",
                Password = "WrongPassword",
            });

            var user = new User { Id = 1, PasswordHash = "hashedPassword" };

            repoMock.Setup(r => r.UserRepository.GetFirstOrDefaultAsync(It.IsAny<QueryOptions<User>>()))
                    .ReturnsAsync(user);

            passwordHasherMock.Setup(p => p.VerifyHashedPassword(user, user.PasswordHash, command.UserLogInDto.Password))
                              .Returns(PasswordVerificationResult.Failed);

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.False(result.IsSuccess);
            Assert.Equal(UserConstants.UserLogInError, result.Errors[0].Message);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Success_When_ValidCredentials()
        {
            var command = new UserLogInCommand(new UserLogInDto
            {
                Username = "olga123",
                Password = "Password1!",
            });

            var user = new User { Id = 1, PasswordHash = "hashedPassword" };

            repoMock.Setup(r => r.UserRepository.GetFirstOrDefaultAsync(It.IsAny<QueryOptions<User>>()))
                    .ReturnsAsync(user);

            passwordHasherMock.Setup(p => p.VerifyHashedPassword(user, user.PasswordHash, command.UserLogInDto.Password))
                              .Returns(PasswordVerificationResult.Success);

            tokenServiceMock.Setup(t => t.GenerateJwtToken(user)).Returns("jwtToken");

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(1, result.Value.Id);
            Assert.Equal("jwtToken", result.Value.AccessToken);
        }
    }
}
