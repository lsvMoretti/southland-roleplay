using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AltV.Net.Data;

namespace Server.Models
{
    public class Garage
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Exterior Position X
        /// </summary>
        public float ExtPosX { get; set; }

        /// <summary>
        /// Exterior Position Y
        /// </summary>
        public float ExtPosY { get; set; }

        /// <summary>
        /// Exterior Position Z
        /// </summary>
        public float ExtPosZ { get; set; }

        /// <summary>
        /// Exterior Rotation Z
        /// </summary>
        public float ExtRotZ { get; set; }

        /// <summary>
        /// The Exterior Dimension
        /// </summary>
        public uint ExtDimension { get; set; }

        /// <summary>
        /// Internal Position X
        /// </summary>
        public float IntPosX { get; set; }

        /// <summary>
        /// Internal Position Y
        /// </summary>
        public float IntPosY { get; set; }

        /// <summary>
        /// Internal Position Z
        /// </summary>
        public float IntPosZ { get; set; }

        /// <summary>
        /// Internal Position Z
        /// </summary>
        public float IntRotZ { get; set; }

        /// <summary>
        /// IPL to be loaded
        /// </summary>
        public string Ipl { get; set; }

        /// <summary>
        /// Json of Prop List
        /// </summary>
        public string PropJson { get; set; }

        /// <summary>
        /// Sets the color
        /// </summary>
        public int ColorId { get; set; }

        /// <summary>
        /// Which garage it's linked to
        /// </summary>
        public int LinkedGarage { get; set; }

        /// <summary>
        /// Fetches Garage by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Garage FetchGarage(int id)
        {
            using (Context context = new Context())
            {
                return context.Garages.Find(id);
            }
        }

        public static List<Garage> FetchGarages()
        {
            using (Context context = new Context())
            {
                return context.Garages.ToList();
            }
        }

        /// <summary>
        /// Fetches Garage's External Position by ID
        /// </summary>
        /// <param name="garage"></param>
        /// <returns></returns>
        public static Position FetchGarageExteriorPosition(Garage garage)
        {
            return new Position(garage.ExtPosX, garage.ExtPosY, garage.ExtPosZ);
        }

        /// <summary>
        /// Fetches Garage's Internal Position by ID
        /// </summary>
        /// <param name="garage"></param>
        /// <returns></returns>
        public static Position FetchGarageInternalPosition(Garage garage)
        {
            return new Position(garage.IntPosX, garage.IntPosY, garage.IntPosZ);
        }
    }
}