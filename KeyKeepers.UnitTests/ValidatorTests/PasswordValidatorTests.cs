using FluentValidation.TestHelper;
using KeyKeepers.BLL.Constants;
using KeyKeepers.BLL.Validators.Users;
using Xunit;

namespace KeyKeepers.UnitTests.ValidatorTests
{
    public class PasswordValidatorTests
    {
        private readonly PasswordValidator validator;

        public PasswordValidatorTests()
        {
            validator = new PasswordValidator();
        }

        [Fact]
        public void Should_Have_Error_When_Password_Is_Empty()
        {
            // Arrange
            string password = string.Empty;

            // Act
            var result = validator.TestValidate(password);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x)
                .WithErrorMessage(UserConstants.PasswordRequiredErrorMessage);
        }

        [Fact]
        public void Should_Have_Error_When_Password_Is_Too_Short()
        {
            // Arrange
            string password = new string('a', UserConstants.MinPasswordLength - 1);

            // Act
            var result = validator.TestValidate(password);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x)
                .WithErrorMessage(UserConstants.PasswordLengthErrorMessage);
        }

        [Fact]
        public void Should_Have_Error_When_Password_Has_No_Uppercase()
        {
            // Arrange
            string password = "password1!";

            // Act
            var result = validator.TestValidate(password);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x)
                .WithErrorMessage(UserConstants.PasswordUppercaseLetterErrorMessage);
        }

        [Fact]
        public void Should_Have_Error_When_Password_Has_No_Digit()
        {
            // Arrange
            string password = "Password!";

            // Act
            var result = validator.TestValidate(password);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x)
                .WithErrorMessage(UserConstants.PasswordDigitErrorMessage);
        }

        [Fact]
        public void Should_Have_Error_When_Password_Has_No_SpecialCharacter()
        {
            // Arrange
            string password = "Password1";

            // Act
            var result = validator.TestValidate(password);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x)
                .WithErrorMessage(UserConstants.PasswordSpecialCharacterErrorMessage);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Password_Is_Valid()
        {
            // Arrange
            string password = "ValidPass1!";

            // Act
            var result = validator.TestValidate(password);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x);
        }
    }
}
