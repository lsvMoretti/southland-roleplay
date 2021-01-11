using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.AspNetCore.SignalR;

namespace SignalR
{
    public class Interface : Hub
    {
        public static Interface Instance;

        private static DateTime GameServerStartTime = DateTime.Now;

        public Interface()
        {
            Instance = this;
        }

        public async void AddToUsergroup(string userGroup, string userName = null)
        {
            if (userGroup == "AltVServer")
            {
                GameServerStartTime = DateTime.Now;
                Console.WriteLine($"AltVServer is up!");
                await Clients.Others.SendAsync("ServerRestart");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, userGroup);
            await Clients.Caller.SendAsync("AddedToUsergroup", userGroup);
        }

        public async void FetchOnlinePlayerList()
        {
            await Clients.Group("AltVServer").SendAsync("FetchOnlinePlayers", Context.ConnectionId);
        }

        public async void SendOnlinePlayerList(string connectionId, string jsonPlayerList)
        {
            await Clients.Client(connectionId).SendAsync("ReceiveOnlinePlayerList", jsonPlayerList);
        }

        public async void SendMessageToACP(string message)
        {
            return;
        }

        public async void SendMessageToPlayer(string playerName, string message)
        {
            await Clients.Group("AltVServer").SendAsync("AltVSendMessageToPlayer", playerName, message);
        }

        private static int currentHour = 0;
        private static int currentMinute = 0;

        public async Task<int[]> FetchGameTime()
        {
            int[] info = new int[2];

            info[0] = currentHour;
            info[1] = currentMinute;

            return info;
            //await Clients.Group("AltVServer").SendAsync("AltVGameTime", Context.ConnectionId);

            //await Clients.Client(Context.ConnectionId).SendAsync("RecieveGameTime", hour, minute);
        }

        public async void SendGameTime(int hour, int minute)
        {
            //await Clients.Client(connectionId).SendAsync("RecieveGameTime", hour, minute);
            currentHour = hour;
            currentMinute = minute;
        }

        public async void SendDiscordMessage(string channelId, string message)
        {
            await Clients.Group("Discord").SendAsync("ReceiveDiscordMessage", channelId, message);
        }

        public async void SendDiscordEmbed(string channelId, string embedJson)
        {
            await Clients.Group("Discord").SendAsync("ReceiveDiscordEmbed", channelId, embedJson);
        }

        public bool RestartGameServer()
        {
            Console.WriteLine($"Restart Sever Request from SignalR");

            Process serverProcess = Process.GetProcesses().FirstOrDefault(x => x.ProcessName.Contains("altv"));

            serverProcess?.Kill();

            ProcessStartInfo startProcess = new ProcessStartInfo($"C:/Game Server/altv-server.exe")
            {
                WorkingDirectory = "C:/Game Server",
                UseShellExecute = true,
            };

            serverProcess = Process.Start(startProcess);

            if (serverProcess == null || !serverProcess.Responding)
            {
                return false;
            }

            return true;
        }

        private static int currentPlayerCount = 0;

        public async Task<int> FetchOnlinePlayerCount()
        {
            return currentPlayerCount;
        }

        public async void ReceivePlayerCount(int playerCount)
        {
            currentPlayerCount = playerCount;
        }

        public async void OnUserLogin(int accountId, string userName)
        {
            await Clients.Others.SendAsync("OnUserLogin", accountId, userName);
        }

        public async void OnUserLogout(int accountId, string userName)
        {
            await Clients.Others.SendAsync("OnUserLogout", accountId, userName);
        }

        public async Task<DateTime> FetchGameServerStartTime()
        {
            return GameServerStartTime;
        }

        public async void SendDiscordUserMessage(string userId, string message)
        {
            await Clients.Groups("Discord").SendAsync("SendMessageToUser", userId, message);
        }

        public async void SendMessageFromAdminChat(string username, string message)
        {
            await Clients.Groups("AltVServer").SendAsync("AltVSendIgAdminMessage", username, message);
        }

        public async void SendGameReport(string reportJson)
        {
            Console.WriteLine($"New Game Report");
            await Clients.Groups("Discord").SendAsync("NewReport", reportJson);
        }

        public async void RemoveGameReport(string reportJson)
        {
            await Clients.Groups("Discord").SendAsync("RemoveReport", reportJson);
        }

        public async void SendMessageToReport(int reportId, string message)
        {
            await Clients.Others.SendAsync("SendMessageToReport", reportId, message);
        }

        public async void CloseReport(int reportId)
        {
            await Clients.Others.SendAsync("CloseReport", reportId);
        }

        public async void SendDiscordReportMessage(int reportId, string message)
        {
            await Clients.Groups("Discord").SendAsync("SendReportReply", reportId, message);
        }

        public async void SendDiscordLinkedMessage(ulong userId)
        {
            await Clients.Groups("Discord").SendAsync("SendLinkedDiscordMessage", userId);
        }
    }
}