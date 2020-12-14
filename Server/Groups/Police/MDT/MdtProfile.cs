using System.Collections.Generic;

namespace Server.Groups.Police.MDT
{
    public class MdtProfile
    {
        public string Name { get; set; }
        public string Age { get; set; }
        public string Gender { get; set; }
        public bool DriversLicense { get; set; }
        public List<string> OwnedProperties { get; set; }
        public List<Models.Vehicle> OwnedVehicles { get; set; }

        public bool PistolLicense { get; set; }

        /// <summary>
        /// MDT Profile Page for Person
        /// </summary>
        /// <param name="name"></param>
        /// <param name="age"></param>
        /// <param name="gender"></param>
        /// <param name="driversLicense"></param>
        /// <param name="ownedProperties"></param>
        /// <param name="ownedVehicles"></param>
        /// <param name="pistolLicense"></param>
        public MdtProfile(string name, string age, string gender, bool driversLicense,
            List<string> ownedProperties = null, List<Models.Vehicle> ownedVehicles = null, bool pistolLicense = false)
        {
            Name = name;
            Age = age;
            Gender = gender;
            DriversLicense = driversLicense;
            OwnedProperties = ownedProperties ?? new List<string>();
            OwnedVehicles = ownedVehicles ?? new List<Models.Vehicle>();
            PistolLicense = pistolLicense;
        }
    }
}