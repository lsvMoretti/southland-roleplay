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

        public static void SendMessageToReport(int reportId, string messageText)
        {
            AdminReport adminReport = AdminReports.FirstOrDefault(x => x.Id == reportId);

            if (adminReport == null) return;

            IPlayer reportPlayer = adminReport.Player;

            if (reportPlayer == null) return;
            
            reportPlayer.SendAdminMessage($"Report Message: {messageText}");
        }

        public static void CloseReport(int reportId)
        {
            AdminReport adminReport = AdminReports.FirstOrDefault(x => x.Id == reportId);

            if (adminReport == null) return;

            AdminReports.Remove(adminReport);

            AdminReportObject reportObject = AdminReportObjects.FirstOrDefault(x => x.Id == reportId);

            if (reportObject == null) return;

            AdminReportObjects.Remove(reportObject);
            
            
            var onlineAdmins = Alt.Server.GetPlayers()
                .Where(x => x.FetchAccount()?.AdminLevel >= AdminLevel.Support).ToList();

            if (onlineAdmins.Any())
            {
                foreach (IPlayer onlineAdmin in onlineAdmins)
                {
                    onlineAdmin.SendAdminMessage($"Report ID {reportId} has been closed.");
                }
            }

        }
    }

}