using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using Server.Chat;
using Server.Commands;
using Server.Extensions;
using Server.Extensions.TextLabel;

namespace Server.Character
{
    public class DamageHandler
    {
        private const string _weaponDamage = "WeaponDamageData";
        
        public static Dictionary<int, TextLabel> BulletCasingLabels = new Dictionary<int, TextLabel>();
        public static Dictionary<int, BulletTextDraw> BulletCasings = new Dictionary<int, BulletTextDraw>();

        public static int BulletId = 1;

        public static void AltOnOnPlayerDamage(IPlayer player, IEntity attacker, uint weapon, ushort damage)
        {
        }

        public static bool AltOnOnWeaponDamage(IPlayer player, IEntity target, uint weapon, ushort damage,
            Position shotoffset, BodyPart bodypart)
        {
            
            
            bool hasDamageData = player.GetData(_weaponDamage, out string damageJson);
            
            List<BodyDamage> bodyDamages = null;
            
            if (!hasDamageData)
            {
                bodyDamages = new List<BodyDamage>
                {
                    new BodyDamage(bodypart, 1, damage)
                };
                
                player.SetData(_weaponDamage, JsonConvert.SerializeObject(bodyDamages));
                return true;
            }

            bodyDamages = JsonConvert.DeserializeObject<List<BodyDamage>>(damageJson);

            BodyDamage bodyDamage = bodyDamages.FirstOrDefault(x => x.BodyPart == bodypart);

            BodyDamage newDamage = new BodyDamage(bodypart, 1, damage);
            
            if (bodyDamage == null)
            {
                bodyDamages.Add(newDamage);
            }
            else
            {
                bodyDamages.Remove(bodyDamage);
                newDamage.Count = bodyDamage.Count + 1;
                newDamage.DamageAmount = Convert.ToUInt16(bodyDamage.DamageAmount + damage);
                bodyDamages.Add(newDamage);
            }
            
            player.SetData(_weaponDamage, JsonConvert.SerializeObject(bodyDamages));
            return true;
        }

        [Command("damages", onlyOne: true, commandType: CommandType.Character,
            description: "Shows yours or another players damages")]
        public static void CharacterDamagesCommand(IPlayer player, string args = "")
        {
            if (!player.IsSpawned()) return;

            if (args == "")
            {
                bool hasOwnDamageData = player.GetData(_weaponDamage, out string damageString);

                if (!hasOwnDamageData)
                {
                    player.SendErrorNotification("You don't have any damages recorded!");
                    return;
                }

                List<BodyDamage> ownDamages = JsonConvert.DeserializeObject<List<BodyDamage>>(damageString);
                
                player.SendInfoMessage($"-- Your Damages -- ");

                foreach (BodyDamage ownDamage in ownDamages)
                {
                    player.SendInfoMessage($"Location: {ownDamage.BodyPart.ToString()}, Total Damage: {ownDamage.DamageAmount}, Damage Count: {ownDamage.Count}.");
                }

                return;
            }

            IPlayer targetPlayer = Utility.FindPlayerByNameOrId(args);

            if (targetPlayer == null || !targetPlayer.IsSpawned())
            {
                player.SendErrorNotification("Unable to find target player.");
                return;
            }

            if (targetPlayer.Position.Distance(player.Position) > 3)
            {
                player.SendErrorNotification("You must be closer!");
                return;
            }

            bool targetHasDamageData = targetPlayer.GetData(_weaponDamage, out string targetString);

            if (!targetHasDamageData)
            {
                player.SendErrorNotification("They have no damages recorded!");
                return;
            }

            List<BodyDamage> targetBodyDamages = JsonConvert.DeserializeObject<List<BodyDamage>>(targetString);
            
            player.SendInfoMessage($"-- Damages for {targetPlayer.GetClass().Name} --");

            foreach (BodyDamage targetBodyDamage in targetBodyDamages)
            {
                player.SendInfoMessage($"Location: {targetBodyDamage.BodyPart.ToString()}, Total Damage: {targetBodyDamage.DamageAmount}, Damage Count: {targetBodyDamage.Count}.");
            }
        }
    }
}