using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Server.Models
{
    public class Faction
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// The name of the Faction
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// The factions main type
        /// </summary>
        public FactionTypes FactionType { get; set; }

        /// <summary>
        /// The sub-type for the faction
        /// </summary>
        public SubFactionTypes SubFactionType { get; set; }

        /// <summary>
        /// Ranks saved into JSON
        /// </summary>
        public string? RanksJson { get; set; }

        /// <summary>
        /// Divisions saved into JSON
        /// </summary>
        public string? DivisionJson { get; set; }

        /// <summary>
        /// Adds a faction to the DB
        /// </summary>
        /// <param name="faction"></param>
        /// <returns>New faction ID</returns>
        public static int AddFaction(Faction faction)
        {
            using Context context = new Context();
            context.Faction.Add(faction);
            context.SaveChanges();

            return faction.Id;
        }

        /// <summary>
        /// Fetches a Faction by Name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static Faction FetchFaction(string name)
        {
            using Context context = new Context();

            return context.Faction.FirstOrDefault(i => i.Name == name);
        }

        /// <summary>
        /// Fetches a Faction by ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Faction FetchFaction(int id)
        {
            using Context context = new Context();

            return context.Faction.Find(id);
        }

        /// <summary>
        /// Fetches List of Factions
        /// </summary>
        /// <returns></returns>
        public static List<Faction> FetchFactions()
        {
            using Context context = new Context();

            return context.Faction.ToList();
        }
    }

    public enum FactionTypes
    {
        Business,
        Faction
    }

    public enum SubFactionTypes
    {
        None,
        Law,
        Medical,
        Government,
        News,
        Humane
    }

    public class Rank
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public bool AddRanks { get; set; }

        public bool Invite { get; set; }

        public bool Promote { get; set; }
        public bool Tow { get; set; }
    }

    public class Division
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}