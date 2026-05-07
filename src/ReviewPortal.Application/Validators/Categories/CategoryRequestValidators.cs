using FluentValidation;
using ReviewPortal.Application.DTOs.Categories;

namespace ReviewPortal.Application.Validators.Categories;

public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequest>
{
    public CreateCategoryRequestValidator()
    {
        RuleFor(request => request.Name)
            .ApplyCategoryNameRules();

        RuleFor(request => request.Description)
            .Must(value => ValidationHelpers.OptionalHasTrimmedLengthAtMost(value, 500))
            .WithMessage("Category description must be 500 characters or fewer.");

        RuleFor(request => request.ImageUrl)
            .Must(value => ValidationHelpers.OptionalHasTrimmedLengthAtMost(value, 500))
            .WithMessage("Category image URL must be 500 characters or fewer.");
    }
}

public class UpdateCategoryRequestValidator : AbstractValidator<UpdateCategoryRequest>
{
    public UpdateCategoryRequestValidator()
    {
        RuleFor(request => request.Name)
            .ApplyCategoryNameRules();

        RuleFor(request => request.Description)
            .Must(value => ValidationHelpers.OptionalHasTrimmedLengthAtMost(value, 500))
            .WithMessage("Category description must be 500 characters or fewer.");

        RuleFor(request => request.ImageUrl)
            .Must(value => ValidationHelpers.OptionalHasTrimmedLengthAtMost(value, 500))
            .WithMessage("Category image URL must be 500 characters or fewer.");
    }
}

internal static class CategoryValidationRuleBuilderExtensions
{
    public static IRuleBuilderOptions<T, string> ApplyCategoryNameRules<T>(this IRuleBuilderInitial<T, string> ruleBuilder)
    {
        return ruleBuilder
            .Cascade(CascadeMode.Stop)
            .Must(ValidationHelpers.HasText)
            .WithMessage("Category name is required.")
            .Must(value => ValidationHelpers.HasTrimmedLengthAtMost(value, 100))
            .WithMessage("Category name must be 100 characters or fewer.");
    }
}
