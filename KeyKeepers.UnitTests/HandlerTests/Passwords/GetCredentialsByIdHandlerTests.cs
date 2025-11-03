using Xunit;
using Moq;
using AutoMapper;
using FluentResults;
using KeyKeepers.BLL.Queries.Passwords.GetAllById;
using KeyKeepers.BLL.DTOs.Passwords;
using KeyKeepers.DAL.Entities;
using KeyKeepers.DAL.Repositories.Interfaces.Base;
using KeyKeepers.DAL.Repositories.Options;

namespace KeyKeepers.UnitTests.Handlers.Passwords
{
    public class GetCredentialsByIdHandlerTests
    {
        private readonly Mock<IRepositoryWrapper> repositoryWrapperMock;
        private readonly Mock<IMapper> mapperMock;
        private readonly GetCredentialsByIdHandler handler;

        public GetCredentialsByIdHandlerTests()
        {
            repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            mapperMock = new Mock<IMapper>();
            handler = new GetCredentialsByIdHandler(mapperMock.Object, repositoryWrapperMock.Object);
        }

        [Fact]
        public async Task Handle_Should_ReturnMappedResult_WhenDataExists()
        {
            // Arrange
            var categoryId = 5L;
            var credentials = new List<Credentials>
            {
                new Credentials { Id = 1, AppName = "Gmail", Login = "test@gmail.com", CategoryId = categoryId },
                new Credentials { Id = 2, AppName = "Facebook", Login = "user@fb.com", CategoryId = categoryId },
            };

            var responseDtos = new List<PasswordResponse>
            {
                new PasswordResponse { Id = 1, AppName = "Gmail", Login = "test@gmail.com" },
                new PasswordResponse { Id = 2, AppName = "Facebook", Login = "user@fb.com" },
            };

            repositoryWrapperMock
                .Setup(r => r.PasswordRepository.GetAllAsync(It.IsAny<QueryOptions<Credentials>>()))
                .ReturnsAsync(credentials);

            mapperMock
                .Setup(m => m.Map<IEnumerable<PasswordResponse>>(credentials))
                .Returns(responseDtos);

            var query = new GetCredentialsByIdQuery(categoryId);

            // Act
            var result = await handler.Handle(query, default);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Count());
            Assert.Equal("Gmail", result.Value.First().AppName);
        }

        [Fact]
        public async Task Handle_Should_ReturnEmptyList_WhenNoCredentialsExist()
        {
            // Arrange
            var categoryId = 10L;

            repositoryWrapperMock
                .Setup(r => r.PasswordRepository.GetAllAsync(It.IsAny<QueryOptions<Credentials>>()))
                .ReturnsAsync(new List<Credentials>());

            mapperMock
                .Setup(m => m.Map<IEnumerable<PasswordResponse>>(It.IsAny<IEnumerable<Credentials>>()))
                .Returns(new List<PasswordResponse>());

            var query = new GetCredentialsByIdQuery(categoryId);

            // Act
            var result = await handler.Handle(query, default);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value);
        }

        [Fact]
        public async Task Handle_Should_ThrowException_WhenRepositoryFails()
        {
            // Arrange
            repositoryWrapperMock
                .Setup(r => r.PasswordRepository.GetAllAsync(It.IsAny<QueryOptions<Credentials>>()))
                .ThrowsAsync(new Exception("Database error"));

            var query = new GetCredentialsByIdQuery(999);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(async () =>
            {
                await handler.Handle(query, default);
            });
        }

        [Fact]
        public async Task Handle_Should_CallMapper_Once_WhenRepositoryReturnsData()
        {
            // Arrange
            var credentials = new List<Credentials>
            {
                new Credentials { Id = 1, AppName = "TestApp" },
            };

            repositoryWrapperMock
                .Setup(r => r.PasswordRepository.GetAllAsync(It.IsAny<QueryOptions<Credentials>>()))
                .ReturnsAsync(credentials);

            mapperMock
                .Setup(m => m.Map<IEnumerable<PasswordResponse>>(credentials))
                .Returns(new List<PasswordResponse> { new PasswordResponse { Id = 1, AppName = "TestApp" } });

            var query = new GetCredentialsByIdQuery(10);

            // Act
            await handler.Handle(query, default);

            // Assert
            mapperMock.Verify(m => m.Map<IEnumerable<PasswordResponse>>(credentials), Times.Once);
        }
    }
}
