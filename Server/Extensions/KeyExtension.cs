using AltV.Net.Elements.Entities;
using Server.Character;
using Server.Chat;
using Server.Groups.Police;
using Server.Property;
using Server.Vehicle;
using Server.Weapons;

namespace Server.Extensions
{
    public class KeyExtension
    {
        public static void OnKeyUpEvent(IPlayer player, string key)
        {
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
                if (Vehicle.Commands.EngineCommand(player)) return;
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