using FluentValidation.TestHelper;
using KeyKeepers.BLL.DTOs.PasswordCategories;
using KeyKeepers.BLL.Validators.PasswordCategories;
using KeyKeepers.BLL.Constants;
using Xunit;

namespace KeyKeepers.UnitTests.ValidatorTests
{
    public class PrivateCategoryValidatorTests
    {
        private readonly PrivateCategoryValidator validator;

        public PrivateCategoryValidatorTests()
        {
            validator = new PrivateCategoryValidator();
        }

        [Fact]
        public void Should_Have_Error_When_Name_Is_Empty()
        {
            // Arrange
            var model = new CreatePrivateCategoryDto { Name = string.Empty };

            // Act
            var result = validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name)
                .WithErrorMessage(PasswordCategoriesConstants.NameRequiredErrorMessage);
        }

        [Fact]
        public void Should_Have_Error_When_Name_Is_Too_Short()
        {
            // Arrange
            var model = new CreatePrivateCategoryDto { Name = "A" }; // коротше за мінімум

            // Act
            var result = validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name)
                .WithErrorMessage(PasswordCategoriesConstants.MinNameLengthErrorMessage);
        }

        [Fact]
        public void Should_Have_Error_When_Name_Is_Too_Long()
        {
            // Arrange
            var longName = new string('X', PasswordCategoriesConstants.MaxNameLength + 1);
            var model = new CreatePrivateCategoryDto { Name = longName };

            // Act
            var result = validator.TestValidate(model);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name)
                .WithErrorMessage(PasswordCategoriesConstants.MaxNameLengthErrorMessage);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Name_Is_Valid()
        {
            // Arrange
            var model = new CreatePrivateCategoryDto
            {
                Name = "Personal Accounts",
            };

            // Act
            var result = validator.TestValidate(model);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Name);
        }
    }
}
