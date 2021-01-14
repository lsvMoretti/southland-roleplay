using System.Linq;
using AltV.Net.Elements.Entities;
using Server.Chat;
using Server.Commands;
using Server.Extensions;
using Server.Models;

namespace Server.Doors
{
    public class DoorCommands
    {
        [Command("dlock", onlyOne: true,commandType: CommandType.Property,
            description: "Doors: Used to lock/unlock registered doors")]
        public static bool DoorCommandLock(IPlayer player, string args = "")
        {
            if (!player.IsSpawned()) return false;

            Door nearestDoor = null;

            if (args == "")
            {
                nearestDoor = Door.FetchNearestDoor(player, 3f);
            }
            else
            {
                bool tryParse = int.TryParse(args, out int doorId);

                if (!tryParse)
                {
                    player.SendErrorNotification("Door Id must be a number.");
                    return false;
                }

                nearestDoor = Door.FetchDoor(doorId);
            }

            if (nearestDoor == null) return false;

            bool canLock = false;

            if (nearestDoor.OwnerId > 0)
            {
                canLock = nearestDoor.OwnerId == player.GetClass().CharacterId;
            }

            if (nearestDoor.FactionId > 0)
            {
                if (!canLock)
                {
                    canLock = nearestDoor.FactionId == player.FetchCharacter().ActiveFaction;
                }
            }

            if (nearestDoor.PropertyId > 0)
            {
                if (!canLock)
                {
                    Models.Property property = Models.Property.FetchProperty(nearestDoor.PropertyId);

                    if (property != null)
                    {
                    
                        Inventory.Inventory playerInventory = player.FetchInventory();

                        canLock = playerInventory.GetInventoryItems("ITEM_PROPERTY_KEY")
                            .FirstOrDefault(x => x.ItemValue == property.Key) != null;
                    }
                }

            }

            if (!canLock)
            {
                canLock = player.GetClass().AdminDuty;
            }

            if (!canLock)
            {
                player.SendPermissionError();
                return true; 
            }

            using Context context = new Context();

            Door doorDb = context.Doors.Find(nearestDoor.Id);

            doorDb.Locked = !doorDb.Locked;

            context.SaveChanges();

            string state = doorDb.Locked ? "locked" : "unlocked";

            player.SendNotification($"~y~You've {state} the door.");

            DoorHandler.UpdateDoorsForAllPlayers();

            return true;
        }

        [Command("did", commandType: CommandType.Property, description: "Doors: Used to find a door id")]
        public static void DoorCommandId(IPlayer player)
        {
            if (!player.IsSpawned())
                return;

            Door nearestDoor = Door.FetchNearestDoor(player, 3f);

            if (nearestDoor == null)
            {
                player.SendNotification("~r~Door not found.");
                return;
            }

            player.SendNotification($"Door Id: {nearestDoor.Id}.");
        }
    }
}