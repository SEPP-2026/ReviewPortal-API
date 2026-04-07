using ReviewPortal.Infrastructure.Authentication;

namespace ReviewPortal.IntegrationTests.Authentication;

public class IdentityPasswordHasherTests
{
    [Fact]
    public void HashAndVerify_WithMatchingPassword_ReturnsTrue()
    {
        var hasher = new IdentityPasswordHasher();
        var hash = hasher.Hash("Customer123!");

        var result = hasher.Verify("Customer123!", hash);

        Assert.True(result);
    }

    [Fact]
    public void Verify_WithWrongPassword_ReturnsFalse()
    {
        var hasher = new IdentityPasswordHasher();
        var hash = hasher.Hash("Customer123!");

        var result = hasher.Verify("WrongPassword123!", hash);

        Assert.False(result);
    }
}
