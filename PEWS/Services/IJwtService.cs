using PEWS.Models;

namespace PEWS.Services;

public interface IJwtService
{
    string GenerateToken(ApplicationUser user, IList<string> roles);
}
