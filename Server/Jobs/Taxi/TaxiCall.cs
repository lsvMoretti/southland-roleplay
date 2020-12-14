using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Server.Extensions;
using Server.Models;

namespace Server.Jobs.Taxi
{
    public class TaxiCall
    {
        /// <summary>
        /// Unique Taxi Call Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Caller Character Id
        /// </summary>
        public int CallerId { get; set; }

        /// <summary>
        /// Caller Phone Number
        /// </summary>
        public string Number { get; set; }

        /// <summary>
        /// Caller Street
        /// </summary>
        public string Street { get; set; }

        /// <summary>
        /// Caller Area
        /// </summary>
        public string Area { get; set; }

        /// <summary>
        /// Caller Position
        /// </summary>
        public Position Position { get; set; }

        /// <summary>
        /// The Caller's Destination
        /// </summary>
        public string Destination { get; set; }

        public TaxiCall(IPlayer player, string phoneNumber, string street, string area, int callId)
        {
            Id = callId;
            CallerId = player.GetClass().CharacterId;
            Number = phoneNumber;
            Street = street;
            Area = area;
            Position = player.Position;
        }
    }
}