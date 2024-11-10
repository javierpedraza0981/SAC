using System.Security.Claims;
using System.Collections.Generic;
using AuthenticationService.Models;

namespace AuthenticationService.Managers
{
    public interface IAuthService
    {
        string SecretKeyClear { get; set; }
        string SecretKey { get; }

        bool IsTokenValid(string token, ref string mensaje);
        string GenerateToken(IAuthContainerModel model);
        IEnumerable<Claim> GetTokenClaims(string token);
        string GetClaim(List<Claim> claims, string name);
    }
}
