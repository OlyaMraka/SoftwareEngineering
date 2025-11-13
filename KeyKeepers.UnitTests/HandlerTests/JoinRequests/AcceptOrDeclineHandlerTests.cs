using AutoMapper;
using FluentResults;
using KeyKeepers.BLL.Commands.JoinRequests.Accept;
using KeyKeepers.BLL.Constants;
using KeyKeepers.BLL.DTOs.JoinRequests;
using KeyKeepers.DAL.Entities;
using KeyKeepers.DAL.Enums;
using KeyKeepers.DAL.Repositories.Interfaces.Base;
using KeyKeepers.DAL.Repositories.Options;
using Moq;
using Xunit;

namespace KeyKeepers.UnitTests.HandlerTests.JoinRequests
{
    public class AcceptOrDeclineHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> repoMock;
        private readonly Mock<IMapper> mapperMock;
        private readonly AcceptOrDeclineHandler handler;

        public AcceptOrDeclineHandlerTests()
        {
            repoMock = new Mock<IRepositoryWrapper>();
            mapperMock = new Mock<IMapper>();
            handler = new AcceptOrDeclineHandler(repoMock.Object, mapperMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Fail_When_RequestNotFound()
        {
            // Arrange
            var dto = new AcceptOrDeclineDto { Id = 1, Status = RequestStatus.Accepted };
            var command = new AcceptOrDeclineCommand(dto);

            repoMock.Setup(r => r.JoinRequestRepository.GetFirstOrDefaultAsync(It.IsAny<QueryOptions<JoinRequest>>()))
                    .ReturnsAsync((JoinRequest?)null);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(JoinRequestConstants.RequestNotFound, result.Errors[0].Message);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Fail_When_DbSaveFails_OnDecline()
        {
            // Arrange
            var joinRequest = new JoinRequest { Id = 1, Status = RequestStatus.Pending };
            var dto = new AcceptOrDeclineDto { Id = 1, Status = RequestStatus.Declined };
            var command = new AcceptOrDeclineCommand(dto);

            repoMock.Setup(r => r.JoinRequestRepository.GetFirstOrDefaultAsync(It.IsAny<QueryOptions<JoinRequest>>()))
                    .ReturnsAsync(joinRequest);

            repoMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(0);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(JoinRequestConstants.DbSaveError, result.Errors[0].Message);

            repoMock.Verify(r => r.JoinRequestRepository.Update(joinRequest), Times.Once);
            repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Success_When_RequestDeclined()
        {
            // Arrange
            var joinRequest = new JoinRequest { Id = 1, Status = RequestStatus.Pending };
            var dto = new AcceptOrDeclineDto { Id = 1, Status = RequestStatus.Declined };
            var command = new AcceptOrDeclineCommand(dto);

            repoMock.Setup(r => r.JoinRequestRepository.GetFirstOrDefaultAsync(It.IsAny<QueryOptions<JoinRequest>>()))
                    .ReturnsAsync(joinRequest);

            repoMock.SetupSequence(r => r.SaveChangesAsync())
                    .ReturnsAsync(1);

            var responseDto = new JoinRequestResponseDto { Id = 1, Status = RequestStatus.Declined };
            mapperMock.Setup(m => m.Map<JoinRequestResponseDto>(joinRequest)).Returns(responseDto);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(RequestStatus.Declined, result.Value.Status);

            repoMock.Verify(r => r.JoinRequestRepository.Update(joinRequest), Times.Once);
            repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Fail_When_DbSaveFails_OnAccept()
        {
            // Arrange
            var joinRequest = new JoinRequest { Id = 1, CommunityId = 100, RecipientId = 200, Status = RequestStatus.Pending };
            var dto = new AcceptOrDeclineDto { Id = 1, Status = RequestStatus.Accepted };
            var command = new AcceptOrDeclineCommand(dto);

            repoMock.Setup(r => r.JoinRequestRepository.GetFirstOrDefaultAsync(It.IsAny<QueryOptions<JoinRequest>>()))
                    .ReturnsAsync(joinRequest);

            // перший SaveChanges — після оновлення JoinRequest
            repoMock.SetupSequence(r => r.SaveChangesAsync())
                    .ReturnsAsync(0); // імітація помилки

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(JoinRequestConstants.DbSaveError, result.Errors[0].Message);

            repoMock.Verify(r => r.JoinRequestRepository.Update(joinRequest), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Success_When_RequestAccepted()
        {
            // Arrange
            var joinRequest = new JoinRequest { Id = 1, CommunityId = 10, RecipientId = 20, Status = RequestStatus.Pending };
            var dto = new AcceptOrDeclineDto { Id = 1, Status = RequestStatus.Accepted };
            var command = new AcceptOrDeclineCommand(dto);

            repoMock.Setup(r => r.JoinRequestRepository.GetFirstOrDefaultAsync(It.IsAny<QueryOptions<JoinRequest>>()))
                    .ReturnsAsync(joinRequest);

            repoMock.SetupSequence(r => r.SaveChangesAsync())
                    .ReturnsAsync(1) // після оновлення JoinRequest
                    .ReturnsAsync(1); // після створення CommunityUser

            repoMock.Setup(r => r.CommunityUserRepository.CreateAsync(It.IsAny<CommunityUser>()))
                    .ReturnsAsync(new CommunityUser { Id = 99 });

            var responseDto = new JoinRequestResponseDto { Id = 1, Status = RequestStatus.Accepted };
            mapperMock.Setup(m => m.Map<JoinRequestResponseDto>(joinRequest)).Returns(responseDto);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(RequestStatus.Accepted, result.Value.Status);

            repoMock.Verify(r => r.JoinRequestRepository.Update(joinRequest), Times.Once);
            repoMock.Verify(r => r.CommunityUserRepository.CreateAsync(It.IsAny<CommunityUser>()), Times.Once);
            repoMock.Verify(r => r.SaveChangesAsync(), Times.Exactly(2));
        }
    }
}
