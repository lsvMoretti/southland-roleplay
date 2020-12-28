using System;
using System.Collections.Generic;
using System.Reflection;
using AltV.Net.Elements.Entities;
using Server.Chat;
using Server.Commands;
using Server.Models;

namespace Server.Extensions
{
    public class CommandRow
    {
        private readonly CommandAttribute _classAttribute;
        private readonly MethodInfo _classMethodInfo;
        private readonly ParameterInfo[] _parameters;

        public CommandRow(CommandAttribute attribute, MethodInfo methodInfo, ParameterInfo[] parameters)
        {
            _classAttribute = attribute;
            _classMethodInfo = methodInfo;
            _parameters = parameters;
        }

        public CommandAttribute Attribute => _classAttribute;

        public void Execute(IPlayer player, string[] parameters = null)
        {
            try
            {
                List<object> args = new List<object> { player };

                if (_classAttribute.AdminLevel != AdminLevel.None)
                {
                    Models.Account playerAccount = player.FetchAccount();

                    if (playerAccount == null)
                    {
                        PlayerChatExtension.SendErrorNotification(player, "You don't have permission!");
                        return;
                    }

                    if (playerAccount.AdminLevel < _classAttribute.AdminLevel)
                    {
                        if (!playerAccount.Developer)
                        {
                            PlayerChatExtension.SendErrorNotification(player, "You don't have permission!");
                            return;
                        }
                    }
                }
                if (_classAttribute.OnlyOne)
                {
                    if (parameters != null)
                    {
                        string output = string.Join(' ', parameters);
                        args.Add(output);
                    }
                }
                else
                {
                    if (parameters != null)
                    {
                        args.AddRange(parameters);

                        for (int i = 0; i < _parameters.Length; i++)
                        {
                            if (i == 0) continue;

                            if (i > args.Count - 1)
                            {
                                args.Add(Type.Missing);
                            }
                        }
                    }
                }

                _classMethodInfo.Invoke(null, args.ToArray());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                PlayerChatExtension.SendErrorNotification(player, "An error occurred.");
                return;
            }
        }
    }
}