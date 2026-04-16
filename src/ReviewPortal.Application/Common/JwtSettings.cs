namespace ReviewPortal.Application.Common;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Secret { get; init; } = string.Empty;

    public string Issuer { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;

    public int ExpiryMinutes { get; init; } = 60;
}

public sealed record JwtToken(
    string Value,
    DateTime ExpiresAtUtc
);
