using System.Collections.Generic;
using System.Linq;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using Server.Chat;
using Server.Commands;
using Server.Extensions;
using Server.Models;

namespace Server.Motel
{
    public class MotelCommands
    {
        [Command("rentroom", commandType: CommandType.Property, description: "Motel: Used to rent a motel room")]
        public static void MotelCommandRentRoom(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            Models.Character playerCharacter = player.FetchCharacter();

            if (playerCharacter == null)
            {
                player.SendPermissionError();
                return;
            }

            if (playerCharacter.RentingMotelRoom)
            {
                player.SendErrorNotification("You are renting a room already!");
                return;
            }

            MotelRoom nearRoom = MotelHandler.FetchNearestMotelRoom(player.Position);

            if (nearRoom == null)
            {
                player.SendErrorNotification("You are not near a room!");
                return;
            }

            if (nearRoom.OwnerId != 0)
            {
                player.SendErrorNotification("This room is already taken!");
                return;
            }

            if (playerCharacter.Money < nearRoom.Value)
            {
                player.SendErrorNotification($"You require {nearRoom.Value:C} to rent this room.");
                return;
            }

            using Context context = new Context();

            var motel = context.Motels.Find(nearRoom.MotelId);

            if (motel == null)
            {
                player.SendErrorNotification("An error occurred fetching the motel.");
                return;
            }

            List<MotelRoom> motelRooms = JsonConvert.DeserializeObject<List<MotelRoom>>(motel.RoomList);

            MotelRoom room = motelRooms.FirstOrDefault(x => x.Id == nearRoom.Id);

            room.OwnerId = playerCharacter.Id;

            motel.RoomList = JsonConvert.SerializeObject(motelRooms);

            Models.Character character = context.Character.Find(playerCharacter.Id);

            character.RentingMotelRoom = true;

            context.SaveChanges();
            

            player.RemoveCash(room.Value);

            player.SendInfoNotification($"You are now renting room {room.Id} at the {motel.Name}. This has cost you {room.Value:C}.");
            
            MotelHandler.ReloadMotel(motel);
        }

        [Command("unrentroom", commandType: CommandType.Property, description: "Motel: Used to un rent the room")]
        public static void MotelCommandUnRent(IPlayer player)
        {
            if (!player.IsSpawned()) return;

            Models.Character playerCharacter = player.FetchCharacter();

            if (playerCharacter == null)
            {
                player.SendPermissionError();
                return;
            }

            if (!playerCharacter.RentingMotelRoom)
            {
                player.SendErrorNotification("You aren't renting a room!");
                return;
            }

            MotelRoom nearRoom = MotelHandler.FetchNearestMotelRoom(player.Position);

            if (nearRoom == null)
            {
                player.SendErrorNotification("You are not near a room!");
                return;
            }

            if (nearRoom.OwnerId != playerCharacter.Id)
            {
                player.SendErrorNotification("You don't rent this room!");
                return;
            }

            using Context context = new Context();

            var motel = context.Motels.Find(nearRoom.MotelId);

            if (motel == null)
            {
                player.SendErrorNotification("An error occurred fetching the motel.");
                return;
            }

            List<MotelRoom> motelRooms = JsonConvert.DeserializeObject<List<MotelRoom>>(motel.RoomList);

            MotelRoom room = motelRooms.FirstOrDefault(x => x.Id == nearRoom.Id);

            room.OwnerId = 0;

            motel.RoomList = JsonConvert.SerializeObject(motelRooms);

            Models.Character character = context.Character.Find(playerCharacter.Id);

            character.RentingMotelRoom = false;

            context.SaveChanges();
            

            player.SendInfoNotification($"You have un rented this room.");

            MotelHandler.ReloadMotel(motel);
        }
    }
}