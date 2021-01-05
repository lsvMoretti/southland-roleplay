using System;
using System.Collections.Generic;
using System.Linq;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using Newtonsoft.Json;
using Server.Animation;
using Server.Character.Clothing;
using Server.Chat;
using Server.Extensions;

namespace Server.Character
{
    public class CharacterCreator
    {
        public static double SurgeonCost = 0;

        /// <summary>
        /// Sends a player to the creator
        /// </summary>
        /// <param name="player"></param>
        /// <param name="reason">0 - Surgery, 1 - First Time</param>
        public static void SendToCreator(IPlayer player, int reason = 0)
        {
            CharacterHandler.SaveCharacterPosition(player);

            player.SetData("LastPos", player.Position);
            player.Position = CreatorRoom.CreatorPosition;
            player.Emit("loadCharacterCreator", JsonConvert.SerializeObject(CustomCharacter.DefaultCharacter()), JsonConvert.SerializeObject(CustomCharacter.DefaultCharacter()));
            player.Dimension = (short)player.GetPlayerId();
            player.Rotation = CreatorRoom.CreatorRotation;
            player.HideChat(true);
            player.ShowHud(true);
            player.SetData("SENTTOCREATOR", reason);
            player.GetClass().CreatorRoom = true;
            player.GetClass().EditingCharacter = true;

            player.SetData("LastPos", player.Position);

            //Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.StopOnLastFrame, "amb@world_human_stand_guard@male@idle_a", "idle_a");
        }

        /// <summary>
        /// Sets the gender when selected on UI
        /// </summary>
        /// <param name="player"></param>
        /// <param name="newGender">0 = Male / 1 = Female</param>
        public static void OnGenderChange(IPlayer player, int newGender)
        {
            if (newGender == 0)
            {
                player.Model = (uint)PedModel.FreemodeMale01;
            }
            else
            {
                player.Model = (uint)PedModel.FreemodeFemale01;
            }

            player.Emit("creatorSetGender", newGender);
        }

        public static void OnCreationFinished(IPlayer player, string characterJson)
        {
            player.GetClass().EditingCharacter = false;
            player.GetClass().CreatorRoom = false;
            player.HideChat(false);
            player.ShowHud(false);

            using Context context = new Context();
            Models.Character playerCharacter = context.Character.Find(player.GetClass().CharacterId);

            if (playerCharacter == null)
            {
                player.SendErrorNotification("An error occurred fetching your character data.");
                return;
            }

            playerCharacter.CustomCharacter = characterJson;

            CustomCharacter customCharacter = JsonConvert.DeserializeObject<CustomCharacter>(characterJson);

            playerCharacter.Sex = customCharacter.Gender;

            player.Emit("loadCustomPlayer", playerCharacter.CustomCharacter, playerCharacter.ClothesJson, playerCharacter.AccessoryJson);

            List<ClothesData> clothingData =
                JsonConvert.DeserializeObject<List<ClothesData>>(playerCharacter.ClothesJson);

            List<AccessoryData> accessoryData =
                JsonConvert.DeserializeObject<List<AccessoryData>>(playerCharacter.AccessoryJson);

            Clothes.LoadClothes(player, clothingData, accessoryData);

            player.GetData("SENTTOCREATOR", out int creatorReason);

            if (creatorReason == 0)
            {
                player.GetData("LastPos", out Position position);

                player.SendInfoNotification($"You have updated your character. This has cost you {SurgeonCost:C}.");

                //player.Position = new Position(playerCharacter.PosX, playerCharacter.PosY, playerCharacter.PosZ);

                //player.Position = position;

                player.SetPosition(position, player.Rotation, loadWeapon: true);

                player.Dimension = (short)playerCharacter.Dimension;

                player.RemoveCash((float)SurgeonCost);

                player.ShowHud(true);

                player.GetData("INSTOREID", out int storeId);

                Models.Property property = Models.Property.FetchProperty(storeId);

                property?.AddToBalance(SurgeonCost);
            }

            if (creatorReason == 1)
            {
                bool previousTutorialCompleted = Models.Character
                    .FetchCharacters(player.FetchAccount()).Any(x => x.StartStage == 2);

                if (previousTutorialCompleted)
                {
                    playerCharacter.StartStage = 2;
                    playerCharacter.PosX = TutorialHandler.StrawberrySpawnPosition.X;
                    playerCharacter.PosY = TutorialHandler.StrawberrySpawnPosition.Y;
                    playerCharacter.PosZ = TutorialHandler.StrawberrySpawnPosition.Z;

                    playerCharacter.Dimension = 0;
                    context.SaveChanges();

                    CreatorRoom.LeaveCreatorRoom(player);

                    player.GetClass().CompletedTutorial = true;
                }
                else
                {
                    playerCharacter.StartStage = 1;
                    TutorialHandler.StartTutorial(player);
                }
            }

            player.SetWeather(TimeWeather.CurrentWeatherType);

            Handler.StopPlayerAnimation(player);

            context.SaveChanges();

            player.LoadCharacterCustomization();
        }
    }
}