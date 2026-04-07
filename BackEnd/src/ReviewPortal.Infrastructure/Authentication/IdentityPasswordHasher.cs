using Microsoft.AspNetCore.Identity;
using ReviewPortal.Application.Interfaces;
using ReviewPortal.Domain.Entities;

namespace ReviewPortal.Infrastructure.Authentication;

public sealed class IdentityPasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    public string Hash(string password)
    {
        var user = CreateHashingUser();
        return _passwordHasher.HashPassword(user, password);
    }

    public bool Verify(string password, string passwordHash)
    {
        var user = CreateHashingUser();

        var result = _passwordHasher.VerifyHashedPassword(user, passwordHash, password);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }

    private static User CreateHashingUser()
    {
        return new User
        {
            Name = "PasswordHasher",
            Email = "passwordhasher@local",
            PasswordHash = string.Empty
        };
    }
}
