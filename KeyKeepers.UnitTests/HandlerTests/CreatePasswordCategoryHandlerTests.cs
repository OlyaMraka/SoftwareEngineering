using AutoMapper;
using FluentResults;
using FluentValidation;
using FluentValidation.Results;
using KeyKeepers.BLL.Commands.PasswordCategory.Create;
using KeyKeepers.BLL.Constants;
using KeyKeepers.BLL.DTOs.PasswordCategories;
using KeyKeepers.DAL.Entities;
using KeyKeepers.DAL.Repositories.Interfaces.Base;
using KeyKeepers.DAL.Repositories.Options;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace KeyKeepers.UnitTests.HandlerTests
{
    public class CreatePasswordCategoryHandlerTests
    {
        private readonly Mock<IMapper> mapperMock;
        private readonly Mock<IRepositoryWrapper> repoMock;
        private readonly Mock<IValidator<CreatePrivateCategoryCommand>> validatorMock;
        private readonly CreatePrivateCategoryHandler handler;

        public CreatePasswordCategoryHandlerTests()
        {
            mapperMock = new Mock<IMapper>();
            repoMock = new Mock<IRepositoryWrapper>();
            validatorMock = new Mock<IValidator<CreatePrivateCategoryCommand>>();

            handler = new CreatePrivateCategoryHandler(
                mapperMock.Object,
                repoMock.Object,
                validatorMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Fail_When_CategoryAlreadyExists()
        {
            // Arrange
            var command = new CreatePrivateCategoryCommand(new CreatePrivateCategoryDto()
            {
                Name = "Finance",
            });

            validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new ValidationResult());

            repoMock.Setup(r => r.PrivatePasswordCategoryRepository
                .GetFirstOrDefaultAsync(It.IsAny<QueryOptions<PrivateCategory>>()))
                .ReturnsAsync(new PrivateCategory { Id = 1, Name = "Finance" });

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains(PasswordCategoriesConstants.CategoryAlreadyExistsErrorMessage, result.Errors[0].Message);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Fail_When_SaveChangesFailed()
        {
            // Arrange
            var command = new CreatePrivateCategoryCommand(new CreatePrivateCategoryDto()
            {
                Name = "Personal",
            });

            validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new ValidationResult());

            repoMock.Setup(r => r.PrivatePasswordCategoryRepository
                .GetFirstOrDefaultAsync(It.IsAny<QueryOptions<PrivateCategory>>()))
                .ReturnsAsync((PrivateCategory?)null);

            var entity = new PrivateCategory { Id = 5, Name = "Personal" };
            mapperMock.Setup(m => m.Map<PrivateCategory>(command)).Returns(entity);

            repoMock.Setup(r => r.PrivatePasswordCategoryRepository.CreateAsync(entity))
                    .ReturnsAsync(entity);

            repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains(PasswordCategoriesConstants.DbSaveErrorMessage, result.Errors[0].Message);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Success_When_CategoryCreated()
        {
            // Arrange
            var command = new CreatePrivateCategoryCommand(new CreatePrivateCategoryDto()
            {
                Name = "Work",
            });

            validatorMock.Setup(v => v.ValidateAsync(command, It.IsAny<CancellationToken>()))
                         .ReturnsAsync(new ValidationResult());

            repoMock.Setup(r => r.PrivatePasswordCategoryRepository
                .GetFirstOrDefaultAsync(It.IsAny<QueryOptions<PrivateCategory>>()))
                .ReturnsAsync((PrivateCategory?)null);

            var entity = new PrivateCategory { Id = 2, Name = "Work" };
            mapperMock.Setup(m => m.Map<PrivateCategory>(command)).Returns(entity);

            repoMock.Setup(r => r.PrivatePasswordCategoryRepository.CreateAsync(entity))
                    .ReturnsAsync(entity);

            repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            var responseDto = new PrivateCategoryResponseDto { Id = 2, Name = "Work" };
            mapperMock.Setup(m => m.Map<PrivateCategoryResponseDto>(entity)).Returns(responseDto);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(responseDto.Id, result.Value.Id);
            Assert.Equal("Work", result.Value.Name);
        }

        [Fact]
        public async Task Handle_ShouldThrow_When_ValidationFails()
        {
            // Arrange
            var command = new CreatePrivateCategoryCommand(new CreatePrivateCategoryDto()
            {
                Name = "ErrorCategory",
            });

            validatorMock.Setup(v => v.ValidateAsync(
                    It.IsAny<CreatePrivateCategoryCommand>(),
                    It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ValidationException("Validation failed"));

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.True(result.IsFailed);
        }
    }
}
