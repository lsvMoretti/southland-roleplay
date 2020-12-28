using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Server.Models
{
    public class Dealership
    {
        /// <summary>
        /// Unique Dealership Id
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Dealership Name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Interaction Point X
        /// </summary>
        public float PosX { get; set; }

        /// <summary>
        /// Interaction Point Y
        /// </summary>
        public float PosY { get; set; }

        /// <summary>
        /// Interaction Point Z
        /// </summary>
        public float PosZ { get; set; }

        /// <summary>
        /// Vehicle spawn position X
        /// </summary>
        public float VehPosX { get; set; }

        /// <summary>
        /// Vehicle spawn position Y
        /// </summary>
        public float VehPosY { get; set; }

        /// <summary>
        /// Vehicle spawn position Z
        /// </summary>
        public float VehPosZ { get; set; }

        /// <summary>
        /// Vehicle spawn rotation Z
        /// </summary>
        public float VehRotZ { get; set; }

        /// <summary>
        /// JSON of DealershipVehicle
        /// </summary>
        public string? VehicleList { get; set; }

        public float CamPosX { get; set; }
        public float CamPosY { get; set; }
        public float CamPosZ { get; set; }

        public float CamRotZ { get; set; }

        /// <summary>
        /// Fetches Dealership by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Dealership</returns>
        public static Dealership FetchDealership(int id)
        {
            using Context context = new Context();

            return context.Dealership.Find(id);
        }

        /// <summary>
        /// Adds a Dealership to the Database
        /// </summary>
        /// <param name="dealership"></param>
        /// <returns>Dealership Id</returns>
        public static int AddDealership(Dealership dealership)
        {
            using Context context = new Context();

            context.Dealership.Add(dealership);
            context.SaveChanges();

            return dealership.Id;
        }

        /// <summary>
        /// Fetches Dealership list
        /// </summary>
        /// <returns>Dealership List</returns>
        public static List<Dealership> FetchDealerships()
        {
            Context context = new Context();

            List<Dealership> dealershipList = context.Dealership.ToList();

            return dealershipList;
        }
    }

    public class DealershipVehicle
    {
        /// <summary>
        /// Vehicle Name
        /// </summary>
        public string VehName { get; set; }

        /// <summary>
        /// Vehicle Model
        /// </summary>
        public int VehModel { get; set; }

        /// <summary>
        /// Vehicle Price
        /// </summary>
        public int VehPrice { get; set; }

        public string NewVehModel { get; set; }
    }
}