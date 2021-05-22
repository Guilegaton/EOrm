using EOrm.Attributes;
using EOrm.Interfaces;

namespace EOrm.DBModels
{
    public class TruckDriver : IEntity
    {
        [ColumnProperty]
        public int DriverId { get; set; }
        [ColumnProperty]
        public int TruckId { get; set; }

        public Truck Truck { get; set; }
        public Driver Driver { get; set; }

        public int Id => -1;

        public string GetCreateCommand()
        {
            return $"INSERT INTO TruckDriver(DriverId,State) VALUES ({DriverId}, {TruckId});";
        }

        public string GetDeleteCommand()
        {
            return $"DELETE FROM TruckDriver WHERE DriverId = {DriverId} AND TruckId={TruckId};";
        }

        public string GetUpdateCommand()
        {
            throw new System.Exception("Not supported action");
        }
    }
}
