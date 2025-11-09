using System;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using KeyKeepers.BLL.Services;
using System.Security.Cryptography;
using Xunit;

namespace KeyKeepers.UnitTests.ServiceTests
{
    public class EncryptionServiceTests
    {
        private readonly string validBase64Key;
        private readonly IConfiguration configuration;

        public EncryptionServiceTests()
        {
            // Створюємо 32-байтний ключ (256-біт)
            var keyBytes = new byte[32];
            new Random().NextBytes(keyBytes);
            validBase64Key = Convert.ToBase64String(keyBytes);

            var inMemorySettings = new Dictionary<string, string?>
            {
                { "Encryption:KeyBase64", validBase64Key },
            };

            configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenKeyMissing()
        {
            // Arrange
            var config = new ConfigurationBuilder().Build();

            // Act
            Action act = () => new EncryptionService(config);

            // Assert
            act.Should()
               .Throw<InvalidOperationException>()
               .WithMessage("*Encryption key not found*");
        }

        [Fact]
        public void Constructor_ShouldThrow_WhenKeyInvalidLength()
        {
            // Arrange
            var invalidKey = Convert.ToBase64String(new byte[16]); // 128 біт
            var config = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "Encryption:KeyBase64", invalidKey },
                })
                .Build();

            // Act
            Action act = () => new EncryptionService(config);

            // Assert
            act.Should()
               .Throw<InvalidOperationException>()
               .WithMessage("*32 bytes*");
        }

        [Fact]
        public void Encrypt_And_Decrypt_ShouldWorkCorrectly()
        {
            // Arrange
            var service = new EncryptionService(configuration);
            var text = "Hello world!";

            // Act
            var cipher = service.Encrypt(text);
            var decrypted = service.Decrypt(cipher);

            // Assert
            decrypted.Should().Be(text);
            cipher.Should().NotBeNullOrEmpty();
            cipher.Should().NotContain(text); // Перевіряємо, що не просто Base64 plain text
        }

        [Fact]
        public void Encrypt_And_Decrypt_WithAssociatedData_ShouldWorkCorrectly()
        {
            // Arrange
            var service = new EncryptionService(configuration);
            var text = "Sensitive Data";
            var associated = "metadata";

            // Act
            var cipher = service.Encrypt(text, associated);
            var decrypted = service.Decrypt(cipher, associated);

            // Assert
            decrypted.Should().Be(text);
        }

        [Fact]
        public void Decrypt_ShouldThrow_WhenAssociatedDataIsWrong()
        {
            // Arrange
            var service = new EncryptionService(configuration);
            var text = "Test message";
            var cipher = service.Encrypt(text, "correctAD");

            // Act
            Action act = () => service.Decrypt(cipher, "wrongAD");

            // Assert
            act.Should().Throw<CryptographicException>();
        }

        [Fact]
        public void Encrypt_ShouldThrow_WhenPlainTextIsNull()
        {
            // Arrange
            var service = new EncryptionService(configuration);

            // Act
            Action act = () => service.Encrypt(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Decrypt_ShouldThrow_WhenCipherTextIsNull()
        {
            // Arrange
            var service = new EncryptionService(configuration);

            // Act
            Action act = () => service.Decrypt(null!);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Decrypt_ShouldThrow_WhenCipherTextIsCorrupted()
        {
            // Arrange
            var service = new EncryptionService(configuration);
            var text = "Some text";
            var cipher = service.Encrypt(text);

            // Пошкодимо кілька байтів
            var corruptedBytes = Convert.FromBase64String(cipher);
            corruptedBytes[5] ^= 0xFF;
            var corrupted = Convert.ToBase64String(corruptedBytes);

            // Act
            Action act = () => service.Decrypt(corrupted);

            // Assert
            act.Should().Throw<CryptographicException>();
        }
    }
}
