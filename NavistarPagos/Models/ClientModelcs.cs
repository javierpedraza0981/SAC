using System;
using System.Web;

namespace NavistarPagos.Models
{

    public class ClientModel
    {
        public int Cve { get; set; }
        public string Password { get; set; }
        public string email { get; set; }
    }

    public class DireccionClientModel
    {
        public string Calle { get; set; }
        public string NoInt { get; set; }
        public string NoExt { get; set; }
        public string Colonia { get; set; }
        public string DelMun { get; set; }
        public string Ciudad { get; set; }
        public string Entidad { get; set; }
        public string CP { get; set; }
        public string TipoDomicilio { get; set; }
    }

    public class TelefonoClientModel
    {
        public string TipoTelefono { get; set; }
        public string Telefono { get; set; }
    }

}
