using Moq;
using KeyKeepers.DAL.Repositories.Interfaces.Base;
using KeyKeepers.BLL.Services;
using FluentAssertions;
using KeyKeepers.DAL.Entities;
using Microsoft.Extensions.Configuration;

namespace KeyKeepers.UnitTests.ServiceTests
{
    public class TokenServiceTests
    {
        private readonly Mock<IRepositoryWrapper> repositoryWrapperMock;
        private readonly Mock<IConfiguration> configurationMock;
        private readonly TokenService tokenService;

        public TokenServiceTests()
        {
            repositoryWrapperMock = new Mock<IRepositoryWrapper>();
            configurationMock = new Mock<IConfiguration>();

            configurationMock.Setup(c => c["Jwt:Key"]).Returns("SuperSecretKey1234567890SuperSecret!");
            configurationMock.Setup(c => c["Jwt:Issuer"]).Returns("TestIssuer");
            configurationMock.Setup(c => c["Jwt:Audience"]).Returns("TestAudience");

            tokenService = new TokenService(repositoryWrapperMock.Object, configurationMock.Object);
        }

        [Fact]
        public void GenerateJwtToken_ShouldReturn_ValidToken()
        {
            var user = new User { Id = 1, Email = "test@example.com" };

            var token = tokenService.GenerateJwtToken(user);

            token.Should().NotBeNullOrEmpty();
            token.Should().Contain(".");
        }

        [Fact]
        public void GenerateRefreshToken_ShouldReturn_ValidRefreshToken()
        {
            var user = new User { Id = 1 };

            var refreshToken = tokenService.GenerateRefreshToken(user);

            refreshToken.Token.Should().NotBeNullOrEmpty();
            refreshToken.UserId.Should().Be(user.Id);
            refreshToken.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
            refreshToken.CreatedAt.Should().BeOnOrBefore(DateTime.UtcNow);
        }

        [Fact]
        public async Task RevokeRefreshToken_ShouldSetRevokedAndSaveChanges()
        {
            var refreshToken = new RefreshToken();

            repositoryWrapperMock
                .Setup(r => r.RefreshTokenRepository.Update(It.IsAny<RefreshToken>()));
            repositoryWrapperMock
                .Setup(r => r.SaveChangesAsync())
                .ReturnsAsync(1)
                .Verifiable();

            await tokenService.RevokeRefreshToken(refreshToken);

            refreshToken.Revoked.Should().NotBeNull();
            repositoryWrapperMock.Verify(r => r.RefreshTokenRepository.Update(refreshToken), Times.Once);
            repositoryWrapperMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}
