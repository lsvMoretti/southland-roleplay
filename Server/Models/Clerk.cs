using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AltV.Net.Data;
using Newtonsoft.Json;

namespace Server.Models
{
    public class Clerk
    {
        [Key] public int Id { get; set; }
        public string StoreName { get; set; }
        public ClerkStoreType StoreType { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        public string Positions { get; set; }
        public int PropertyId { get; set; }

        public static List<Clerk> FetchClerks()
        {
            using Context context = new Context();

            return context.Clerks.ToList();
        }

        public static Position FetchPosition(Clerk clerk)
        {
            return new Position(clerk.PosX, clerk.PosY, clerk.PosZ);
        }

        public static List<Position> FetchPositions(Clerk clerk)
        {
            return JsonConvert.DeserializeObject<List<Position>>(clerk.Positions, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
        }

        public static Clerk FetchClerk(int id)
        {
            using Context context = new Context();

            return context.Clerks.Find(id);
        }
    }

    public enum ClerkStoreType
    {
        FuelStation,
        Convenience
    }
}