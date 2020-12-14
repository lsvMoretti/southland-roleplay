using System.Collections.Generic;
using Newtonsoft.Json;
using Server.Extensions;
using Server.Groups;

namespace Server.Inventory
{
    public class WeaponInfo
    {
        public int AmmoCount { get; set; }
        public bool Legal { get; set; }
        public string SerialNumber { get; set; }
        public string Purchaser { get; set; }

        public List<string> LastPerson { get; set; }

        public WeaponInfo(int ammoCount, bool legal, string purchaserName = null)
        {
            AmmoCount = ammoCount;
            Legal = legal;
            SerialNumber = legal ? Utility.GenerateRandomNumber(6) : null;

            Purchaser = purchaserName;

            LastPerson = new List<string>(5);
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}