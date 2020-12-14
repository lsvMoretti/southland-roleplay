using System;
using AltV.Net.Elements.Entities;

namespace Server.Admin
{
    public class AdminReport
    {
        public int Id { get; set; }

        public IPlayer Player { get; set; }
        public string Message { get; set; }

        public DateTime Time { get; set; }

        public AdminReport(int id, IPlayer player, string message)
        {
            Id = id;
            Player = player;
            Message = message;
            Time = DateTime.Now;
        }
    }
}