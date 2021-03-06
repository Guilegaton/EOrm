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

    public partial class Cargo : IEntity
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Cargo()
        {
        }
    
        [PrimaryKey]
        [ColumnProperty]
        public int CargoId { get; set; }
        [ColumnProperty]
        public decimal Weight { get; set; }
        [ColumnProperty]
        public decimal Volume { get; set; }
        [ColumnProperty]
        public int CustomerId { get; set; }
        [ColumnProperty]
        public int RecipientId { get; set; }
        [ColumnProperty]
        public int Destination { get; set; }

        public virtual Contact Customer { get; set; }
        public virtual Contact Recipient { get; set; }
        public virtual Route Route { get; set; }

        public int Id => CargoId;

        public string GetCreateCommand()
        {
            return $"INSERT INTO Cargo(Weight,Volume,CustomerId,RecipientId,Destination) VALUES ({Weight}, {Volume}, {CustomerId}, {RecipientId}, {Destination});";
        }

        public string GetDeleteCommand()
        {
            return $"DELETE FROM Cargo WHERE CargoId = {CargoId}";
        }

        public string GetUpdateCommand()
        {
            return $"UPDATE Cargo SET Weight={Weight},Volume={Volume},CustomerId={CustomerId},RecipientId={RecipientId},Destination={Destination} WHERE CargoId={CargoId};";
        }
    }
}
