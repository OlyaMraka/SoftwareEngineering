using AutoMapper;
using FluentResults;
using KeyKeepers.BLL.Queries.JoinRequests.GetByRecipientId;
using KeyKeepers.BLL.Constants;
using KeyKeepers.BLL.DTOs.JoinRequests;
using KeyKeepers.DAL.Entities;
using KeyKeepers.DAL.Repositories.Interfaces.Base;
using KeyKeepers.DAL.Repositories.Options;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace KeyKeepers.UnitTests.HandlerTests.JoinRequests
{
    public class GetByRecipientInHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> repoMock;
        private readonly Mock<IMapper> mapperMock;
        private readonly GetByRecipientInHandler handler;

        public GetByRecipientInHandlerTests()
        {
            repoMock = new Mock<IRepositoryWrapper>();
            mapperMock = new Mock<IMapper>();
            handler = new GetByRecipientInHandler(mapperMock.Object, repoMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Fail_When_NoJoinRequestsFound()
        {
            // Arrange
            var query = new GetByRecipientIdQuery(1);

            repoMock.Setup(r => r.JoinRequestRepository.GetAllAsync(It.IsAny<QueryOptions<JoinRequest>>()))
                    .ReturnsAsync(new List<JoinRequest>());

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(JoinRequestConstants.DataNotFoundError, result.Errors[0].Message);
        }

        [Fact]
        public async Task Handle_ShouldReturn_Success_When_JoinRequestsFound()
        {
            // Arrange
            var query = new GetByRecipientIdQuery(2);

            var entities = new List<JoinRequest>
            {
                new JoinRequest { Id = 1, CommunityId = 10, RecipientId = 2, SenderId = 3 },
                new JoinRequest { Id = 2, CommunityId = 11, RecipientId = 2, SenderId = 4 },
            };

            repoMock.Setup(r => r.JoinRequestRepository.GetAllAsync(It.IsAny<QueryOptions<JoinRequest>>()))
                    .ReturnsAsync(entities);

            var responseDtos = new List<JoinRequestResponseDto>
            {
                new JoinRequestResponseDto { Id = 1 },
                new JoinRequestResponseDto { Id = 2 },
            };

            mapperMock.Setup(m => m.Map<IEnumerable<JoinRequestResponseDto>>(entities))
                      .Returns(responseDtos);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Count());
            Assert.Contains(result.Value, r => r.Id == 1);
            Assert.Contains(result.Value, r => r.Id == 2);
        }
    }
}
