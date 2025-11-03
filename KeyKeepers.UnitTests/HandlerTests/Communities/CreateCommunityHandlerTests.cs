using AutoMapper;
using FluentResults;
using KeyKeepers.BLL.Commands.Communities.Create;
using KeyKeepers.BLL.Constants;
using KeyKeepers.BLL.DTOs.Communities;
using KeyKeepers.BLL.DTOs.CommunityUsers;
using KeyKeepers.DAL.Entities;
using KeyKeepers.DAL.Enums;
using KeyKeepers.DAL.Repositories.Interfaces.Base;
using KeyKeepers.DAL.Repositories.Options;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace KeyKeepers.UnitTests.HandlerTests
{
    public class CreateCommunityHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> repoMock;
        private readonly Mock<IMapper> mapperMock;
        private readonly CreateCommunityHandler handler;

        public CreateCommunityHandlerTests()
        {
            repoMock = new Mock<IRepositoryWrapper>();
            mapperMock = new Mock<IMapper>();

            handler = new CreateCommunityHandler(repoMock.Object, mapperMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Fail_When_CommunityAlreadyExists()
        {
            // Arrange
            var command = new CreateCommunityCommand(new CreateCommunityDto
            {
                Name = "Tech Club",
                OwnerId = 5,
            });

            repoMock.Setup(r => r.CommunityRepository.GetFirstOrDefaultAsync(It.IsAny<QueryOptions<Community>>()))
                    .ReturnsAsync(new Community { Id = 1, Name = "Tech Club" });

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(CommunityConstants.AlreadyExistsError, result.Errors[0].Message);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Fail_When_FirstSaveChangesFails()
        {
            // Arrange
            var command = new CreateCommunityCommand(new CreateCommunityDto
            {
                Name = "Science Hub",
                OwnerId = 7,
            });

            var community = new Community { Id = 1, Name = "Science Hub" };

            repoMock.Setup(r => r.CommunityRepository.GetFirstOrDefaultAsync(It.IsAny<QueryOptions<Community>>()))
                    .ReturnsAsync((Community?)null);

            mapperMock.Setup(m => m.Map<Community>(command.RequestDto)).Returns(community);

            repoMock.Setup(r => r.CommunityRepository.CreateAsync(community))
                    .ReturnsAsync(community);

            repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0); // simulate DB error

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(CommunityConstants.DbSaveError, result.Errors[0].Message);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Fail_When_SecondSaveChangesFails()
        {
            // Arrange
            var command = new CreateCommunityCommand(new CreateCommunityDto
            {
                Name = "Gaming Club",
                OwnerId = 10,
            });

            var community = new Community { Id = 1, Name = "Gaming Club" };
            var communityUser = new CommunityUser { CommunityId = 1, UserId = 10, Role = CommunityRole.Owner };

            repoMock.Setup(r => r.CommunityRepository.GetFirstOrDefaultAsync(It.IsAny<QueryOptions<Community>>()))
                    .ReturnsAsync((Community?)null);

            mapperMock.Setup(m => m.Map<Community>(command.RequestDto)).Returns(community);

            repoMock.Setup(r => r.CommunityRepository.CreateAsync(community)).ReturnsAsync(community);
            repoMock.SetupSequence(r => r.SaveChangesAsync())
                    .ReturnsAsync(1) // first success
                    .ReturnsAsync(0); // second fails

            repoMock.Setup(r => r.CommunityUserRepository.CreateAsync(It.IsAny<CommunityUser>()))
                    .ReturnsAsync(communityUser);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(CommunityConstants.DbSaveError, result.Errors[0].Message);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Success_When_CommunityCreated()
        {
            // Arrange
            var command = new CreateCommunityCommand(new CreateCommunityDto
            {
                Name = "Developers",
                OwnerId = 15,
            });

            var community = new Community { Id = 1, Name = "Developers" };
            var communityUser = new CommunityUser
            {
                CommunityId = 1,
                UserId = 15,
                Role = CommunityRole.Owner,
            };

            repoMock.Setup(r => r.CommunityRepository.GetFirstOrDefaultAsync(It.IsAny<QueryOptions<Community>>()))
                    .ReturnsAsync((Community?)null);

            mapperMock.Setup(m => m.Map<Community>(command.RequestDto)).Returns(community);

            repoMock.Setup(r => r.CommunityRepository.CreateAsync(community))
                    .ReturnsAsync(community);

            repoMock.SetupSequence(r => r.SaveChangesAsync())
                    .ReturnsAsync(1)
                    .ReturnsAsync(1);

            repoMock.Setup(r => r.CommunityUserRepository.CreateAsync(It.IsAny<CommunityUser>()))
                    .ReturnsAsync(communityUser);

            var expectedResponse = new CreateCommunityResponseDto
            {
                NewCommunity = new CommunityResponseDto { Id = community.Id, Name = community.Name },
                Owner = new CommunityUserResponseDto { UserId = 15, Role = CommunityRole.Owner },
            };

            mapperMock.Setup(m => m.Map<CommunityResponseDto>(community))
                      .Returns(expectedResponse.NewCommunity);

            mapperMock.Setup(m => m.Map<CommunityUserResponseDto>(It.IsAny<CommunityUser>()))
                      .Returns(expectedResponse.Owner);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(expectedResponse.NewCommunity.Id, result.Value.NewCommunity.Id);
            Assert.Equal(expectedResponse.NewCommunity.Name, result.Value.NewCommunity.Name);
            Assert.Equal(expectedResponse.Owner.UserId, result.Value.Owner.UserId);

            // Verify calls
            repoMock.Verify(r => r.CommunityRepository.CreateAsync(community), Times.Once);
            repoMock.Verify(r => r.CommunityUserRepository.CreateAsync(It.IsAny<CommunityUser>()), Times.Once);
            repoMock.Verify(r => r.SaveChangesAsync(), Times.Exactly(2));
        }
    }
}
