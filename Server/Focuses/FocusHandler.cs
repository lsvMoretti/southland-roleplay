using System;
using System.Collections.Generic;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using Server.Extensions;
using Server.Extensions.Blip;
using Server.Extensions.TextLabel;
using Blip = Server.Objects.Blip;

namespace Server.Focuses
{
    public class FocusHandler
    {
        public static readonly Position MechanicPosition = new Position(-29.512087f, -1103.8418f, 26.415405f);
        
        public static void InitFocuses()
        {
            TextLabel mechanicLabel = new TextLabel("Mechanic Focus \n /joinfocus mechanic", MechanicPosition, TextFont.FontChaletComprimeCologne, new LsvColor(255, 131, 0));

            mechanicLabel.Add();

            Blip mechanicBlip = new Blip("Mechanic", MechanicPosition, 402, 6, 0.7f);

            mechanicBlip.Add();
        }

        public static void SetStealthStatusForPlayer(IPlayer player, bool status)
        {
            using Context context = new Context();

            Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

            if (playerCharacter == null) return;

            List<FocusTypes> characterFocuses =
                JsonConvert.DeserializeObject<List<FocusTypes>>(playerCharacter.FocusJson);

            if (characterFocuses.Contains(FocusTypes.Stealth))
            {
                characterFocuses.Remove(FocusTypes.Stealth);
            }

            if (status)
            {
                characterFocuses.Add(FocusTypes.Stealth);
            }

            playerCharacter.FocusJson = JsonConvert.SerializeObject(characterFocuses);

            context.SaveChanges();

            bool hasStealthData = player.GetSyncedMetaData("StealthStatus", out bool stealthStatus);

            if (hasStealthData && stealthStatus && status) return;

            if (hasStealthData)
            {
                if (!status)
                {
                    player.DeleteSyncedMetaData("StealthStatus");
                    return;
                }
            }

            if (status && !hasStealthData)
            {
                player.SetSyncedMetaData("StealthStatus", true);
            }
        }
    }
}