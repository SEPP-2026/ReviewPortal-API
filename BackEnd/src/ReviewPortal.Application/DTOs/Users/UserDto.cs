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

public record AuthResponse(
    string Token,
    string Name,
    string Email,
    string Role,
    DateTime Expiry
);
