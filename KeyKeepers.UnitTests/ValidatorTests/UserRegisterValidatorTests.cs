using KeyKeepers.BLL.Commands.Users.Create;
using KeyKeepers.BLL.Constants;
using KeyKeepers.BLL.Validators.Users;
using FluentAssertions;
using FluentValidation.TestHelper;
using KeyKeepers.BLL.DTOs.Users;
using Xunit;

namespace KeyKeepers.UnitTests.ValidatorTests
{
    public class UserRegisterValidatorTests
    {
        private readonly UserRegisterValidator validator;

        public UserRegisterValidatorTests()
        {
            validator = new UserRegisterValidator();
        }

        [Fact]
        public void Should_Have_Error_When_Name_Is_Empty()
        {
            var command = new CreateUserCommand(new UserRegisterDto
            {
                Name = string.Empty,
                Surname = "Petrova",
                Email = "olga@example.com",
                UserName = "olga123",
                Password = "Password1!",
            });

            var result = validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.RegisterDto.Name)
                  .WithErrorMessage(UserConstants.NameRequiredErrorMessage);
        }

        [Fact]
        public void Should_Have_Error_When_Name_TooShort()
        {
            var command = new CreateUserCommand(new UserRegisterDto
            {
                Name = new string('A', UserConstants.MinNameLength - 1),
                Surname = "Petrova",
                Email = "olga@example.com",
                UserName = "olga123",
                Password = "Password1!",
            });

            var result = validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.RegisterDto.Name)
                  .WithErrorMessage(UserConstants.MinNameLengthErrorMessage);
        }

        [Fact]
        public void Should_Have_Error_When_Name_TooLong()
        {
            var command = new CreateUserCommand(new UserRegisterDto
            {
                Name = new string('A', UserConstants.MaxNameLength + 1),
                Surname = "Petrova",
                Email = "olga@example.com",
                UserName = "olga123",
                Password = "Password1!",
            });

            var result = validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.RegisterDto.Name)
                  .WithErrorMessage(UserConstants.MaxNameLengthErrorMessage);
        }

        [Fact]
        public void Should_Have_Error_When_Password_Invalid()
        {
            var command = new CreateUserCommand(new UserRegisterDto
            {
                Name = "Olga",
                Surname = "Petrova",
                Email = "olga@example.com",
                UserName = "olga123",
                Password = "abc",
            });

            var result = validator.TestValidate(command);

            result.ShouldHaveValidationErrorFor(x => x.RegisterDto.Password)
                  .WithErrorMessage(UserConstants.PasswordLengthErrorMessage);
        }

        [Fact]
        public void Should_Not_Have_Error_For_Valid_Data()
        {
            var command = new CreateUserCommand(new UserRegisterDto
            {
                Name = "Olga",
                Surname = "Petrova",
                Email = "olga@example.com",
                UserName = "olga123",
                Password = "Password1!",
            });

            var result = validator.TestValidate(command);

            result.IsValid.Should().BeTrue();
        }
    }
}
