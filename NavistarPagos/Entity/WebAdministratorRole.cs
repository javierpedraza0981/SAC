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
    
    public partial class WebAdministratorRole
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public WebAdministratorRole()
        {
            this.cat_WebAdministrators = new HashSet<cat_WebAdministrators>();
        }
    
        public int WebAdministratorRoleId { get; set; }
        public string WebAdministratorRoleName { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<cat_WebAdministrators> cat_WebAdministrators { get; set; }
    }
}
