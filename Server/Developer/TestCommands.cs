using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using Newtonsoft.Json;
using Pomelo.EntityFrameworkCore.MySql.Query.ExpressionTranslators.Internal;
using Server.Animation;
using Server.Character;
using Server.Chat;
using Server.Commands;
using Server.Extensions;
using Server.Extensions.TextLabel;
using Server.Models;
using Server.Objects;

namespace Server.Developer
{
    public class TestCommands
    {
        [Command("hash", AdminLevel.HeadAdmin, onlyOne: true)]
        public static void CommandHash(IPlayer player, string hash = "")
        {
            if (player.GetClass().AccountId != 1) return;

            Console.WriteLine($"Hashed: {hash} to {Alt.Hash(hash)}");
        }

        [Command("save", onlyOne: true)]
        public static void Command_SavePos(IPlayer player, string sName = "")
        {
            if (player.GetClass().AccountId != 1) return;

            if (sName == "")
            {
                player.SendSyntaxMessage("/save [SaveName]");
                return;
            }

            Position playerPosition = player.Position;
            DegreeRotation playerRotation = player.Rotation;

            if (player.IsInVehicle)
            {
                playerPosition = player.Vehicle.Position;
                playerRotation = player.Vehicle.Rotation;
            }

            File.AppendAllText("savepos.txt",
                $"{player.GetClass().UcpName}, {sName}: {playerPosition.X}f, {playerPosition.Y}f, {playerPosition.Z}f : DegreeRotation Yaw: {playerRotation.Yaw}f\n");

            player.Emit("SendNotification", "info", "Position Saved");
        }

        [Command("range", AdminLevel.Management, true)]
        public static void DevCommandRange(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/range [NameOrId]");
                return;
            }

            IPlayer targetPlayer = Utility.FindPlayerByNameOrId(args);
            if (targetPlayer == null)
            {
                player.SendNotification("~r~Unable to find player");
                return;
            }

            player.SendInfoNotification($"Distance to {targetPlayer.GetClass().Name} (PID: {targetPlayer.GetPlayerId()}) is {player.Position.Distance(targetPlayer.Position)}.");
        }

        [Command("anim", AdminLevel.Tester, true)]
        public static void AnimTest(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/anim [Dict] [Name]");
                return;
            }

            string[] split = args.Split(' ');

            if (split.Length != 2)
            {
                player.SendSyntaxMessage("/anim [Dict] [Name]");
                return;
            }

            Animation.Handler.PlayPlayerAnimationEx(player, (int)AnimationFlags.Loop, split[0], split[1]);
        }

        private static bool _traceStatus = false;

        [Command("trace", AdminLevel.Management)]
        public static void TraceTest(IPlayer player)
        {
            if (!_traceStatus)
            {
                player.SendInfoNotification("Enabled Tracing");

                AltTrace.Start("server");
                _traceStatus = true;
                return;
            }

            player.SendInfoNotification("Disabled Tracing");
            AltTrace.Stop();
            _traceStatus = false;
        }

        [Command("seatid")]
        public static void SeatIdCommand(IPlayer player)
        {
            if (!player.IsInVehicle)
            {
                player.SendErrorNotification("You must be in a vehicle.");
                return;
            }

            player.SendInfoNotification($"Your seat Id is: {player.Seat}.");
        }

        [Command("c", AdminLevel.Tester, true)]
        public static void DevClothesCommand(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/c [Slot] [Draw] [Text]");
                return;
            }

            string[] split = args.Split(' ');

            if (split.Length != 3)
            {
                player.SendSyntaxMessage("/c [Slot] [Draw] [Text]");
                return;
            }

            bool slotParse = int.TryParse(split[0], out int slot);
            bool drawParse = int.TryParse(split[1], out int draw);
            bool textParse = int.TryParse(split[2], out int text);

            if (!slotParse || !drawParse || !textParse)
            {
                player.SendSyntaxMessage("/c [Slot] [Draw] [Text]");
                return;
            }

            player.SetClothes(slot, draw, text);

            player.SendInfoNotification($"Slot: {slot}, Draw: {draw}, Texture: {text}.");
        }

        [Command("ca", AdminLevel.Tester, true)]
        public static void DevAccessoriesCommand(IPlayer player, string args = "")
        {
            if (args == "")
            {
                player.SendSyntaxMessage("/ca [Slot] [Draw] [Text]");
                return;
            }

            string[] split = args.Split(' ');

            if (split.Length != 3)
            {
                player.SendSyntaxMessage("/ca [Slot] [Draw] [Text]");
                return;
            }

            bool slotParse = int.TryParse(split[0], out int slot);
            bool drawParse = int.TryParse(split[1], out int draw);
            bool textParse = int.TryParse(split[2], out int text);

            if (!slotParse || !drawParse || !textParse)
            {
                player.SendSyntaxMessage("/ca [Slot] [Draw] [Text]");
                return;
            }

            player.SetAccessory(slot, draw, text);

            player.SendInfoNotification($"Slot: {slot}, Draw: {draw}, Texture: {text}.");
        }

        [Command("lipl", onlyOne: true, adminLevel: AdminLevel.Management)]
        public static void DevLoadIpl(IPlayer player, string args = "")
        {
            player.RequestIpl(args);
        }

        [Command("uipl", onlyOne: true, adminLevel: AdminLevel.Management)]
        public static void DevUnloadIpl(IPlayer player, string args = "")
        {
            player.UnloadIpl(args);
        }
    }
}