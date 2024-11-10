using System.Security.Claims;

namespace AuthenticationService.Models
{
    public interface IAuthContainerModel
    {
        #region Members
        string SecretKeyClear { get; set; }
        string SecretKey { get; }
        string SecurityAlgorithm { get; set; }
        int ExpireMinutes { get; set; }

        Claim[] Claims { get; set; }
        #endregion
    }
}
