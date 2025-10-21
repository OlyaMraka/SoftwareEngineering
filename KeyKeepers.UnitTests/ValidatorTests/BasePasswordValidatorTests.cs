using FluentValidation.TestHelper;
using KeyKeepers.BLL.Constants;
using KeyKeepers.BLL.DTOs.Passwords;
using KeyKeepers.BLL.Validators.Passwords;
using Xunit;

namespace KeyKeepers.UnitTests.ValidatorTests
{
    public class BasePasswordValidatorTests
    {
        private readonly BasePasswordValidator validator;

        public BasePasswordValidatorTests()
        {
            validator = new BasePasswordValidator();
        }

        [Fact]
        public void Should_HaveError_When_AppName_IsEmpty()
        {
            // Arrange
            var model = new CreatePasswordRequest { AppName = " ", Login = "user", Password = "12345" };

            // Act
            var result = validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.AppName)
                  .WithErrorMessage(PasswordConstants.AppNameRequiredError);
        }

        [Fact]
        public void Should_HaveError_When_AppName_TooShort()
        {
            var model = new CreatePasswordRequest { AppName = "A", Login = "user", Password = "12345" };

            var result = validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.AppName)
                  .WithErrorMessage(PasswordConstants.MinAppNameLenghtError);
        }

        [Fact]
        public void Should_HaveError_When_AppName_TooLong()
        {
            var longName = new string('A', PasswordConstants.MaxAppNameLenght + 1);
            var model = new CreatePasswordRequest { AppName = longName, Login = "user", Password = "12345" };

            var result = validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.AppName)
                  .WithErrorMessage(PasswordConstants.MaxAppNameLenghtError);
        }

        [Fact]
        public void Should_HaveError_When_Password_IsEmpty()
        {
            var model = new CreatePasswordRequest { AppName = "Gmail", Login = "user", Password = " " };

            var result = validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.Password)
                  .WithErrorMessage(PasswordConstants.PasswordRequiredError);
        }

        [Fact]
        public void Should_HaveError_When_Password_TooLong()
        {
            var longPassword = new string('p', PasswordConstants.MaxPasswordLenght + 1);
            var model = new CreatePasswordRequest { AppName = "Gmail", Login = "user", Password = longPassword };

            var result = validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.Password)
                  .WithErrorMessage(PasswordConstants.MaxPasswordLenghtError);
        }

        [Fact]
        public void Should_HaveError_When_Login_IsEmpty()
        {
            var model = new CreatePasswordRequest { AppName = "Gmail", Login = " ", Password = "12345" };

            var result = validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.Login)
                  .WithErrorMessage(PasswordConstants.LoginRequiredError);
        }

        [Fact]
        public void Should_HaveError_When_Login_TooLong()
        {
            var longLogin = new string('L', PasswordConstants.MaxLoginLenght + 1);
            var model = new CreatePasswordRequest { AppName = "Gmail", Login = longLogin, Password = "12345" };

            var result = validator.TestValidate(model);

            result.ShouldHaveValidationErrorFor(x => x.Login)
                  .WithErrorMessage(PasswordConstants.MaxLoginLenghtError);
        }

        [Fact]
        public void Should_NotHaveError_When_Model_IsValid()
        {
            var model = new CreatePasswordRequest
            {
                AppName = "Telegram",
                Login = "user@mail.com",
                Password = "StrongPass123",
            };

            var result = validator.TestValidate(model);

            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
