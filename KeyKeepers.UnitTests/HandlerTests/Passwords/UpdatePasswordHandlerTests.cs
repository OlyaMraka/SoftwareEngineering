using AutoMapper;
using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using KeyKeepers.BLL.Commands.Passwords.Update;
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
    public class UpdatePasswordHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> repoMock;
        private readonly Mock<IMapper> mapperMock;
        private readonly Mock<IValidator<UpdatePasswordCommand>> validatorMock;
        private readonly UpdatePasswordHandler handler;

        public UpdatePasswordHandlerTests()
        {
            repoMock = new Mock<IRepositoryWrapper>();
            mapperMock = new Mock<IMapper>();
            validatorMock = new Mock<IValidator<UpdatePasswordCommand>>();

            handler = new UpdatePasswordHandler(mapperMock.Object, repoMock.Object, validatorMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Fail_When_ValidationFails()
        {
            // Arrange
            var command = new UpdatePasswordCommand(new UpdatePasswordRequest() { Id = 1, AppName = "App1", Login = "user1" });

            validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new ValidationResult(new[] { new ValidationFailure("AppName", "Validation failed") }));

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Contains("Validation failed", result.Errors[0].Message);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Fail_When_PasswordNotFound()
        {
            // Arrange
            var command = new UpdatePasswordCommand(new UpdatePasswordRequest() { Id = 42, AppName = "App1", Login = "user1" });

            validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new ValidationResult());

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
            var command = new UpdatePasswordCommand(new UpdatePasswordRequest() { Id = 42, AppName = "App1", Login = "user1" });
            var entity = new Credentials { Id = 42, AppName = "OldApp", Login = "olduser" };

            validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new ValidationResult());

            repoMock.Setup(r => r.PasswordRepository
                .GetFirstOrDefaultAsync(It.IsAny<QueryOptions<Credentials>>()))
                .ReturnsAsync(entity);

            mapperMock.Setup(m => m.Map(command.Request, entity));

            repoMock.Setup(r => r.PasswordRepository.Update(entity));
            repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0); // Збереження не пройшло

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains(PasswordConstants.SaveDataBaseError, result.Errors[0].Message);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Success_When_PasswordUpdated()
        {
            // Arrange
            var command = new UpdatePasswordCommand(new UpdatePasswordRequest() { Id = 42, AppName = "App1", Login = "user1" });
            var entity = new Credentials { Id = 42, AppName = "OldApp", Login = "olduser" };
            var responseDto = new PasswordResponse { Id = 42, AppName = "App1", Login = "user1" };

            validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new ValidationResult());

            repoMock.Setup(r => r.PasswordRepository
                .GetFirstOrDefaultAsync(It.IsAny<QueryOptions<Credentials>>()))
                .ReturnsAsync(entity);

            mapperMock.Setup(m => m.Map(command.Request, entity));
            repoMock.Setup(r => r.PasswordRepository.Update(entity));
            repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
            mapperMock.Setup(m => m.Map<PasswordResponse>(entity)).Returns(responseDto);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(responseDto.Id, result.Value.Id);
            Assert.Equal(responseDto.AppName, result.Value.AppName);
            Assert.Equal(responseDto.Login, result.Value.Login);
        }
    }
}
