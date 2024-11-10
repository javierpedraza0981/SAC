using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace AuthenticationService.Models
{
    public class JWTContainerModel : IAuthContainerModel
    {
        #region Public Methods

        public int ExpireMinutes { get; set; } = 0; // 10080 = 7 days.
        public string SecretKeyClear { get; set; }
        public string SecretKey { get { return System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(SecretKeyClear)); } }
        public string SecurityAlgorithm { get; set; } = SecurityAlgorithms.HmacSha256Signature;
        public Claim[] Claims { get; set; }

        #endregion
    }
}