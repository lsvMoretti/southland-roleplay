namespace Server.Vehicle
{
    public class StolenPlate
    {
        public int VehicleId { get; set; }
        public string Plate { get; set; }
        public string Model { get; set; }

        public StolenPlate(Models.Vehicle vehicleData)
        {
            VehicleId = vehicleData.Id;
            Plate = vehicleData.Plate;
            Model = vehicleData.Model;
        }
    }
}