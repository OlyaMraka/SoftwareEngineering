using FluentResults;
using FluentValidation;
using KeyKeepers.BLL.Commands.Communities.Update;
using KeyKeepers.BLL.Constants;
using KeyKeepers.BLL.DTOs.Communities;
using KeyKeepers.DAL.Entities;
using KeyKeepers.DAL.Repositories.Interfaces.Base;
using KeyKeepers.DAL.Repositories.Options;
using AutoMapper;
using Moq;
using Xunit;

namespace KeyKeepers.UnitTests.HandlerTests.Communities
{
    public class UpdateCommunityHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> repoMock;
        private readonly Mock<IMapper> mapperMock;
        private readonly Mock<IValidator<UpdateCommunityCommand>> validatorMock;
        private readonly UpdateCommunityHandler handler;

        public UpdateCommunityHandlerTests()
        {
            repoMock = new Mock<IRepositoryWrapper>();
            mapperMock = new Mock<IMapper>();
            validatorMock = new Mock<IValidator<UpdateCommunityCommand>>();
            handler = new UpdateCommunityHandler(mapperMock.Object, repoMock.Object, validatorMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Fail_When_ValidationFails()
        {
            // Arrange
            var command = new UpdateCommunityCommand(new UpdateCommunityRequestDto() { CommunityId = 1, Name = "New Name" });

            validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new FluentValidation.Results.ValidationResult(
                             new[] { new FluentValidation.Results.ValidationFailure("Name", "Invalid Name") }));

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Invalid Name", result.Errors[0].Message);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Fail_When_CommunityNotFound()
        {
            // Arrange
            var command = new UpdateCommunityCommand(new UpdateCommunityRequestDto() { CommunityId = 2, Name = "New Name" });

            validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new FluentValidation.Results.ValidationResult());

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
            var dto = new UpdateCommunityRequestDto() { CommunityId = 3, Name = "Updated Name" };
            var command = new UpdateCommunityCommand(dto);
            var entity = new Community { Id = 3, Name = "Old Name" };

            validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            repoMock.Setup(r => r.CommunityRepository.GetFirstOrDefaultAsync(It.IsAny<QueryOptions<Community>>()))
                    .ReturnsAsync(entity);

            mapperMock.Setup(m => m.Map(dto, entity));

            repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0); // simulate DB error

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(CommunityConstants.DbSaveError, result.Errors[0].Message);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Success_When_CommunityUpdated()
        {
            // Arrange
            var dto = new UpdateCommunityRequestDto() { CommunityId = 4, Name = "Updated Club" };
            var command = new UpdateCommunityCommand(dto);
            var entity = new Community { Id = 4, Name = "Old Club" };
            var responseDto = new CommunityResponseDto { Id = 4, Name = "Updated Club" };

            validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new FluentValidation.Results.ValidationResult());

            repoMock.Setup(r => r.CommunityRepository.GetFirstOrDefaultAsync(It.IsAny<QueryOptions<Community>>()))
                    .ReturnsAsync(entity);

            mapperMock.Setup(m => m.Map(dto, entity));
            repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
            mapperMock.Setup(m => m.Map<CommunityResponseDto>(entity)).Returns(responseDto);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(responseDto.Id, result.Value.Id);
            Assert.Equal(responseDto.Name, result.Value.Name);

            repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}
