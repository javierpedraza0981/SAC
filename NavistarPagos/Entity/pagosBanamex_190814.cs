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
    
    public partial class pagosBanamex_190814
    {
        public long id_pago { get; set; }
        public Nullable<long> id_movimiento { get; set; }
        public Nullable<System.DateTime> fecha { get; set; }
        public Nullable<int> cveCliente { get; set; }
        public string Operacion { get; set; }
        public string CTE { get; set; }
        public string REFER_PGO { get; set; }
        public string Importe { get; set; }
        public string Concepto { get; set; }
        public string TPO_ABO { get; set; }
        public string AUTORIZA { get; set; }
        public string AUTORIZA2 { get; set; }
    }
}
