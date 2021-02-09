using AltV.Net.Elements.Entities;
using EntityStreamer;
using Server.Character;
using Server.Character.Clothing;
using Server.Chat;
using Server.Groups.Police;
using Server.Inventory;
using Server.Property;
using Server.Vehicle;
using Server.Weapons;

namespace Server.Extensions
{
    public class KeyExtension
    {
        public static void OnKeyUpEvent(IPlayer player, string key)
        {
            if (key.ToLower() == "u")
            {
                if (!player.IsSpawned()) return;

                if (player.IsDead) return;

                WeaponCommands.Command_Weapons(player);
            }

            if (key.ToLower() == "i")
            {
                if (!player.IsSpawned()) return;

                if (player.IsDead) return;

                InventoryCommands.InventoryCommand(player);
            }

            if (key.ToLower() == "f")
            {
                player.GetData("INCREATORROOM", out bool inCreator);
                if (inCreator)
                {
                    if (player.Position.Distance(CreatorRoom.DoorPosition) <= 2)
                    {
                        if (player.FetchCharacter() == null)
                        {
                            PlayerChatExtension.SendErrorNotification(player, "You need to select a character first!");
                            return;
                        }
                        CreatorRoom.LeaveCreatorRoom(player);
                        return;
                    }
                    CreatorRoom.SelectPlayerCharacter(player);
                    return;
                }
            }

            if (key.ToLower() == "lmb")
            {
                // Left Mouse Button
                WeaponSwitch.OnLeftMouseButton(player);
                return;
            }

            if (key.ToLower() == "y")
            {

                if (!player.IsSpawned()) return;

                if (player.IsDead) return;

                if (Vehicle.Commands.EngineCommand(player)) return;

                if (PropertyCommands.CommandEnterProperty(player)) return;

                if (PropertyCommands.CommandExitProperty(player)) return;

                if (PropertyCommands.BuyCommand(player)) return;

                ClothingCommand.CommandClothes(player);

            }

            if (key.ToLower() == "l")
            {
                // L
                CharacterCommands.CommandLock(player);
                return;
            }

            if (key.ToLower() == "ctrle")
            {
                // Control + E
                SittingHandler.ToggleSitting(player);
                return;
            }
        }
    }
}