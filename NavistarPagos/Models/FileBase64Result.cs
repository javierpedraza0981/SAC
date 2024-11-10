using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NavistarPagos.Models
{
    public class FileBase64Result
    {
        public string Nombre { get; set; }
        public string ContenidoBase64 { get; set; }
        public string RutaBase64 { get; set; }
        public string _Carpeta { get; set; }
        public string Contrato { get; set; }
    }
}