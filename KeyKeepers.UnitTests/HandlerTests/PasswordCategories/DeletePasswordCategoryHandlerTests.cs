using KeyKeepers.BLL.Commands.PasswordCategory.Delete;
using KeyKeepers.BLL.Constants;
using KeyKeepers.DAL.Entities;
using KeyKeepers.DAL.Repositories.Interfaces.Base;
using KeyKeepers.DAL.Repositories.Options;
using Moq;

namespace KeyKeepers.UnitTests.HandlerTests.PasswordCategories
{
    public class DeletePasswordCategoryHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> repoMock;
        private readonly DeletePrivateCategoryHandler handler;

        public DeletePasswordCategoryHandlerTests()
        {
            repoMock = new Mock<IRepositoryWrapper>();
            handler = new DeletePrivateCategoryHandler(repoMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Fail_When_CategoryNotFound()
        {
            // Arrange
            var command = new DeletePrivateCategoryCommand(5);

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
            var category = new PrivateCategory
            {
                Id = 3,
                Name = "Work",
            };

            var command = new DeletePrivateCategoryCommand(category.Id);

            repoMock.Setup(r => r.PrivatePasswordCategoryRepository
                .GetFirstOrDefaultAsync(It.IsAny<QueryOptions<PrivateCategory>>()))
                .ReturnsAsync(category);

            repoMock.Setup(r => r.PrivatePasswordCategoryRepository.Delete(category))
                    .Verifiable();

            repoMock.Setup(r => r.SaveChangesAsync())
                    .ReturnsAsync(0);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(PasswordCategoriesConstants.DbSaveErrorMessage, result.Errors[0].Message);
            repoMock.Verify(r => r.PrivatePasswordCategoryRepository.Delete(category), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Success_When_CategoryDeletedSuccessfully()
        {
            // Arrange
            var category = new PrivateCategory
            {
                Id = 2,
                Name = "Finance",
            };

            var command = new DeletePrivateCategoryCommand(category.Id);

            repoMock.Setup(r => r.PrivatePasswordCategoryRepository
                .GetFirstOrDefaultAsync(It.IsAny<QueryOptions<PrivateCategory>>()))
                .ReturnsAsync(category);

            repoMock.Setup(r => r.PrivatePasswordCategoryRepository.Delete(category))
                    .Verifiable();

            repoMock.Setup(r => r.SaveChangesAsync())
                    .ReturnsAsync(1);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(category.Id, result.Value);
            repoMock.Verify(r => r.PrivatePasswordCategoryRepository.Delete(category), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_When_ExceptionThrown()
        {
            // Arrange
            var command = new DeletePrivateCategoryCommand(9);

            repoMock.Setup(r => r.PrivatePasswordCategoryRepository
                    .GetFirstOrDefaultAsync(It.IsAny<QueryOptions<PrivateCategory>>()))
                .ThrowsAsync(new Exception("DB crash"));
            var ex = await Assert.ThrowsAsync<Exception>(
                () => handler.Handle(command, CancellationToken.None));

            Assert.Equal("DB crash", ex.Message);
        }
    }
}
