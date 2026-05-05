using FluentValidation;
using ReviewPortal.Application.DTOs.Users;

namespace ReviewPortal.Application.Validators.Users;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(request => request.Name)
            .Cascade(CascadeMode.Stop)
            .Must(ValidationHelpers.HasText)
            .WithMessage("Name is required.")
            .Must(value => ValidationHelpers.HasTrimmedLengthAtMost(value, 100))
            .WithMessage("Name must be 100 characters or fewer.");

        RuleFor(request => request.Email)
            .ApplyEmailRules();

        RuleFor(request => request.Password)
            .ApplyPasswordRules("Password");
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(request => request.Email)
            .Must(ValidationHelpers.HasText)
            .WithMessage("Email is required.");

        RuleFor(request => request.Password)
            .Must(ValidationHelpers.HasText)
            .WithMessage("Password is required.");
    }
}

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(request => request.CurrentPassword)
            .Must(ValidationHelpers.HasText)
            .WithMessage("Current password is required.");

        RuleFor(request => request.NewPassword)
            .ApplyPasswordRules("New password")
            .NotEqual(request => request.CurrentPassword)
            .WithMessage("New password must be different from the current password.");
    }
}

public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(request => request.Email)
            .ApplyEmailRules();
    }
}

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(request => request.ResetToken)
            .Must(ValidationHelpers.HasText)
            .WithMessage("Reset token is required.");

        RuleFor(request => request.Email)
            .ApplyEmailRules();

        RuleFor(request => request.NewPassword)
            .ApplyPasswordRules("New password");
    }
}

internal static class AuthValidationRuleBuilderExtensions
{
    public static IRuleBuilderOptions<T, string> ApplyEmailRules<T>(this IRuleBuilderInitial<T, string> ruleBuilder)
    {
        return ruleBuilder
            .Cascade(CascadeMode.Stop)
            .Must(ValidationHelpers.HasText)
            .WithMessage("Email is required.")
            .Must(value => ValidationHelpers.HasTrimmedLengthAtMost(value, 256))
            .WithMessage("Email must be 256 characters or fewer.")
            .Must(ValidationHelpers.IsValidEmail)
            .WithMessage("A valid email address is required.");
    }

    public static IRuleBuilderOptions<T, string> ApplyPasswordRules<T>(
        this IRuleBuilderInitial<T, string> ruleBuilder,
        string fieldLabel)
    {
        return ruleBuilder
            .Cascade(CascadeMode.Stop)
            .Must(ValidationHelpers.HasText)
            .WithMessage($"{fieldLabel} is required.")
            .MinimumLength(8)
            .WithMessage($"{fieldLabel} must be at least 8 characters long.")
            .Must(ValidationHelpers.ContainsUppercase)
            .WithMessage($"{fieldLabel} must contain at least one uppercase letter.")
            .Must(ValidationHelpers.ContainsNumber)
            .WithMessage($"{fieldLabel} must contain at least one number.");
    }
}
