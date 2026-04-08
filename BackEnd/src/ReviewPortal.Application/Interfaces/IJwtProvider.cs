using ReviewPortal.Application.Common;
using ReviewPortal.Domain.Entities;

namespace ReviewPortal.Application.Interfaces;

public interface IJwtProvider
{
    JwtToken Generate(User user);
}
