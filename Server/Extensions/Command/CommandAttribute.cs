using Server.Models;
using System;
using System.Collections.Generic;

namespace Server.Commands
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        public string Command { get; }
        public AdminLevel AdminLevel { get; }
        public bool OnlyOne { get; }

        public string Alternatives { get; }

        public CommandType CommandType { get; }

        public string Description { get; }

        /// <summary>
        /// The Command Attribute
        /// </summary>
        /// <param name="command">The Command</param>
        /// <param name="adminLevel">Minimum Admin Level</param>
        /// <param name="onlyOne">Only one Parameter</param>
        /// <param name="alternatives"></param>
        /// <param name="commandType">Specify the Command Type to be generated for help commands</param>
        /// <param name="description">A description to show for the help commands</param>
        public CommandAttribute(string command, AdminLevel adminLevel = AdminLevel.None, bool onlyOne = false, string alternatives = "", CommandType commandType = CommandType.Unassigned, string description = "")
        {
            Command = command;
            AdminLevel = adminLevel;
            OnlyOne = onlyOne;
            Alternatives = alternatives;
            CommandType = commandType;
            Description = description;
        }
    }
}