using FluentValidation.TestHelper;
using KeyKeepers.BLL.Commands.Communities.Update;
using KeyKeepers.BLL.Constants;
using KeyKeepers.BLL.DTOs.Communities;
using KeyKeepers.BLL.Validators.Communities;
using Xunit;

namespace KeyKeepers.UnitTests.ValidatorTests
{
    public class UpdateCommunityValidatorTests
    {
        private readonly UpdateCommunityValidator validator;

        public UpdateCommunityValidatorTests()
        {
            validator = new UpdateCommunityValidator();
        }

        [Fact]
        public void Should_Have_Error_When_Name_Is_Empty()
        {
            // Arrange
            var dto = new UpdateCommunityRequestDto
            {
                CommunityId = 1,
                Name = string.Empty,
            };
            var model = new UpdateCommunityCommand(dto);

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
            var dto = new UpdateCommunityRequestDto
            {
                CommunityId = 1,
                Name = new string('A', CommunityConstants.MinNameLenght - 1),
            };
            var model = new UpdateCommunityCommand(dto);

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
            var dto = new UpdateCommunityRequestDto
            {
                CommunityId = 1,
                Name = new string('X', CommunityConstants.MaxNameLenght + 1),
            };
            var model = new UpdateCommunityCommand(dto);

            // Act
            var result = validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.RequestDto.Name)
                .WithErrorMessage(CommunityConstants.MaxNameLenghtError);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Name_Is_Valid()
        {
            // Arrange
            var dto = new UpdateCommunityRequestDto
            {
                CommunityId = 1,
                Name = "Updated Community Name",
            };
            var model = new UpdateCommunityCommand(dto);

            // Act
            var result = validator.TestValidate(model);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.RequestDto.Name);
        }
    }
}
