namespace ReviewPortal.Application.DTOs.Users;

public record UserDto(
    int Id,
    string Name,
    string Email,
    string Role,
    DateTime CreatedDate
);

public record RegisterRequest(
    string Name,
    string Email,
    string Password
);

public record LoginRequest(
    string Email,
    string Password
);

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword
);

public record ForgotPasswordRequest(
    string Email
);

public record ForgotPasswordResponse(
    string Message,
    string? ResetToken,
    DateTime? ResetTokenExpiresAtUtc
);

public record ResetPasswordRequest(
    string Email,
    string ResetToken,
    string NewPassword
);

public record PasswordActionResponse(
    string Message
);

public record AuthResponse(
    string Token,
    string Name,
    string Email,
    string Role,
    DateTime Expiry
);
