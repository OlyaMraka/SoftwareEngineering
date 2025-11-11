using AutoMapper;
using FluentResults;
using KeyKeepers.BLL.Commands.JoinRequests.Create;
using KeyKeepers.BLL.Constants;
using KeyKeepers.BLL.DTOs.JoinRequests;
using KeyKeepers.DAL.Entities;
using KeyKeepers.DAL.Repositories.Interfaces.Base;
using KeyKeepers.DAL.Repositories.Options;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace KeyKeepers.UnitTests.HandlerTests.JoinRequests
{
    public class CreateJoinRequestHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> repoMock;
        private readonly Mock<IMapper> mapperMock;
        private readonly CreateJoinRequestHandler handler;

        public CreateJoinRequestHandlerTests()
        {
            repoMock = new Mock<IRepositoryWrapper>();
            mapperMock = new Mock<IMapper>();
            handler = new CreateJoinRequestHandler(mapperMock.Object, repoMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Fail_When_RequestAlreadyExists()
        {
            // Arrange
            var dto = new CreateRequestDto() { CommunityId = 1, RecipientId = 2, SenderId = 3 };
            var command = new CreateJoinRequestCommand(dto);

            repoMock.Setup(r => r.JoinRequestRepository.GetFirstOrDefaultAsync(It.IsAny<QueryOptions<JoinRequest>>()))
                    .ReturnsAsync(new JoinRequest { Id = 1 });

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(JoinRequestConstants.RequestAlreadyExistsError, result.Errors[0].Message);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Fail_When_SaveChangesFails()
        {
            // Arrange
            var dto = new CreateRequestDto { CommunityId = 1, RecipientId = 2, SenderId = 3 };
            var command = new CreateJoinRequestCommand(dto);

            repoMock.Setup(r => r.JoinRequestRepository.GetFirstOrDefaultAsync(It.IsAny<QueryOptions<JoinRequest>>()))
                    .ReturnsAsync((JoinRequest?)null);

            var newEntity = new JoinRequest { Id = 10 };
            mapperMock.Setup(m => m.Map<JoinRequest>(command)).Returns(newEntity);

            repoMock.Setup(r => r.JoinRequestRepository.CreateAsync(newEntity))
                    .ReturnsAsync(newEntity);

            repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0); // simulate DB error

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(JoinRequestConstants.DbSaveError, result.Errors[0].Message);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Success_When_RequestCreated()
        {
            // Arrange
            var dto = new CreateRequestDto { CommunityId = 1, RecipientId = 2, SenderId = 3 };
            var command = new CreateJoinRequestCommand(dto);

            repoMock.Setup(r => r.JoinRequestRepository.GetFirstOrDefaultAsync(It.IsAny<QueryOptions<JoinRequest>>()))
                    .ReturnsAsync((JoinRequest?)null);

            var newEntity = new JoinRequest { Id = 10 };
            mapperMock.Setup(m => m.Map<JoinRequest>(command)).Returns(newEntity);

            repoMock.Setup(r => r.JoinRequestRepository.CreateAsync(newEntity)).ReturnsAsync(newEntity);
            repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

            var responseDto = new JoinRequestResponseDto { Id = 10 };
            mapperMock.Setup(m => m.Map<JoinRequestResponseDto>(newEntity)).Returns(responseDto);

            var communityUser = new CommunityUser
            {
                Id = 1,
                User = new User { Id = 3, UserName = "TestUser" },
            };
            repoMock.Setup(r => r.CommunityUserRepository.GetFirstOrDefaultAsync(It.IsAny<QueryOptions<CommunityUser>>()))
                    .ReturnsAsync(communityUser);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(responseDto.Id, result.Value.Id);
            Assert.Equal("TestUser", result.Value.SenderUsername);

            repoMock.Verify(r => r.JoinRequestRepository.CreateAsync(newEntity), Times.Once);
            repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}
