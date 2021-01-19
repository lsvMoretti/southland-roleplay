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

        public static Dictionary<int, List<BodyDamage>> DamageDictionary = new Dictionary<int, List<BodyDamage>>();

        public static void AltOnOnPlayerDamage(IPlayer player, IEntity attacker, uint weapon, ushort damage)
        {
        }

        public static bool AltOnOnWeaponDamage(IPlayer player, IEntity target, uint weapon, ushort damage,
            Position shotoffset, BodyPart bodypart)
        {
            Console.WriteLine($"Shot in the {bodypart}");

            if (target.Type == BaseObjectType.Player)
            {
                IPlayer targetPlayer = (IPlayer)target;

                if (targetPlayer.GetClass().AdminDuty)
                {
                    return false;
                }

                int characterId = targetPlayer.GetClass().CharacterId;

                BodyDamage newDamage = new BodyDamage(bodypart, 1, damage);

                bool hasWeaponDamages =
                    DamageDictionary.TryGetValue(characterId, out List<BodyDamage>? weaponDamage);

                if (!hasWeaponDamages || weaponDamage == null)
                {
                    List<BodyDamage> newDamageList = new List<BodyDamage> { newDamage };

                    DamageDictionary.Add(characterId, newDamageList);

                    return true;
                }

                BodyDamage? currentDamage = weaponDamage.FirstOrDefault(x => x.BodyPart == bodypart);

                if (currentDamage == null)
                {
                    DamageDictionary.Remove(characterId);
                    weaponDamage.Add(newDamage);
                    DamageDictionary.Add(characterId, weaponDamage);
                    return true;
                }

                BodyDamage bodyDamage = currentDamage;

                weaponDamage.Remove(currentDamage);

                bodyDamage.Count += 1;
                bodyDamage.DamageAmount += damage;

                DamageDictionary.Remove(characterId);
                weaponDamage.Add(bodyDamage);
                DamageDictionary.Add(characterId, weaponDamage);

                return true;
            }

            return true;
        }

        [Command("damages", onlyOne: true, commandType: CommandType.Character,
            description: "Shows yours or another players damages")]
        public static void CharacterDamagesCommand(IPlayer player, string args = "")
        {
            if (!player.IsSpawned()) return;

            if (args == "")
            {
                int characterId = player.GetClass().CharacterId;

                bool hasOwnDamageData = DamageDictionary.TryGetValue(characterId, out List<BodyDamage>? ownDamages);

                if (!hasOwnDamageData || ownDamages == null)
                {
                    player.SendErrorNotification("You don't have any damages recorded!");
                    return;
                }

                player.SendInfoMessage($"-- Your Damages -- ");

                foreach (BodyDamage ownDamage in ownDamages)
                {
                    string bodyPart = ownDamage.BodyPart switch
                    {
                        BodyPart.Pelvis => "Pelvis",
                        BodyPart.LeftHip => "Left Hip",
                        BodyPart.LeftLeg => "Left Leg",
                        BodyPart.LeftFoot => "Left Foot",
                        BodyPart.RightHip => "Right Hip",
                        BodyPart.RightLeg => "Right Leg",
                        BodyPart.RightFoot => "Right Foot",
                        BodyPart.LowerTorso => "Lower Torso",
                        BodyPart.UpperTorso => "Upper Torso",
                        BodyPart.Chest => "Chest",
                        BodyPart.UnderNeck => "Under Neck",
                        BodyPart.LeftShoulder => "Left Shoulder",
                        BodyPart.LeftUpperArm => "Left Upper Arm",
                        BodyPart.LeftElbow => "Left Elbow",
                        BodyPart.LeftWrist => "Left Wrist",
                        BodyPart.RightShoulder => "Right Shoulder",
                        BodyPart.RightUpperArm => "Right Upper Arm",
                        BodyPart.RightElbow => "Right Elbow",
                        BodyPart.RightWrist => "Right Wrist",
                        BodyPart.Neck => "Neck",
                        BodyPart.Head => "Head",
                        BodyPart.Unknown => "Unknown",
                        _ => "Unknown"
                    };

                    player.SendInfoMessage($"Location: {bodyPart}, Total Damage: {ownDamage.DamageAmount}, Damage Count: {ownDamage.Count}.");
                }

                return;
            }

            IPlayer? targetPlayer = Utility.FindPlayerByNameOrId(args);

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

            bool targetHasDamageData = DamageDictionary.TryGetValue(targetPlayer.GetClass().CharacterId, out List<BodyDamage>? targetBodyDamages);

            if (!targetHasDamageData || targetBodyDamages == null)
            {
                player.SendErrorNotification("They have no damages recorded!");
                return;
            }

            player.SendInfoMessage($"-- Damages for {targetPlayer.GetClass().Name} --");

            foreach (BodyDamage targetBodyDamage in targetBodyDamages)
            {
                string bodyPart = targetBodyDamage.BodyPart switch
                {
                    BodyPart.Pelvis => "Pelvis",
                    BodyPart.LeftHip => "Left Hip",
                    BodyPart.LeftLeg => "Left Leg",
                    BodyPart.LeftFoot => "Left Foot",
                    BodyPart.RightHip => "Right Hip",
                    BodyPart.RightLeg => "Right Leg",
                    BodyPart.RightFoot => "Right Foot",
                    BodyPart.LowerTorso => "Lower Torso",
                    BodyPart.UpperTorso => "Upper Torso",
                    BodyPart.Chest => "Chest",
                    BodyPart.UnderNeck => "Under Neck",
                    BodyPart.LeftShoulder => "Left Shoulder",
                    BodyPart.LeftUpperArm => "Left Upper Arm",
                    BodyPart.LeftElbow => "Left Elbow",
                    BodyPart.LeftWrist => "Left Wrist",
                    BodyPart.RightShoulder => "Right Shoulder",
                    BodyPart.RightUpperArm => "Right Upper Arm",
                    BodyPart.RightElbow => "Right Elbow",
                    BodyPart.RightWrist => "Right Wrist",
                    BodyPart.Neck => "Neck",
                    BodyPart.Head => "Head",
                    BodyPart.Unknown => "Unknown",
                    _ => "Unknown"
                };

                player.SendInfoMessage($"Location: {bodyPart}, Total Damage: {targetBodyDamage.DamageAmount}, Damage Count: {targetBodyDamage.Count}.");
            }
        }
    }
}