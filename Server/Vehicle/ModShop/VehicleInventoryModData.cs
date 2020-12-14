using System.Collections.Generic;

namespace Server.Vehicle.ModShop
{
    public class VehicleInventoryModData
    {
        public string ModClassName { get; set; }
        public string ModName { get; set; }
        public string VehicleName { get; set; }
        public KeyValuePair<int, int> ModData { get; set; }
        public int WheelType { get; set; }
    }
}