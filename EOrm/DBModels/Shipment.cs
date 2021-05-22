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
    using EOrm.Attributes;
    using EOrm.Interfaces;
    using System;
    using System.Collections.Generic;
    
    public partial class Shipment : IEntity
    {
        [PrimaryKey]
        [ColumnProperty]
        public int ShipmentId { get; set; }
        [ColumnProperty]
        public int TruckId { get; set; }
        [ColumnProperty]
        public int DriverId { get; set; }
        [ColumnProperty]
        public int CargoId { get; set; }
        [ColumnProperty]
        public int RouteId { get; set; }
        [ColumnProperty]
        public System.DateTime StartDate { get; set; }
        [ColumnProperty]
        public System.DateTime EndDate { get; set; }
    
        public virtual Cargo Cargo { get; set; }
        public virtual Driver Driver { get; set; }
        public virtual Route Route { get; set; }
        public virtual Truck Truck { get; set; }

        public int Id => ShipmentId;

        public string GetCreateCommand()
        {
            return $"INSERT INTO Shipment(TruckId,DriverId,CargoId,RouteId,StartDate,EndDate) VALUES ({TruckId}, {DriverId}, {CargoId}, {RouteId}, '{StartDate}', '{EndDate}');";
        }

        public string GetDeleteCommand()
        {
            return $"DELETE [TruckDriver] WHERE ShipmentId = {ShipmentId}; " +
                $"DELETE FROM ShipmentId WHERE ShipmentId = {ShipmentId};";
        }

        public string GetUpdateCommand()
        {
            return $"UPDATE Shipment SET TruckId={TruckId},DriverId={DriverId},CargoId={CargoId},RouteId={RouteId},StartDate='{StartDate}',EndDate='{EndDate}' WHERE ShipmentId={ShipmentId};";
        }
    }
}
