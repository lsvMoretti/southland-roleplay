using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Server.Models;

namespace Server.Groups
{
    public class FactionHandler
    {
        public static bool RemoveFaction(int factionId)
        {
            using Context context = new Context();

            Faction selectedFaction = context.Faction.Find(factionId);

            if (selectedFaction == null) return false;

            List<Models.Character> playerCharacters = context.Character.ToList();

            foreach (Models.Character playerCharacter in playerCharacters)
            {
                if (string.IsNullOrEmpty(playerCharacter.FactionList))
                {
                    playerCharacter.FactionList = JsonConvert.SerializeObject(new List<PlayerFaction>());
                }
                else
                {
                    
                    List<PlayerFaction> playerFactions =
                        JsonConvert.DeserializeObject<List<PlayerFaction>>(playerCharacter.FactionList);

                    if(playerFactions == null) continue;

                    PlayerFaction playerFaction = playerFactions.FirstOrDefault(x => x.Id == factionId);

                    if (playerFaction == null) continue;

                    playerFactions.Remove(playerFaction);

                    playerCharacter.FactionList = JsonConvert.SerializeObject(playerFactions);

                    context.SaveChanges();
                }

            }

            context.Faction.Remove(selectedFaction);

            context.SaveChanges();

            

            return true;
        }
    }
}