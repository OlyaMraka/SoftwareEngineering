using AutoMapper;
using KeyKeepers.BLL.Constants;
using KeyKeepers.BLL.DTOs.Users;
using KeyKeepers.BLL.Queries.Users.GetById;
using KeyKeepers.DAL.Entities;
using KeyKeepers.DAL.Repositories.Interfaces.Base;
using KeyKeepers.DAL.Repositories.Options;
using Moq;

namespace KeyKeepers.UnitTests.HandlerTests.Users
{
    public class GetUserByIdQueryHandlerTests
    {
        private readonly Mock<IMapper> mapperMock;
        private readonly Mock<IRepositoryWrapper> repoMock;
        private readonly GetUserByIdQueryHandler handler;

        public GetUserByIdQueryHandlerTests()
        {
            mapperMock = new Mock<IMapper>();
            repoMock = new Mock<IRepositoryWrapper>();

            handler = new GetUserByIdQueryHandler(
                mapperMock.Object,
                repoMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnFail_WhenUserNotFound()
        {
            // Arrange
            long id = 10;
            var query = new GetUserByIdQuery(id);

            repoMock.Setup(r => r.UserRepository
                .GetFirstOrDefaultAsync(It.IsAny<QueryOptions<User>>()))
                .ReturnsAsync((User?)null);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains(UserConstants.UserNotFound, result.Errors[0].Message);
        }

        [Fact]
        public async Task Handle_ShouldReturnSuccess_WhenUserFound()
        {
            // Arrange
            long id = 5;
            var query = new GetUserByIdQuery(id);

            var userEntity = new User { Id = id, Email = "test@gmail.com" };
            repoMock.Setup(r => r.UserRepository
                .GetFirstOrDefaultAsync(It.IsAny<QueryOptions<User>>()))
                .ReturnsAsync(userEntity);

            var mapped = new UserResponseDto { Id = id, Email = "test@gmail.com" };
            mapperMock.Setup(m => m.Map<UserResponseDto>(userEntity)).Returns(mapped);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(id, result.Value.Id);
            Assert.Equal("test@gmail.com", result.Value.Email);
        }
    }
}
