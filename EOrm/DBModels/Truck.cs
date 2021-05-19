//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EOrm.DBModels
{
    using System;
    using System.Collections.Generic;
    
    public partial class Truck
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Truck()
        {
            this.Shipments = new HashSet<Shipment>();
            this.Drivers = new HashSet<Driver>();
        }
    
        public int TruckId { get; set; }
        public string BrandName { get; set; }
        public string RegistrationNumber { get; set; }
        public int Year { get; set; }
        public decimal Payload { get; set; }
        public decimal FuelConsumption { get; set; }
        public decimal Volume { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Shipment> Shipments { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Driver> Drivers { get; set; }
    }
}
