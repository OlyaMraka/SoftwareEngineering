using Moq;
using FluentAssertions;
using AutoMapper;
using KeyKeepers.BLL.Queries.CommunityUsers.GetByUserId;
using KeyKeepers.DAL.Repositories.Interfaces.Base;
using KeyKeepers.DAL.Entities;
using KeyKeepers.DAL.Repositories.Options;
using KeyKeepers.BLL.DTOs.CommunityUsers;
using KeyKeepers.BLL.DTOs.Communities;

namespace KeyKeepers.UnitTests.HandlerTests.CommunityUsers
{
    public class GetByUserIdHandlerTests
    {
        private readonly Mock<IMapper> mapperMock;
        private readonly Mock<IRepositoryWrapper> repoMock;
        private readonly GetByUserIdHandler handler;

        public GetByUserIdHandlerTests()
        {
            mapperMock = new Mock<IMapper>();
            repoMock = new Mock<IRepositoryWrapper>();
            handler = new GetByUserIdHandler(mapperMock.Object, repoMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Success_WithMappedResult()
        {
            // Arrange
            var userId = 10;
            var query = new GetByUserIdQuery(userId);

            var communityUsers = new List<CommunityUser>
            {
                new CommunityUser
                {
                    Id = 1,
                    UserId = userId,
                    Role = DAL.Enums.CommunityRole.Member,
                    Community = new Community { Id = 100, Name = "C# Developers" },
                },
                new CommunityUser
                {
                    Id = 2,
                    UserId = userId,
                    Role = DAL.Enums.CommunityRole.Owner,
                    Community = new Community { Id = 200, Name = "AI Enthusiasts" },
                },
            };

            var expectedDtos = new List<GetByUserIdResponseDto>
            {
                new GetByUserIdResponseDto
                {
                    Id = 1,
                    Community = new CommunityResponseDto { Id = 100, Name = "C# Developers" },
                    UserRole = DAL.Enums.CommunityRole.Member,
                },
                new GetByUserIdResponseDto
                {
                    Id = 2,
                    Community = new CommunityResponseDto { Id = 200, Name = "AI Enthusiasts" },
                    UserRole = DAL.Enums.CommunityRole.Owner,
                },
            };

            repoMock.Setup(r => r.CommunityUserRepository.GetAllAsync(It.IsAny<QueryOptions<CommunityUser>>()))
                    .ReturnsAsync(communityUsers);

            mapperMock.Setup(m => m.Map<IEnumerable<GetByUserIdResponseDto>>(communityUsers))
                      .Returns(expectedDtos);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEquivalentTo(expectedDtos);

            repoMock.Verify(r => r.CommunityUserRepository.GetAllAsync(It.IsAny<QueryOptions<CommunityUser>>()), Times.Once);
            mapperMock.Verify(m => m.Map<IEnumerable<GetByUserIdResponseDto>>(communityUsers), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturn_EmptyResult_WhenNoCommunityUsersFound()
        {
            // Arrange
            var query = new GetByUserIdQuery(99);

            repoMock.Setup(r => r.CommunityUserRepository.GetAllAsync(It.IsAny<QueryOptions<CommunityUser>>()))
                    .ReturnsAsync(new List<CommunityUser>());

            mapperMock.Setup(m => m.Map<IEnumerable<GetByUserIdResponseDto>>(It.IsAny<IEnumerable<CommunityUser>>()))
                      .Returns(new List<GetByUserIdResponseDto>());

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().BeEmpty();

            repoMock.Verify(r => r.CommunityUserRepository.GetAllAsync(It.IsAny<QueryOptions<CommunityUser>>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldThrowException_WhenRepositoryFails()
        {
            // Arrange
            var query = new GetByUserIdQuery(5);

            repoMock.Setup(r => r.CommunityUserRepository.GetAllAsync(It.IsAny<QueryOptions<CommunityUser>>()))
                    .ThrowsAsync(new System.Exception("Database connection failed"));

            // Act & Assert
            await Xunit.Record.ExceptionAsync(() => handler.Handle(query, CancellationToken.None))
                .ContinueWith(task =>
                {
                    task.Result.Should().BeOfType<System.Exception>();
                    task.Result.Message.Should().Be("Database connection failed");
                });

            repoMock.Verify(r => r.CommunityUserRepository.GetAllAsync(It.IsAny<QueryOptions<CommunityUser>>()), Times.Once);
        }
    }
}
