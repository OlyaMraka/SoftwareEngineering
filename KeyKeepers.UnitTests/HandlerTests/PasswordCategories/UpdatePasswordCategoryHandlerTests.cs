using AutoMapper;
using FluentResults;
using FluentValidation;
using KeyKeepers.BLL.Commands.PasswordCategory.Update;
using KeyKeepers.BLL.Constants;
using KeyKeepers.BLL.DTOs.PasswordCategories;
using KeyKeepers.DAL.Entities;
using KeyKeepers.DAL.Repositories.Interfaces.Base;
using KeyKeepers.DAL.Repositories.Options;
using Moq;

namespace KeyKeepers.UnitTests.HandlerTests
{
    public class UpdatePasswordCategoryHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> repoMock;
        private readonly Mock<IMapper> mapperMock;
        private readonly UpdatePrivateCategoryHandler handler;
        private readonly Mock<IValidator<UpdatePrivateCategoryCommand>> validator;

        public UpdatePasswordCategoryHandlerTests()
        {
            repoMock = new Mock<IRepositoryWrapper>();
            mapperMock = new Mock<IMapper>();
            validator = new Mock<IValidator<UpdatePrivateCategoryCommand>>();
            handler = new UpdatePrivateCategoryHandler(repoMock.Object, mapperMock.Object, validator.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Fail_When_CategoryNotFound()
        {
            // Arrange
            var dto = new UpdatePrivateCategoryDto { Id = 5, Name = "UpdatedName" };
            var command = new UpdatePrivateCategoryCommand(dto);

            repoMock.Setup(r => r.PrivatePasswordCategoryRepository
                .GetFirstOrDefaultAsync(It.IsAny<QueryOptions<PrivateCategory>>()))
                .ReturnsAsync((PrivateCategory?)null);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(PasswordCategoriesConstants.CategoryNotFound, result.Errors[0].Message);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Fail_When_SaveChangesFails()
        {
            // Arrange
            var dto = new UpdatePrivateCategoryDto { Id = 2, Name = "WorkUpdated" };
            var command = new UpdatePrivateCategoryCommand(dto);
            var existingCategory = new PrivateCategory { Id = 2, Name = "Work" };

            repoMock.Setup(r => r.PrivatePasswordCategoryRepository
                .GetFirstOrDefaultAsync(It.IsAny<QueryOptions<PrivateCategory>>()))
                .ReturnsAsync(existingCategory);

            mapperMock.Setup(m => m.Map(command, existingCategory))
                      .Verifiable();

            repoMock.Setup(r => r.PrivatePasswordCategoryRepository.Update(existingCategory))
                    .Verifiable();

            repoMock.Setup(r => r.SaveChangesAsync())
                    .ReturnsAsync(0); // помилка при збереженні

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(PasswordCategoriesConstants.DbSaveErrorMessage, result.Errors[0].Message);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Success_When_CategoryUpdatedSuccessfully()
        {
            // Arrange
            var dto = new UpdatePrivateCategoryDto { Id = 1, Name = "PersonalUpdated" };
            var command = new UpdatePrivateCategoryCommand(dto);
            var existingCategory = new PrivateCategory { Id = 1, Name = "Personal" };

            repoMock.Setup(r => r.PrivatePasswordCategoryRepository
                .GetFirstOrDefaultAsync(It.IsAny<QueryOptions<PrivateCategory>>()))
                .ReturnsAsync(existingCategory);

            mapperMock.Setup(m => m.Map(command, existingCategory))
                      .Verifiable();

            repoMock.Setup(r => r.PrivatePasswordCategoryRepository.Update(existingCategory))
                    .Verifiable();

            repoMock.Setup(r => r.SaveChangesAsync())
                    .ReturnsAsync(1);

            var expectedResponse = new PrivateCategoryResponseDto
            {
                Id = 1,
                Name = "PersonalUpdated",
            };

            mapperMock.Setup(m => m.Map<PrivateCategoryResponseDto>(existingCategory))
                      .Returns(expectedResponse);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(expectedResponse.Name, result.Value.Name);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Fail_When_ExceptionThrown()
        {
            // Arrange
            var dto = new UpdatePrivateCategoryDto { Id = 10, Name = "ErrorCase" };
            var command = new UpdatePrivateCategoryCommand(dto);

            repoMock.Setup(r => r.PrivatePasswordCategoryRepository
                .GetFirstOrDefaultAsync(It.IsAny<QueryOptions<PrivateCategory>>()))
                .ThrowsAsync(new System.Exception("DB error"));

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(PasswordCategoriesConstants.ErrorMessage, result.Errors[0].Message);
        }
    }
}
