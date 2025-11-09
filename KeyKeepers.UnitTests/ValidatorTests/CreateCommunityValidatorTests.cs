using FluentValidation.TestHelper;
using KeyKeepers.BLL.Commands.Communities.Create;
using KeyKeepers.BLL.Constants;
using KeyKeepers.BLL.DTOs.Communities;
using KeyKeepers.BLL.Validators.Communities;
using Xunit;

namespace KeyKeepers.UnitTests.ValidatorTests
{
    public class CreateCommunityValidatorTests
    {
        private readonly CreateCommunityValidator validator;

        public CreateCommunityValidatorTests()
        {
            validator = new CreateCommunityValidator();
        }

        [Fact]
        public void Should_Have_Error_When_OwnerId_Is_Empty()
        {
            // Arrange
            var dto = new CreateCommunityDto
            {
                OwnerId = 0,
                Name = "Valid Name",
            };
            var model = new CreateCommunityCommand(dto);

            // Act
            var result = validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.RequestDto.OwnerId)
                .WithErrorMessage(CommunityConstants.OwnerIdRequiredError);
        }

        [Fact]
        public void Should_Have_Error_When_Name_Is_Empty()
        {
            // Arrange
            var dto = new CreateCommunityDto
            {
                OwnerId = 123,
                Name = string.Empty,
            };
            var model = new CreateCommunityCommand(dto);

            // Act
            var result = validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.RequestDto.Name)
                .WithErrorMessage(CommunityConstants.CommunityNameRequiredError);
        }

        [Fact]
        public void Should_Have_Error_When_Name_Is_Too_Short()
        {
            // Arrange
            var dto = new CreateCommunityDto
            {
                OwnerId = 123,
                Name = new string('A', CommunityConstants.MinNameLenght - 1),
            };
            var model = new CreateCommunityCommand(dto);

            // Act
            var result = validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.RequestDto.Name)
                .WithErrorMessage(CommunityConstants.MinNameLenghtError);
        }

        [Fact]
        public void Should_Have_Error_When_Name_Is_Too_Long()
        {
            // Arrange
            var dto = new CreateCommunityDto
            {
                OwnerId = 123,
                Name = new string('X', CommunityConstants.MaxNameLenght + 1),
            };
            var model = new CreateCommunityCommand(dto);

            // Act
            var result = validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.RequestDto.Name)
                .WithErrorMessage(CommunityConstants.MaxNameLenghtError);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Model_Is_Valid()
        {
            // Arrange
            var dto = new CreateCommunityDto
            {
                OwnerId = 123,
                Name = "C# Developers",
            };
            var model = new CreateCommunityCommand(dto);

            // Act
            var result = validator.TestValidate(model);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.RequestDto.OwnerId);
            result.ShouldNotHaveValidationErrorFor(x => x.RequestDto.Name);
        }
    }
}
