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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class ShipmentEntities : DbContext
    {
        public ShipmentEntities()
            : base("name=ShipmentEntities")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }

        public virtual DbSet<Cargo> Cargoes { get; set; }
        public virtual DbSet<Contact> Contacts { get; set; }
        public virtual DbSet<Driver> Drivers { get; set; }
        public virtual DbSet<Route> Routes { get; set; }
        public virtual DbSet<Shipment> Shipments { get; set; }
        public virtual DbSet<Truck> Trucks { get; set; }
        public virtual DbSet<Wharehouse> Wharehouses { get; set; }
    }
}
