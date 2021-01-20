using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AltV.Net.Elements.Entities;
using Server.Chat;
using Server.Commands;
using Server.Models;

namespace Server.Extensions
{
    public class CommandExtension
    {
        private static CommandExtension _instance;
        private static bool _initalized = false;
        private readonly Dictionary<string, CommandRow> _commands;
        public static Dictionary<string, CommandRow> Commands;

        private CommandExtension()
        {
            _commands = new Dictionary<string, CommandRow>();
            Commands = new Dictionary<string, CommandRow>();
        }

        public static void Init()
        {
            try
            {
                if (_initalized) return;
                _initalized = true;

                _instance = new CommandExtension();
                _instance.GetCommandMethods();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public static void Execute(IPlayer player, string commandName, string[] parameters = null)
        {
            IEnumerable<KeyValuePair<string, CommandRow>> allMethods = _instance._commands.Where(t => t.Key.CompareTo(commandName) == 0);

            if (!allMethods.Any(x => x.Key == commandName))
            {
                PlayerChatExtension.SendErrorNotification(player, "Command Unknown. Check /help.");
                return;
            }

            foreach (KeyValuePair<string, CommandRow> entry in allMethods)
            {
                try
                {
                    entry.Value.Execute(player, parameters);
                }
                catch
                {
                    PlayerChatExtension.SendErrorNotification(player, "An error occurred.");
                    return;
                }
            }
        }

        private void GetCommandMethods()
        {
#if RELEASE

            using StreamWriter sw = new StreamWriter($"{Environment.CurrentDirectory}/commandOutput.txt");

            sw.Flush();

#endif

            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                foreach (MethodInfo method in type.GetRuntimeMethods())
                {
                    CommandAttribute? attribute = method.GetCustomAttribute<CommandAttribute>();
                    if (attribute == null) continue;

                    ParameterInfo[] arguments = method.GetParameters();
                    if (arguments.Length == 0 || arguments[0].ParameterType != typeof(IPlayer))
                    {
                        Console.WriteLine($"Command /{attribute.Command} [{type.Namespace}.{type.Name}.{method.Name}] Incorrect argument. (IPlayer).");
                        continue;
                    }

                    if (attribute.OnlyOne && arguments.Length != 2)
                    {
                        Console.WriteLine($"Command /{attribute.Command} [{type.Namespace}.{type.Name}.{method.Name}] Incorrect amount of Parameters (Only one required)");
                        continue;
                    }

                    if (!method.IsStatic)
                    {
                        Console.WriteLine($"Command /{attribute.Command} [{type.Namespace}.{type.Name}.{method.Name}] Method is not static.");
                        continue;
                    }

                    if (attribute.Alternatives != "")
                    {
                        string[] alternativeCommands = attribute.Alternatives.Split(",");

                        foreach (string alternativeCommand in alternativeCommands)
                        {
                            _commands.Add(alternativeCommand.ToLower(), new CommandRow(attribute, method, arguments));
                        }
                    }

#if RELEASE
                    sw.WriteLine($"Command: /{attribute.Command.ToLower()}, Type: {attribute.CommandType.ToString()}, " +
                                 $"Description: {attribute.Description}, Admin: {attribute.AdminLevel.ToString()}, " +
                                 $"Alternative: {attribute.Alternatives}");
#endif

                    _commands.Add(attribute.Command.ToLower(), new CommandRow(attribute, method, arguments));
                    Commands.Add(attribute.Command.ToLower(), new CommandRow(attribute, method, arguments));
                }
            }

#if RELEASE

            sw.Close();

#endif

            Console.WriteLine($"Loaded {_commands.Count} command(s).");
        }
    }
}