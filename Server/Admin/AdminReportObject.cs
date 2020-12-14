using System;
using AltV.Net.Elements.Entities;

namespace Server.Admin
{
    public class AdminReportObject
    {
        public int Id { get; }
        
        public int CharacterId { get; }
        public int PlayerId { get; }
        public string CharacterName { get; }
        public string Message { get; }

        public DateTime Time { get;}

        public AdminReportObject(int id, int characterId, int playerId, string characterName, string message)
        {
            Id = id;
            CharacterId = characterId;
            PlayerId = playerId;
            CharacterName = characterName;
            Message = message;
            Time = DateTime.Now;
        }
    }
}