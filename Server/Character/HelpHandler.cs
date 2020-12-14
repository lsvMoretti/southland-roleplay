using System.Collections.Generic;
using System.Linq;
using AltV.Net.Elements.Entities;
using Newtonsoft.Json;
using Server.Commands;
using Server.Extensions;
using Server.Models;

namespace Server.Character
{
    public class HelpHandler
    {
        public static void FetchHelpCommands(IPlayer player, string option)
        {
            List<HelpCommand> helpCommands = new List<HelpCommand>();

            if (option == "anim")
            {
                // Fetch list of Animations
                foreach (var command in CommandExtension.Commands.Where(x => x.Value.Attribute.CommandType == CommandType.Anim).OrderByDescending(x => x.Key))
                {
                    helpCommands.Add(new HelpCommand(command.Key, command.Value.Attribute.Description));
                }

                player.Emit($"helpMenu:ReturnAnim", JsonConvert.SerializeObject(helpCommands));
            }

            if (option == "character")
            {
                foreach (var command in CommandExtension.Commands.Where(x => x.Value.Attribute.CommandType == CommandType.Character).OrderByDescending(x => x.Key))
                {
                    helpCommands.Add(new HelpCommand(command.Key, command.Value.Attribute.Description));
                }

                player.Emit($"helpMenu:ReturnAnim", JsonConvert.SerializeObject(helpCommands));
            }

            if (option == "bank")
            {
                foreach (var command in CommandExtension.Commands.Where(x => x.Value.Attribute.CommandType == CommandType.Bank).OrderByDescending(x => x.Key))
                {
                    helpCommands.Add(new HelpCommand(command.Key, command.Value.Attribute.Description));
                }

                player.Emit($"helpMenu:ReturnAnim", JsonConvert.SerializeObject(helpCommands));
            }

            if (option == "faction")
            {
                foreach (var command in CommandExtension.Commands.Where(x => x.Value.Attribute.CommandType == CommandType.Faction).OrderByDescending(x => x.Key))
                {
                    helpCommands.Add(new HelpCommand(command.Key, command.Value.Attribute.Description));
                }

                player.Emit($"helpMenu:ReturnAnim", JsonConvert.SerializeObject(helpCommands));
            }

            if (option == "focus")
            {
                foreach (var command in CommandExtension.Commands.Where(x => x.Value.Attribute.CommandType == CommandType.Focus).OrderByDescending(x => x.Key))
                {
                    helpCommands.Add(new HelpCommand(command.Key, command.Value.Attribute.Description));
                }

                player.Emit($"helpMenu:ReturnAnim", JsonConvert.SerializeObject(helpCommands));
            }

            if (option == "job")
            {
                foreach (var command in CommandExtension.Commands.Where(x => x.Value.Attribute.CommandType == CommandType.Job).OrderByDescending(x => x.Key))
                {
                    helpCommands.Add(new HelpCommand(command.Key, command.Value.Attribute.Description));
                }

                player.Emit($"helpMenu:ReturnAnim", JsonConvert.SerializeObject(helpCommands));
            }
            if (option == "phone")
            {
                foreach (var command in CommandExtension.Commands.Where(x => x.Value.Attribute.CommandType == CommandType.Phone).OrderByDescending(x => x.Key))
                {
                    helpCommands.Add(new HelpCommand(command.Key, command.Value.Attribute.Description));
                }

                player.Emit($"helpMenu:ReturnAnim", JsonConvert.SerializeObject(helpCommands));
            }
            if (option == "vehicle")
            {
                foreach (var command in CommandExtension.Commands.Where(x => x.Value.Attribute.CommandType == CommandType.Vehicle).OrderByDescending(x => x.Key))
                {
                    helpCommands.Add(new HelpCommand(command.Key, command.Value.Attribute.Description));
                }

                player.Emit($"helpMenu:ReturnAnim", JsonConvert.SerializeObject(helpCommands));
            }
            if (option == "chat")
            {
                foreach (var command in CommandExtension.Commands.Where(x => x.Value.Attribute.CommandType == CommandType.Chat).OrderByDescending(x => x.Key))
                {
                    helpCommands.Add(new HelpCommand(command.Key, command.Value.Attribute.Description));
                }

                player.Emit($"helpMenu:ReturnAnim", JsonConvert.SerializeObject(helpCommands));
            }
            if (option == "property")
            {
                foreach (var command in CommandExtension.Commands.Where(x => x.Value.Attribute.CommandType == CommandType.Property).OrderByDescending(x => x.Key))
                {
                    helpCommands.Add(new HelpCommand(command.Key, command.Value.Attribute.Description));
                }

                player.Emit($"helpMenu:ReturnAnim", JsonConvert.SerializeObject(helpCommands));
            }
            if (option == "admin")
            {
                AdminLevel playerAdminLevel = player.FetchAccount().AdminLevel;

                foreach (var command in CommandExtension.Commands.Where(x => x.Value.Attribute.CommandType == CommandType.Admin).OrderByDescending(x => x.Key))
                {
                    if (command.Value.Attribute.AdminLevel > playerAdminLevel) continue;
                    helpCommands.Add(new HelpCommand(command.Key, command.Value.Attribute.Description));
                }

                player.Emit($"helpMenu:ReturnAnim", JsonConvert.SerializeObject(helpCommands));
            }

            if (option == "law")
            {
                foreach (var command in CommandExtension.Commands.Where(x => x.Value.Attribute.CommandType == CommandType.Law).OrderByDescending(x => x.Key))
                {
                    helpCommands.Add(new HelpCommand(command.Key, command.Value.Attribute.Description));
                }
                player.Emit($"helpMenu:ReturnAnim", JsonConvert.SerializeObject(helpCommands));
            }
        }

        public static void OnHelpMenuClose(IPlayer player)
        {
            player.FreezeInput(false);
            player.ChatInput(true);
        }
    }

    public class HelpCommand
    {
        public string Command { get; set; }
        public string Description { get; set; }

        public HelpCommand(string command, string description)
        {
            Command = command;
            Description = description;
        }
    }
}