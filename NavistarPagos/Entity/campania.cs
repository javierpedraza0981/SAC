//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NavistarPagos.Entity
{
    using System;
    using System.Collections.Generic;
    
    public partial class campania
    {
        public int idcampania { get; set; }
        public string titulo { get; set; }
        public string objetivo { get; set; }
        public string link { get; set; }
        public string archivo { get; set; }
        public System.DateTime fInicio { get; set; }
        public System.DateTime fFinal { get; set; }
        public int id_status { get; set; }
        public string usuarioAlta { get; set; }
        public string usuarioModifica { get; set; }
        public System.DateTime fAlta { get; set; }
        public System.DateTime fModifica { get; set; }
    }
}