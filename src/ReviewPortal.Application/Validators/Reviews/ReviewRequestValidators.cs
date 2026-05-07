using FluentValidation;
using ReviewPortal.Application.DTOs.Reviews;

namespace ReviewPortal.Application.Validators.Reviews;

public class CreateReviewRequestValidator : AbstractValidator<CreateReviewRequest>
{
    public CreateReviewRequestValidator()
    {
        RuleFor(request => request.ReviewerName)
            .Must(value => ValidationHelpers.OptionalHasTrimmedLengthAtMost(value, 100))
            .WithMessage("Reviewer name must be 100 characters or fewer.");

        RuleFor(request => request.ReviewerEmail)
            .Cascade(CascadeMode.Stop)
            .Must(value => ValidationHelpers.OptionalHasTrimmedLengthAtMost(value, 256))
            .WithMessage("Reviewer email must be 256 characters or fewer.")
            .Must(ValidationHelpers.IsOptionalValidEmail)
            .WithMessage("Reviewer email must be a valid email address.");

        RuleFor(request => request.ReviewText)
            .Cascade(CascadeMode.Stop)
            .Must(ValidationHelpers.HasText)
            .WithMessage("Review text is required.")
            .Must(value => value is not null && value.Trim().Length >= 20)
            .WithMessage("Review text must be at least 20 characters.")
            .Must(value => ValidationHelpers.HasTrimmedLengthAtMost(value, 2000))
            .WithMessage("Review text must be 2000 characters or fewer.");

        RuleFor(request => request.EquipmentRating)
            .InclusiveBetween(1, 5)
            .WithMessage("Equipment rating must be between 1 and 5.");

        RuleFor(request => request.CustomerServiceRating)
            .InclusiveBetween(1, 5)
            .WithMessage("Customer service rating must be between 1 and 5.");

        RuleFor(request => request.TechnicalSupportRating)
            .InclusiveBetween(1, 5)
            .WithMessage("Technical support rating must be between 1 and 5.");

        RuleFor(request => request.AfterSalesRating)
            .InclusiveBetween(1, 5)
            .WithMessage("After-sales rating must be between 1 and 5.");

        RuleFor(request => request.ValueForMoneyRating)
            .InclusiveBetween(1, 5)
            .WithMessage("Value for money rating must be between 1 and 5.");
    }
}

public class CreateCommentRequestValidator : AbstractValidator<CreateCommentRequest>
{
    public CreateCommentRequestValidator()
    {
        RuleFor(request => request.CommenterName)
            .Must(value => ValidationHelpers.OptionalHasTrimmedLengthAtMost(value, 100))
            .WithMessage("Commenter name must be 100 characters or fewer.");

        RuleFor(request => request.CommentText)
            .Cascade(CascadeMode.Stop)
            .Must(ValidationHelpers.HasText)
            .WithMessage("Comment text is required.")
            .Must(value => value is not null && value.Trim().Length >= 10)
            .WithMessage("Comment text must be at least 10 characters.")
            .Must(value => ValidationHelpers.HasTrimmedLengthAtMost(value, 1000))
            .WithMessage("Comment text must be 1000 characters or fewer.");
    }
}

public class CreateCompanyResponseRequestValidator : AbstractValidator<CreateCompanyResponseRequest>
{
    public CreateCompanyResponseRequestValidator()
    {
        RuleFor(request => request.ResponseText)
            .Cascade(CascadeMode.Stop)
            .Must(ValidationHelpers.HasText)
            .WithMessage("Response text is required.")
            .Must(value => ValidationHelpers.HasTrimmedLengthAtMost(value, 2000))
            .WithMessage("Response text must be 2000 characters or fewer.");
    }
}

public class ModerateReviewRequestValidator : AbstractValidator<ModerateReviewRequest>
{
    public ModerateReviewRequestValidator()
    {
        When(request => !request.Approved, () =>
        {
            RuleFor(request => request.RejectionReason)
                .Cascade(CascadeMode.Stop)
                .Must(ValidationHelpers.HasText)
                .WithMessage("Rejection reason is required when rejecting.")
                .Must(value => ValidationHelpers.HasTrimmedLengthAtMost(value, 500))
                .WithMessage("Rejection reason must be 500 characters or fewer.");
        });
    }
}
