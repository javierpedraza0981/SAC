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
    
    public partial class faq
    {
        public long id_faq { get; set; }
        public string pregunta { get; set; }
        public string respuesta { get; set; }
        public string categoria { get; set; }
        public Nullable<int> activo { get; set; }
        public Nullable<int> orden { get; set; }
    }
}