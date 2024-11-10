using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NavistarPagos.Controllers
{
    public class HomeConfiguracion
    {
        /*
         * Opciones de Servidor
         * 1 = 0 = Servidor de Desarrollo
         * 2 = 1 = Servidor de Calidad BC
         * 3 = 2 = Servidor de Calidad Navistar
         * 4 = 3 = Servidor de Produccion Navistar
         */
        public int TipoServidor = int.Parse(System.Configuration.ConfigurationManager.AppSettings["TipoServidor"]);
    }
}