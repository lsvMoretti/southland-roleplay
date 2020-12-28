using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;

namespace Server.Models
{
    public class ApartmentComplexes
    {
        [Key]
        public int Id { get; set; }

        public string? ComplexName { get; set; }

        /// <summary>
        /// Position X of the Complex
        /// </summary>
        public float PosX { get; set; }

        /// <summary>
        /// Position Y of the Complex
        /// </summary>
        public float PosY { get; set; }

        /// <summary>
        /// Position Z of the Complex Entrance
        /// </summary>
        public float PosZ { get; set; }

        /// <summary>
        /// JSON of Apartments
        /// </summary>
        public string? ApartmentList { get; set; }

        /// <summary>
        /// Garage Position X
        /// </summary>
        public float GaragePosX { get; set; }

        /// <summary>
        /// Garage Position Y
        /// </summary>
        public float GaragePosY { get; set; }

        /// <summary>
        /// Garage Position Z
        /// </summary>
        public float GaragePosZ { get; set; }

        /// <summary>
        /// Fetches all apartment complexes.
        /// </summary>
        /// <returns></returns>
        public static List<ApartmentComplexes> FetchApartmentComplexes()
        {
            using Context context = new Context();
            return context.ApartmentComplexes.ToList();
        }

        /// <summary>
        /// Fetches an Apartment Complex by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static ApartmentComplexes FetchApartmentComplex(int id)
        {
            using Context context = new Context();
            return context.ApartmentComplexes.Find(id);
        }

        /// <summary>
        /// Fetches an Apartment Complex by Complex Name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ApartmentComplexes FetchApartmentComplex(string? name)
        {
            using Context context = new Context();
            return context.ApartmentComplexes.FirstOrDefault(s => s.ComplexName == name);
        }

        /// <summary>
        /// Fetch nearest Apartment Complex
        /// </summary>
        /// <param name="player"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public static ApartmentComplexes FetchNearestApartmentComplex(IPlayer player, float range)
        {
            Position playerPosition = player.Position;

            List<ApartmentComplexes> apartmentComplexes = FetchApartmentComplexes();

            foreach (ApartmentComplexes complex in apartmentComplexes)
            {
                Position apartmentPosition = new Position(complex.PosX, complex.PosY, complex.PosZ);

                if (playerPosition.Distance(apartmentPosition) <= range)
                {
                    return complex;
                }
            }

            return null;
        }
    }

    public class Apartment
    {
        /// <summary>
        /// Apartment Name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Owner ID of the Apartment
        /// </summary>
        public int Owner { get; set; }

        /// <summary>
        /// Price of Apartment
        /// </summary>
        public int Price { get; set; }

        /// <summary>
        /// Apartment Key Code
        /// </summary>
        public string? KeyCode { get; set; }

        /// <summary>
        /// Interior Name
        /// </summary>
        public string? InteriorName { get; set; }

        /// <summary>
        /// Floor ID
        /// </summary>
        public int Floor { get; set; }

        /// <summary>
        /// If the apartment is locked
        /// </summary>
        public bool Locked { get; set; }

        /// <summary>
        /// JSON of List<string?> for props
        /// </summary>
        public string? PropList { get; set; }
    }
}