namespace Server.Groups.Police.MDT
{
    public class VehicleResult
    {
        public string Plate { get; set; }
        public string Owner { get; set; }
        public string Model { get; set; }

        public int Id { get; set; }

        public VehicleResult(string plate, string owner, string model, int id)
        {
            Plate = plate;
            Owner = owner;
            Model = model;
            Id = id;
        }
    }
}