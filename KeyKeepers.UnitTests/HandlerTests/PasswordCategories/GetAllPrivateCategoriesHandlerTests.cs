using Xunit;
using Moq;
using AutoMapper;
using FluentResults;
using KeyKeepers.BLL.Queries.PasswordCategories.GetAll;
using KeyKeepers.BLL.DTOs.PasswordCategories;
using KeyKeepers.DAL.Entities;
using KeyKeepers.DAL.Repositories.Interfaces.Base;
using KeyKeepers.DAL.Repositories.Options;
using KeyKeepers.BLL.Constants;

namespace KeyKeepers.UnitTests.Handlers.PasswordCategories
{
    public class GetAllPrivateCategoriesHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> repositoryWrapperMock;
        private readonly Mock<IMapper> mapperMock;
        private readonly GetAllPrivateCategoriesHandler handler;

        public GetAllPrivateCategoriesHandlerTests()
        {
            repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            mapperMock = new Mock<IMapper>();
            handler = new GetAllPrivateCategoriesHandler(mapperMock.Object, repositoryWrapperMock.Object);
        }

        [Fact]
        public async Task Handle_Should_ReturnMappedResult_WhenDataExists()
        {
            // Arrange
            var userId = 10L;
            var entities = new List<PrivateCategory>
            {
                new PrivateCategory { Id = 1, Name = "Personal", UserId = userId },
                new PrivateCategory { Id = 2, Name = "Work", UserId = userId },
            };

            var dtoList = new List<PrivateCategoryResponseDto>
            {
                new PrivateCategoryResponseDto { Id = 1, Name = "Personal" },
                new PrivateCategoryResponseDto { Id = 2, Name = "Work" },
            };

            repositoryWrapperMock
                .Setup(r => r.PrivatePasswordCategoryRepository.GetAllAsync(It.IsAny<QueryOptions<PrivateCategory>>()))
                .ReturnsAsync(entities);

            mapperMock
                .Setup(m => m.Map<IEnumerable<PrivateCategoryResponseDto>>(entities))
                .Returns(dtoList);

            var query = new GetAllPasswordCategoriesQuery(userId);

            // Act
            var result = await handler.Handle(query, default);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(dtoList.Count, result.Value.Count());
            Assert.Equal("Personal", result.Value.First().Name);
        }

        [Fact]
        public async Task Handle_Should_ReturnEmptyList_WhenNoEntitiesFound()
        {
            // Arrange
            var userId = 99L;

            repositoryWrapperMock
                .Setup(r => r.PrivatePasswordCategoryRepository.GetAllAsync(It.IsAny<QueryOptions<PrivateCategory>>()))
                .ReturnsAsync(new List<PrivateCategory>());

            mapperMock
                .Setup(m => m.Map<IEnumerable<PrivateCategoryResponseDto>>(It.IsAny<IEnumerable<PrivateCategory>>()))
                .Returns(new List<PrivateCategoryResponseDto>());

            var query = new GetAllPasswordCategoriesQuery(userId);

            // Act
            var result = await handler.Handle(query, default);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value);
        }

        [Fact]
        public async Task Handle_Should_ReturnFail_WhenRepositoryThrowsException()
        {
            // Arrange
            repositoryWrapperMock
                .Setup(r => r.PrivatePasswordCategoryRepository.GetAllAsync(It.IsAny<QueryOptions<PrivateCategory>>()))
                .ThrowsAsync(new Exception("DB error"));

            var query = new GetAllPasswordCategoriesQuery(1);

            // Act
            var result = await handler.Handle(query, default);

            // Assert
            Assert.True(result.IsFailed);
            Assert.Equal(PasswordCategoriesConstants.ErrorMessage, result.Errors.First().Message);
        }
    }
}
