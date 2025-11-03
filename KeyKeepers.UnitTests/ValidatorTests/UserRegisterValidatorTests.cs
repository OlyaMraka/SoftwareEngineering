using KeyKeepers.BLL.Constants;
using KeyKeepers.BLL.Validators.Users;
using FluentAssertions;
using FluentValidation.TestHelper;
using KeyKeepers.BLL.DTOs.Users;

namespace KeyKeepers.UnitTests.ValidatorTests
{
    public class UserRegisterValidatorTests
    {
        private readonly BaseUserValidator validator;

        public UserRegisterValidatorTests()
        {
            validator = new BaseUserValidator();
        }

        [Fact]
        public void Should_Have_Error_When_Name_Is_Empty()
        {
            var dto = new UserRegisterDto
            {
                Name = string.Empty,
                Surname = "Petrova",
                Email = "olga@example.com",
                UserName = "olga123",
                Password = "Password1!",
            };

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Name)
                  .WithErrorMessage(UserConstants.NameRequiredErrorMessage);
        }

        [Fact]
        public void Should_Have_Error_When_Name_TooShort()
        {
            var dto = new UserRegisterDto
            {
                Name = new string('A', UserConstants.MinNameLength - 1),
                Surname = "Petrova",
                Email = "olga@example.com",
                UserName = "olga123",
                Password = "Password1!",
            };

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Name)
                  .WithErrorMessage(UserConstants.MinNameLengthErrorMessage);
        }

        [Fact]
        public void Should_Have_Error_When_Name_TooLong()
        {
            var dto = new UserRegisterDto
            {
                Name = new string('A', UserConstants.MaxNameLength + 1),
                Surname = "Petrova",
                Email = "olga@example.com",
                UserName = "olga123",
                Password = "Password1!",
            };

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Name)
                  .WithErrorMessage(UserConstants.MaxNameLengthErrorMessage);
        }

        [Fact]
        public void Should_Have_Error_When_Password_Invalid()
        {
            var dto = new UserRegisterDto
            {
                Name = "Olga",
                Surname = "Petrova",
                Email = "olga@example.com",
                UserName = "olga123",
                Password = "abc",
            };

            var result = validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Password)
                  .WithErrorMessage(UserConstants.PasswordLengthErrorMessage);
        }

        [Fact]
        public void Should_Not_Have_Error_For_Valid_Data()
        {
            var dto = new UserRegisterDto
            {
                Name = "Olga",
                Surname = "Petrova",
                Email = "olga@example.com",
                UserName = "olga123",
                Password = "Password1!",
            };

            var result = validator.TestValidate(dto);

            result.IsValid.Should().BeTrue();
        }
    }
}
