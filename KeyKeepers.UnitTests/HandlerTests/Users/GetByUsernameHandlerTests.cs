using AutoMapper;
using FluentResults;
using KeyKeepers.BLL.Queries.Users.GetByUsername;
using KeyKeepers.BLL.Constants;
using KeyKeepers.BLL.DTOs.Users;
using KeyKeepers.DAL.Entities;
using KeyKeepers.DAL.Repositories.Interfaces.Base;
using KeyKeepers.DAL.Repositories.Options;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace KeyKeepers.UnitTests.HandlerTests.Users
{
    public class GetByUsernameHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> repoMock;
        private readonly Mock<IMapper> mapperMock;
        private readonly GetByUsernameHandler handler;

        public GetByUsernameHandlerTests()
        {
            repoMock = new Mock<IRepositoryWrapper>();
            mapperMock = new Mock<IMapper>();
            handler = new GetByUsernameHandler(repoMock.Object, mapperMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Fail_When_NoUsersFound()
        {
            // Arrange
            var query = new GetByUsernameQuery("NonExistentUser");

            repoMock.Setup(r => r.UserRepository.GetAllAsync(It.IsAny<QueryOptions<User>>()))
                    .ReturnsAsync(new List<User>());

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(UserConstants.DataNotFound, result.Errors[0].Message);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Success_When_UsersFound()
        {
            // Arrange
            var query = new GetByUsernameQuery("TestUser");

            var entities = new List<User>
            {
                new User { Id = 1, UserName = "TestUser1" },
                new User { Id = 2, UserName = "TestUser2" },
            };

            repoMock.Setup(r => r.UserRepository.GetAllAsync(It.IsAny<QueryOptions<User>>()))
                    .ReturnsAsync(entities);

            var responseDtos = new List<UserResponseDto>
            {
                new UserResponseDto { Id = 1, UserName = "TestUser1" },
                new UserResponseDto { Id = 2, UserName = "TestUser2" },
            };

            mapperMock.Setup(m => m.Map<IEnumerable<UserResponseDto>>(entities))
                      .Returns(responseDtos);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Count());
            Assert.Contains(result.Value, u => u.UserName == "TestUser1");
            Assert.Contains(result.Value, u => u.UserName == "TestUser2");
        }
    }
}
