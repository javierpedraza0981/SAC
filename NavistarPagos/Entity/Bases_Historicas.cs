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
    
    public partial class Bases_Historicas
    {
        public int id_base { get; set; }
        public string descripcion { get; set; }
        public int mes { get; set; }
        public int anio { get; set; }
        public string nombrebd { get; set; }
        public int activo { get; set; }
        public Nullable<System.DateTime> fInicioTarjeta { get; set; }
        public Nullable<System.DateTime> fFinalTarjeta { get; set; }
    }
}