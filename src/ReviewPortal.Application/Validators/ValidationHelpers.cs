using System.Net.Mail;

namespace ReviewPortal.Application.Validators;

internal static class ValidationHelpers
{
    public static bool HasText(string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    public static bool HasTrimmedLengthAtMost(string? value, int maxLength)
    {
        return value is not null && value.Trim().Length <= maxLength;
    }

    public static bool OptionalHasTrimmedLengthAtMost(string? value, int maxLength)
    {
        return string.IsNullOrWhiteSpace(value) || value.Trim().Length <= maxLength;
    }

    public static bool IsValidEmail(string? email)
    {
        return !string.IsNullOrWhiteSpace(email) && MailAddress.TryCreate(email.Trim(), out _);
    }

    public static bool IsOptionalValidEmail(string? email)
    {
        return string.IsNullOrWhiteSpace(email) || MailAddress.TryCreate(email.Trim(), out _);
    }

    public static bool ContainsUppercase(string? value)
    {
        return value?.Any(char.IsUpper) == true;
    }

    public static bool ContainsNumber(string? value)
    {
        return value?.Any(char.IsDigit) == true;
    }
}
