using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Claims;
using AuthenticationService.Models;
using AuthenticationService.Managers;

namespace NavistarPagos.AccountCore
{
    public class AppEncriptaJWT
    {
        private IAuthService authService = null;
        private List<Claim> claims = null;

        public AppEncriptaJWT() { }

        public string EncriptaJWT(int idCliente, string nombre, string correo, string key)
        {
            return EncriptaJWT(idCliente, nombre, correo, key, "", "", "");
        }

        public string EncriptaJWT(int idCliente, string nombre, string correo, string key, string urlOk, string urlError, string urlCancela)
        {
            string[] elementos = { "idCliente|" + idCliente, "given_name|" + nombre, "email|" + correo, "muestraCliente|N", "urlOk|" + urlOk, "urlError|" + urlError, "urlCancela|" + urlCancela, "ver|1.0" };
            IAuthContainerModel model = GetJWTContainerModel(elementos);
            authService = new JWTService(key);

            return authService.GenerateToken(model);
        }

        public void DesencriptaJWT(string cadenaToken, string key)
        {
            //string[] elementos = {"name|Juan Martinez","email|jcmartinez"};
            //IAuthContainerModel model = GetJWTContainerModel(elementos);
            authService = new JWTService(key);
            //string token = authService.GenerateToken(model);
            string mensaje = "";
            if (!authService.IsTokenValid(cadenaToken, ref mensaje))
            {
                throw new UnauthorizedAccessException();
            }
            else
            {
                claims = authService.GetTokenClaims(cadenaToken).ToList();
            }
        }

        public string GetClaim(string name)
        {
            IAuthService authService = new JWTService("");
            return authService.GetClaim(claims, name);
        }

        #region Private Methods

        private JWTContainerModel GetJWTContainerModel(string[] elementos)
        {
            Claim[] claims = new Claim[elementos.Length];
            for (int i = 0; i < elementos.Length; i++)
            {
                string[] elmt = elementos[i].Split('|');
                if (elmt.Length == 2)
                {
                    Claim claim = new Claim(elmt[0], elmt[1]);
                    claims[i] = claim;
                }
            }
            return new JWTContainerModel()
            {
                Claims = claims
            };
        }

        #endregion

    }
}