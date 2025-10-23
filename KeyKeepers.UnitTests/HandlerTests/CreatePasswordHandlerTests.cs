using AutoMapper;
using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using KeyKeepers.BLL.Commands.Passwords.Create;
using KeyKeepers.BLL.Constants;
using KeyKeepers.BLL.DTOs.Passwords;
using KeyKeepers.DAL.Entities;
using KeyKeepers.DAL.Repositories.Interfaces.Base;
using KeyKeepers.DAL.Repositories.Options;
using Moq;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace KeyKeepers.UnitTests.HandlerTests
{
    public class CreatePasswordHandlerTests
    {
        private readonly Mock<IMapper> mapperMock;
        private readonly Mock<IRepositoryWrapper> repoMock;
        private readonly Mock<IValidator<CreatePasswordCommand>> validatorMock;
        private readonly CreatePasswordHandler handler;

        public CreatePasswordHandlerTests()
        {
            mapperMock = new Mock<IMapper>();
            repoMock = new Mock<IRepositoryWrapper>();
            validatorMock = new Mock<IValidator<CreatePasswordCommand>>();

            handler = new CreatePasswordHandler(
                mapperMock.Object,
                repoMock.Object,
                validatorMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Fail_When_PasswordAlreadyExists()
        {
            // Arrange
            var command = new CreatePasswordCommand(new CreatePasswordRequest()
            {
                AppName = "Gmail",
                Login = "user@gmail.com",
                Password = "pass123",
            });

            validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new ValidationResult());

            repoMock.Setup(r => r.PasswordRepository
                .GetFirstOrDefaultAsync(It.IsAny<QueryOptions<Credentials>>()))
                .ReturnsAsync(new Credentials { Id = 1, AppName = "Gmail", Login = "user@gmail.com" });

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains(PasswordConstants.AlreadyExistsError, result.Errors[0].Message);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Fail_When_SaveChangesFailed()
        {
            // Arrange
            var command = new CreatePasswordCommand(new CreatePasswordRequest()
            {
                AppName = "Facebook",
                Login = "user@fb.com",
                Password = "pass456",
            });

            validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new ValidationResult());

            repoMock.Setup(r => r.PasswordRepository
                .GetFirstOrDefaultAsync(It.IsAny<QueryOptions<Credentials>>()))
                .ReturnsAsync((Credentials?)null);

            var entity = new Credentials { Id = 5, AppName = "Facebook", Login = "user@fb.com", };
            mapperMock.Setup(m => m.Map<Credentials>(command.Request)).Returns(entity);

            repoMock.Setup(r => r.PasswordRepository.CreateAsync(entity)).ReturnsAsync(entity);
            repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains(PasswordConstants.SaveDataBaseError, result.Errors[0].Message);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Success_When_PasswordCreated()
        {
            // Arrange
            var command = new CreatePasswordCommand(new CreatePasswordRequest()
            {
                AppName = "WorkApp",
                Login = "user@work.com",
                Password = "pass789",
            });

            validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new ValidationResult());

            repoMock.Setup(r => r.PasswordRepository
                    .GetFirstOrDefaultAsync(It.IsAny<QueryOptions<Credentials>>()))
                .ReturnsAsync((Credentials?)null);

            var entity = new Credentials { Id = 2, AppName = "WorkApp", Login = "user@work.com", };
            mapperMock.Setup(m => m.Map<Credentials>(command.Request)).Returns(entity);
            repoMock.Setup(r => r.PasswordRepository.CreateAsync(entity)).ReturnsAsync(entity);
            repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            var responseDto = new PasswordResponse { Id = 2, AppName = "WorkApp", Login = "user@work.com" };
            mapperMock.Setup(m => m.Map<PasswordResponse>(entity)).Returns(responseDto);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(responseDto.Id, result.Value.Id);
            Assert.Equal(responseDto.AppName, result.Value.AppName);
            Assert.Equal(responseDto.Login, result.Value.Login);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Fail_When_ValidationFails()
        {
            // Arrange
            var command = new CreatePasswordCommand(new CreatePasswordRequest()
            {
                AppName = "ErrorApp",
                Login = "error@login.com",
                Password = "error123",
            });

            validatorMock.Setup(v => v.ValidateAsync(It.IsAny<CreatePasswordCommand>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("AppName", "Validation failed") }));

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains("Validation failed", result.Errors[0].Message);
        }
    }
}
