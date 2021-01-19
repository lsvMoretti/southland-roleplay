using System.Linq;
using AltV.Net;
using AltV.Net.Elements.Entities;
using Server.Chat;
using Server.Commands;
using Server.Discord;
using Server.Extensions;
using Server.Models;

namespace Server.Admin
{
    public class HelperCommands
    {
        public static string HelperDutyData = "HELPERONDUTY";

        [Command("tduty", commandType: CommandType.Helper, description: "Toggles on / off duty status for testers")]
        public static void HelperDutyCommand(IPlayer player)
        {
            if (!player.GetClass().Spawned) return;

            if (!player.FetchAccount().Tester)
            {
                player.SendPermissionError();
                return;
            }

            bool hasData = player.HasSyncedMetaData(HelperDutyData);

            if (!hasData)
            {
                // Not on duty
                player.SetSyncedMetaData(HelperDutyData, true);
                player.SendInfoNotification("You've gone on Helper Duty!");
                player.GetClass().Name = player.GetClass().UcpName;
                return;
            }

            player.DeleteSyncedMetaData(HelperDutyData);
            player.GetClass().Name = player.FetchCharacter().Name;
            player.SendInfoNotification("You've gone off Helper Duty!");
        }

        [Command("ah", onlyOne: true, alternatives: "accepthelp", commandType: CommandType.Helper, description: "Accepts a Help Request!")]
        public static void AcceptHelpCommand(IPlayer player, string idString = "")
        {
            if (idString == "")
            {
                player.SendSyntaxMessage("/ah [reportId]");
                return;
            }

            bool hasDutyData = player.GetSyncedMetaData(HelperDutyData, out bool onDuty);

            if (!hasDutyData || !onDuty)
            {
                player.SendErrorNotification("Your not on duty!");
                return;
            }

            bool tryParse = int.TryParse(idString, out int id);

            if (!tryParse)
            {
                player.SendErrorNotification("You must enter a number.");
                return;
            }

            HelpReport helpReport = AdminHandler.HelpReports.FirstOrDefault(x => x.Id == id);

            if (helpReport == null)
            {
                player.SendErrorNotification("Report not found!");
                return;
            }

            IPlayer targetPlayer = Alt.Server.GetPlayers().FirstOrDefault(x => x == helpReport.Player);

            if (targetPlayer == null)
            {
                player.SendErrorNotification("Player not found!");
                return;
            }

            if (player == targetPlayer)
            {
                player.SendErrorNotification("You can not accept your own requests!");
                return;
            }

            AdminHandler.HelpReports.Remove(helpReport);

            targetPlayer.SendInfoNotification(
                $"Your help request Id {helpReport.Id} has been accepted. Please await for them to contact you.");

            player.SendInfoNotification($"You have accepted help request Id {helpReport.Id}. Message: {helpReport.Message}.");
            player.SendInfoNotification($"Player Id: {targetPlayer.GetPlayerId()}");

            var onlineHelpers = Alt.Server.GetPlayers()
                .Where(x => x.FetchAccount()?.Tester == true).ToList();

            if (onlineHelpers.Any())
            {
                foreach (IPlayer onlineHelper in onlineHelpers)
                {
                    if (!onlineHelper.HasSyncedMetaData(HelperDutyData)) return;

                    onlineHelper.SendHelperMessage(
                        $"Tester {player.FetchAccount().Username} has accepted report Id: {helpReport.Id}.");
                }
            }

            DiscordHandler.SendMessageToReportsChannel(
                $"Tester {player.FetchAccount().Username} has accepted Help Id {helpReport.Id}");

            Logging.AddToAdminLog(player,
                $"has accepted helpme Id {helpReport.Id} for character {targetPlayer.GetClass().Name}.");
        }

        [Command("dh", onlyOne: true, alternatives: "denyhelp", commandType: CommandType.Helper, description: "Declines a Help Me")]
        public static void HelpCommandDeclineReport(IPlayer player, string idString = "")
        {
            if (idString == "")
            {
                player.SendSyntaxMessage("/dr [reportId]");
                return;
            }

            bool hasDutyData = player.GetSyncedMetaData(HelperDutyData, out bool onDuty);

            if (!hasDutyData || !onDuty)
            {
                player.SendErrorNotification("Your not on duty!");
                return;
            }

            bool tryParse = int.TryParse(idString, out int id);

            if (!tryParse)
            {
                player.SendErrorNotification("You must enter a number.");
                return;
            }

            HelpReport helpReport = AdminHandler.HelpReports.FirstOrDefault(x => x.Id == id);

            if (helpReport == null)
            {
                player.SendErrorNotification("Report not found!");
                return;
            }

            IPlayer targetPlayer = Alt.Server.GetPlayers().FirstOrDefault(x => x == helpReport.Player);

            if (targetPlayer == null)
            {
                player.SendErrorNotification("Player not found!");
                return;
            }

            if (player == targetPlayer)
            {
                player.SendErrorNotification("You can not decline your own requests!");
                return;
            }

            AdminHandler.HelpReports.Remove(helpReport);

            targetPlayer.SendInfoNotification($"Your helpme request Id: {helpReport.Id} has been denied.");

            player.SendInfoNotification($"You have declined report Id: {helpReport.Id}.");

            foreach (IPlayer onlineHelper in Alt.Server.GetPlayers()
                .Where(x => x.FetchAccount()?.Tester == true))
            {
                if (!onlineHelper.HasSyncedMetaData(HelperDutyData)) return;

                onlineHelper.SendHelperMessage(
                    $"Tester {player.FetchAccount().Username} has denied help me request Id: {helpReport.Id}.");
            }

            DiscordHandler.SendMessageToReportsChannel(
                $"Tester {player.FetchAccount().Username} has denied help request Id {helpReport.Id}");

            Logging.AddToAdminLog(player,
                $"has denied helpme Id {helpReport.Id} for character {targetPlayer.GetClass().Name}.");
        }
    }
}