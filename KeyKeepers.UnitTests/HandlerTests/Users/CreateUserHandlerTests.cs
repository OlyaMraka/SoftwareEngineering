using AutoMapper;
using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using KeyKeepers.BLL.Commands.Users.Create;
using KeyKeepers.BLL.DTOs.Users;
using KeyKeepers.BLL.Interfaces;
using KeyKeepers.DAL.Entities;
using KeyKeepers.DAL.Repositories.Interfaces.Base;
using KeyKeepers.DAL.Repositories.Options;
using Microsoft.AspNetCore.Identity;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace KeyKeepers.UnitTests.HandlerTests
{
    public class CreateUserHandlerTests
    {
        private readonly Mock<IMapper> mapperMock;
        private readonly Mock<IRepositoryWrapper> repoMock;
        private readonly Mock<ITokenService> tokenServiceMock;
        private readonly Mock<IValidator<CreateUserCommand>> validatorMock;
        private readonly Mock<IPasswordHasher<User>> passwordHasherMock;
        private readonly CreateUserHandler handler;

        public CreateUserHandlerTests()
        {
            mapperMock = new Mock<IMapper>();
            repoMock = new Mock<IRepositoryWrapper>();
            tokenServiceMock = new Mock<ITokenService>();
            validatorMock = new Mock<IValidator<CreateUserCommand>>();
            passwordHasherMock = new Mock<IPasswordHasher<User>>();

            handler = new CreateUserHandler(
                mapperMock.Object,
                repoMock.Object,
                tokenServiceMock.Object,
                validatorMock.Object,
                passwordHasherMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Fail_When_UserAlreadyExists()
        {
            var command = new CreateUserCommand(new UserRegisterDto
            {
                Name = "Olga",
                Surname = "Petrova",
                Email = "olga@example.com",
                UserName = "olga123",
                Password = "Password1!",
            });

            // Замокаємо валідатор через ValidateAsync, щоб не кидати exception
            validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new ValidationResult());

            repoMock.Setup(r => r.UserRepository.GetFirstOrDefaultAsync(It.IsAny<QueryOptions<User>>()))
                    .ReturnsAsync(new User());

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.False(result.IsSuccess);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Success_When_UserCreated()
        {
            var command = new CreateUserCommand(new UserRegisterDto
            {
                Name = "Olga",
                Surname = "Petrova",
                Email = "olga@example.com",
                UserName = "olga123",
                Password = "Password1!",
            });

            var user = new User { Id = 1 };

            validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new ValidationResult());

            repoMock.Setup(r => r.UserRepository.GetFirstOrDefaultAsync(It.IsAny<QueryOptions<User>>()))
                    .ReturnsAsync((User?)null);

            mapperMock.Setup(m => m.Map<User>(command.RegisterDto)).Returns(user);
            passwordHasherMock.Setup(p => p.HashPassword(user, command.RegisterDto.Password))
                              .Returns("hashedPassword");

            repoMock.Setup(r => r.UserRepository.CreateAsync(user)).ReturnsAsync(user);
            repoMock.SetupSequence(r => r.SaveChangesAsync())
                    .ReturnsAsync(1)
                    .ReturnsAsync(1);

            var refreshToken = new RefreshToken { Token = "refreshToken", ExpiresAt = System.DateTime.UtcNow.AddDays(7) };
            tokenServiceMock.Setup(t => t.GenerateRefreshToken(user)).Returns(refreshToken);
            repoMock.Setup(r => r.RefreshTokenRepository.CreateAsync(refreshToken)).ReturnsAsync(refreshToken);

            mapperMock.Setup(m => m.Map<UserResponseDto>(user)).Returns(new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
            });

            tokenServiceMock.Setup(t => t.GenerateJwtToken(user)).Returns("jwtToken");

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal("jwtToken", result.Value.AccessToken);
            Assert.Equal("refreshToken", result.Value.RefreshToken);
        }
    }
}
