using ReviewPortal.Domain.Entities;
using ReviewPortal.Domain.Enums;

namespace ReviewPortal.Infrastructure.Data;

internal static class UserSeedData
{
    private static readonly DateTime SeededAtUtc = new(2026, 4, 6, 0, 0, 0, DateTimeKind.Utc);

    public const int CustomerUserId = 1;
    public const int AdminUserId = 2;
    public const int ModeratorUserId = 3;

    public const string CustomerEmail = "customer.test@reviewportal.local";
    public const string AdminEmail = "admin.test@reviewportal.local";
    public const string ModeratorEmail = "moderator.test@reviewportal.local";

    public static IReadOnlyList<User> Users =>
    [
        new User
        {
            Id = CustomerUserId,
            Name = "Test Customer",
            Email = CustomerEmail,
            PasswordHash = "AQAAAAIAAYagAAAAEA0G/BQ4WPdNpKqAnK0Mam0BNoG560l4GFfcdOa4Xps8ZYyGiiH6yxH5EgC1llfMqQ==",
            Role = UserRole.Customer,
            CreatedDate = SeededAtUtc
        },
        new User
        {
            Id = AdminUserId,
            Name = "Test Admin",
            Email = AdminEmail,
            PasswordHash = "AQAAAAIAAYagAAAAEOqaZaaWb29MnbnePdQNWq+wQO66MHMXfZh0ouwcBZjORmC3mllgI0zebkL9iOSFqQ==",
            Role = UserRole.Admin,
            CreatedDate = SeededAtUtc
        },
        new User
        {
            Id = ModeratorUserId,
            Name = "Test Moderator",
            Email = ModeratorEmail,
            PasswordHash = "AQAAAAIAAYagAAAAEMnrqmN8EAAHiVorrGMGoFopVvlvMBVCFw0pvQz9mgWzxZrB1RGNwUsqv+n41Wss/g==",
            Role = UserRole.Moderator,
            CreatedDate = SeededAtUtc
        }
    ];
}
