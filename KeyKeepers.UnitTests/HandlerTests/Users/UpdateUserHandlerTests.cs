using AutoMapper;
using FluentValidation;
using FluentValidation.Results;
using KeyKeepers.BLL.Commands.Users.Update;
using KeyKeepers.BLL.Constants;
using KeyKeepers.BLL.DTOs.Users;
using KeyKeepers.DAL.Entities;
using KeyKeepers.DAL.Repositories.Interfaces.Base;
using KeyKeepers.DAL.Repositories.Options;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace KeyKeepers.UnitTests.HandlerTests.Users
{
    public class UpdateUserHandlerTests
    {
        private readonly Mock<IMapper> mapperMock;
        private readonly Mock<IRepositoryWrapper> repoMock;
        private readonly Mock<IValidator<UpdateUserCommand>> validatorMock;
        private readonly Mock<IValidator<string>> passwordValidator;
        private readonly Mock<IPasswordHasher<User>> passwordHasherMock;
        private readonly UpdateUserHandler handler;

        public UpdateUserHandlerTests()
        {
            mapperMock = new Mock<IMapper>();
            repoMock = new Mock<IRepositoryWrapper>();
            validatorMock = new Mock<IValidator<UpdateUserCommand>>();
            passwordHasherMock = new Mock<IPasswordHasher<User>>();
            passwordValidator = new Mock<IValidator<string>>();

            handler = new UpdateUserHandler(
                mapperMock.Object,
                repoMock.Object,
                validatorMock.Object,
                passwordHasherMock.Object,
                passwordValidator.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Fail_WhenValidationFails()
        {
            // Arrange
            var command = new UpdateUserCommand(new UpdateUserDto
            {
                UserId = 1,
                Name = "Olga",
                Surname = "Petrova",
                Email = "invalid email",
                UserName = "olga123",
                Password = "Password1!",
            });

            validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("Email", "Invalid email") }));

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Invalid email", result.Errors[0].Message);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Fail_When_SaveChanges_ReturnsZero()
        {
            // Arrange
            var command = new UpdateUserCommand(new UpdateUserDto
            {
                UserId = 1,
                Name = "Olga",
                Surname = "Petrova",
                Email = "olga@example.com",
                UserName = "olga123",
                Password = "Password1!",
            });

            var user = new User { Id = 1 };

            passwordValidator.Setup(v => v.ValidateAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationResult());

            validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new ValidationResult());

            repoMock.Setup(r => r.UserRepository.GetFirstOrDefaultAsync(It.IsAny<QueryOptions<User>>()))
                    .ReturnsAsync(user);

            mapperMock.Setup(m => m.Map(command.RequestDto, user));
            passwordHasherMock.Setup(p => p.HashPassword(user, command.RequestDto.Password))
                              .Returns("hashedPassword");

            repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(UserConstants.DbSaveError, result.Errors[0].Message);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Success_When_UserUpdated()
        {
            // Arrange
            var command = new UpdateUserCommand(new UpdateUserDto
            {
                UserId = 1,
                Name = "Olga",
                Surname = "Petrova",
                Email = "olga@example.com",
                UserName = "olga123",
                Password = "Password1!",
            });

            var user = new User { Id = 1 };

            passwordValidator.Setup(v => v.ValidateAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationResult());

            validatorMock.Setup(v => v.ValidateAsync(
                    command,
                    It.IsAny<CancellationToken>())).ReturnsAsync(new ValidationResult());

            repoMock.Setup(r => r.UserRepository.GetFirstOrDefaultAsync(It.IsAny<QueryOptions<User>>()))
                    .ReturnsAsync(user);

            mapperMock.Setup(m => m.Map(command.RequestDto, user));
            passwordHasherMock.Setup(p => p.HashPassword(user, command.RequestDto.Password))
                              .Returns("hashedPassword");

            repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            var expectedResponse = new UserResponseDto
            {
                Id = user.Id,
                Name = command.RequestDto.Name,
                Email = command.RequestDto.Email,
            };
            mapperMock.Setup(m => m.Map<UserResponseDto>(user)).Returns(expectedResponse);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(expectedResponse.Id, result.Value.Id);
            Assert.Equal(expectedResponse.Email, result.Value.Email);
            Assert.Equal(expectedResponse.Name, result.Value.Name);

            // Verify calls
            passwordHasherMock.Verify(p => p.HashPassword(user, command.RequestDto.Password), Times.Once);
            repoMock.Verify(r => r.UserRepository.Update(user), Times.Once);
            repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}
