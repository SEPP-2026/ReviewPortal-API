using FluentValidation;

namespace ReviewPortal.Application.Validators;

internal static class RequestValidatorRunner
{
    public static string? Validate<T>(IValidator<T> validator, T? request, string requiredMessage)
        where T : class
    {
        if (request is null)
        {
            return requiredMessage;
        }

        var validationResult = validator.Validate(request);
        return validationResult.IsValid
            ? null
            : validationResult.Errors[0].ErrorMessage;
    }
}
