using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ReviewPortal.Application.Common;
using ReviewPortal.Domain.Entities;
using ReviewPortal.Domain.Enums;
using ReviewPortal.Infrastructure.Authentication;

namespace ReviewPortal.IntegrationTests.Authentication;

public class JwtProviderTests
{
    [Fact]
    public void Generate_ShouldEmbedUserIdentifierAndRoleClaims()
    {
        var provider = new JwtProvider(new JwtSettings
        {
            Secret = "super-secure-test-secret-with-32-plus-chars",
            Issuer = "ReviewPortalTests",
            Audience = "ReviewPortalClientTests",
            ExpiryMinutes = 60
        });

        var token = provider.Generate(new User
        {
            Id = 42,
            Name = "Morgan",
            Email = "morgan@example.com",
            PasswordHash = "hashed-password",
            Role = UserRole.Admin
        });

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token.Value);

        Assert.Contains(jwt.Claims, claim =>
            claim.Value == "42" &&
            (claim.Type == ClaimTypes.NameIdentifier || claim.Type == JwtRegisteredClaimNames.NameId || claim.Type == "nameid"));

        Assert.Contains(jwt.Claims, claim =>
            claim.Value == "Admin" &&
            (claim.Type == ClaimTypes.Role || claim.Type == "role"));
        Assert.Equal("ReviewPortalTests", jwt.Issuer);
        Assert.Equal("ReviewPortalClientTests", jwt.Audiences.Single());
    }
}
