using System.Collections.Generic;
using System.Linq;
using AltV.Net;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using Server.Chat;
using Server.Extensions;
using Server.Models;

namespace Server.Admin
{
    public class AdminHandler
    {
        #region Help System

        public static List<HelpReport> HelpReports = new List<HelpReport>();

        private static int _nextHelpId = 1;

        public static HelpReport AddHelpReport(IPlayer reporter, string message)
        {
            HelpReport newReport = new HelpReport(_nextHelpId, reporter, message);

            HelpReports.Add(newReport);

            _nextHelpId += 1;

            //TODO: Send to SignalR

            return newReport;
        }

        public static void CloseHelpReport(int reportId)
        {
            HelpReport helpReport = HelpReports.FirstOrDefault(x => x.Id == reportId);

            if (helpReport == null) return;

            HelpReports.Remove(helpReport);

            var onlineHelpers = Alt.GetAllPlayers()
                .Where(x => x.FetchAccount()?.AdminLevel > AdminLevel.Tester).ToList();

            if (!onlineHelpers.Any()) return;

            foreach (IPlayer onlineHelper in onlineHelpers)
            {
                // Not on Helper Duty
                if (!onlineHelper.HasSyncedMetaData(HelperCommands.HelperDutyData)) return;

                onlineHelper.SendHelperMessage($"Help Me Id {reportId} has been closed.");
            }
        }

        public static void SendMessageToHelpMe(int reportId, string messageText)
        {
            HelpReport helpReport = HelpReports.FirstOrDefault(x => x.Id == reportId);

            IPlayer helpPlayer = helpReport?.Player;

            helpPlayer?.SendHelperMessage($"Helper Message: {messageText}");
        }

        #endregion Help System

        #region Report System

        public static List<AdminReport> AdminReports = new List<AdminReport>();

        public static List<AdminReportObject> AdminReportObjects = new List<AdminReportObject>();

        private static int _nextReportId = 1;

        public static AdminReport AddAdminReport(IPlayer reporter, string message)
        {
            AdminReport newReport = new AdminReport(_nextReportId, reporter, message);

            AdminReportObject reportObject = new AdminReportObject(_nextReportId, reporter.GetClass().CharacterId, reporter.GetPlayerId(), reporter.FetchCharacter().Name, message);

            AdminReportObjects.Add(reportObject);

            _nextReportId += 1;

            AdminReports.Add(newReport);

            SignalR.AddReport(reportObject);

            return newReport;
        }

        public static void CloseReport(int reportId)
        {
            AdminReport adminReport = AdminReports.FirstOrDefault(x => x.Id == reportId);

            if (adminReport == null) return;

            AdminReports.Remove(adminReport);

            AdminReportObject reportObject = AdminReportObjects.FirstOrDefault(x => x.Id == reportId);

            if (reportObject == null) return;

            AdminReportObjects.Remove(reportObject);

            var onlineAdmins = Alt.GetAllPlayers()
                .Where(x => x.FetchAccount()?.AdminLevel >= AdminLevel.Tester).ToList();

            if (!onlineAdmins.Any()) return;

            foreach (IPlayer onlineAdmin in onlineAdmins)
            {
                onlineAdmin.SendAdminMessage($"Report Id {reportId} has been closed.");
            }
        }

        public static void SendMessageToReport(int reportId, string messageText)
        {
            AdminReport? adminReport = AdminReports.FirstOrDefault(x => x.Id == reportId);

            IPlayer? reportPlayer = adminReport?.Player;

            reportPlayer?.SendAdminMessage($"Report Message: {messageText}");
        }

        #endregion Report System

        public static Dictionary<int, IVehicle> SpawnedVehicles = new Dictionary<int, IVehicle>();

        public static int NextAdminSpawnedVehicleId = -1;

        public static List<PedModel> AdminModels = new List<PedModel>
        {
            PedModel.Zombie01,
            PedModel.RsRanger01AMO,
            PedModel.MimeSMY,
            PedModel.Pogo01,
            PedModel.Orleans,
            PedModel.Imporage,
            PedModel.FilmNoir
        };
    }
}