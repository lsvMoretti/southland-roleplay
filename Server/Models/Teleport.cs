using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Server.Models
{
    public class Teleport
    {
        /// <summary>
        /// Unique Teleport Id
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Teleport Name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Teleport Pos X
        /// </summary>
        public float PosX { get; set; }

        /// <summary>
        /// Teleport Pos Y
        /// </summary>
        public float PosY { get; set; }

        /// <summary>
        /// Teleport Pos Z
        /// </summary>
        public float PosZ { get; set; }

        /// <summary>
        /// Adds a teleport
        /// </summary>
        /// <param name="teleport"></param>
        public static void AddTeleport(Teleport teleport)
        {
            using Context context = new Context();
            context.Teleport.Add(teleport);
            context.SaveChanges();
        }

        /// <summary>
        /// Fetch teleeport by name
        /// </summary>
        /// <param name="name">Teleport Name</param>
        /// <returns>Teleport Object</returns>
        public static Teleport FetchTeleport(string name)
        {
            using Context context = new Context();
            return context.Teleport.FirstOrDefault(i => i.Name == name);
        }

        public static List<Teleport> FetchTeleports()
        {
            using Context context = new Context();
            return context.Teleport.ToList();
        }
    }
}