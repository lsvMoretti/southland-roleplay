using System;
using System.Linq;
using System.Reflection;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Server.Chat;
using Server.Commands;
using Server.Connection;
using Server.Extensions;
using Server.Groups;
using Server.Jobs.Taxi;

namespace Server.Chat
{
    public class ChatExtension
    {
        public void OnChatEvent(IPlayer player, string message)
        {
            try
            {
                if (message.Trim().Length == 0) return;

                player.SetData("AFK:LastMove", DateTime.Now);

                player.SetData("AFK:LastPosition", player.Position);

                if (message[0] == '/')
                {
                    var splitMessage = message.Substring(1).Split(' ');
                    var command = splitMessage[0].Trim().ToLower();
                    splitMessage = splitMessage.Skip(1).ToArray();

                    Logging.AddToCharacterLog(player, $"Has used Command: /{command}. Full: {message}");

                    CommandExtension.Execute(player, command, splitMessage);
                }
                else if (message.Length > 0)
                {
                    OnChatMessage(player, message);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void OnChatMessage(IPlayer player, string message)
        {
            bool hasMuteData = player.GetData("admin:isMuted", out bool isMuted);

            if (hasMuteData && isMuted)
            {
                player.SendErrorNotification("You are muted.");
                return;
            }

            string chatString = string.Join("", message);
            Logging.AddToCharacterLog(player, $"{player.GetClass().Name} says: {message}");

            ChatHandler.SendMessageToNearbyPlayers(player, chatString, MessageType.Talk);
        }
    }
}