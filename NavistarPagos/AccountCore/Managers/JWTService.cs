using System;
using System.Linq;
using System.Security.Claims;
using System.Collections.Generic;
using AuthenticationService.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace AuthenticationService.Managers
{
    public class JWTService : IAuthService
    {

        #region Members

        /// <summary>
        /// The secret key we use to encrypt out token with.
        /// </summary>
        public string SecretKeyClear { get; set; }

        public string SecretKey { get { return System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(SecretKeyClear)); } }

        #endregion

        #region Constructor

        public JWTService(string secretKeyClear)
        {
            SecretKeyClear = secretKeyClear;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Validates whether a given token is valid or not, and returns true in case the token is valid otherwise it will return false;
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool IsTokenValid(string token, ref string mensaje)
        {
            if (string.IsNullOrEmpty(token)) throw new ArgumentException("El token está nulo o vacio.");
            TokenValidationParameters tokenValidationParameters = GetTokenValidationParameters();
            JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            SecurityToken validatedToken = null;
            try
            {
                ClaimsPrincipal tokenValid = jwtSecurityTokenHandler.ValidateToken(token, tokenValidationParameters, out validatedToken);
                return true;
            }
            catch (Exception ex)
            {
                mensaje = "Error: " + ex.Message + ((ex.InnerException != null) ? " - " + ex.InnerException.Message : "");
                return false;
            }
        }

        /// <summary>
        /// Generates token by given model.
        /// Validates whether the given model is valid, then gets the symmetric key.
        /// Encrypt the token and returns it.
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Generated token.</returns>
        public string GenerateToken(IAuthContainerModel model)
        {
            if (model == null || model.Claims == null || model.Claims.Length == 0)
                throw new ArgumentException("Los argumentos para crear el token no son válidos.");

            SecurityTokenDescriptor securityTokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(model.Claims),
                //Expires = DateTime.UtcNow.AddMinutes(Convert.ToInt32(model.ExpireMinutes)),
                SigningCredentials = new SigningCredentials(GetSymmetricSecurityKey(), model.SecurityAlgorithm)
            };
            JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken = jwtSecurityTokenHandler.CreateToken(securityTokenDescriptor);
            return jwtSecurityTokenHandler.WriteToken(securityToken);
        }

        /// <summary>
        /// Receives the claims of token by given token as string.
        /// </summary>
        /// <remarks>
        /// Pay attention, one the token is FAKE the method will throw an exception.
        /// </remarks>
        /// <param name="token"></param>
        /// <returns>IEnumerable of claims for the given token.</returns>
        public IEnumerable<Claim> GetTokenClaims(string token)
        {
            if (string.IsNullOrEmpty(token)) throw new ArgumentException("El token está nulo o vacio.");
            TokenValidationParameters tokenValidationParameters = GetTokenValidationParameters();
            JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            try
            {
                //ClaimsPrincipal tokenValid = jwtSecurityTokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);
                var tokenSecurity = jwtSecurityTokenHandler.ReadToken(token) as JwtSecurityToken;
                return tokenSecurity.Claims;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public string GetClaim(List<Claim> claims, string name)
        {
            Claim claim = claims.FirstOrDefault(e => e.Type.Equals(name));
            if (claim != null) return claim.Value;
            return "";
        }

        #endregion

        #region Private Methods

        private SecurityKey GetSymmetricSecurityKey()
        {
            byte[] symmetricKey = Convert.FromBase64String(SecretKey);
            return new SymmetricSecurityKey(symmetricKey);
        }

        private TokenValidationParameters GetTokenValidationParameters()
        {
            return new TokenValidationParameters()
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                IssuerSigningKey = GetSymmetricSecurityKey()
            };
        }

        #endregion

    }
}