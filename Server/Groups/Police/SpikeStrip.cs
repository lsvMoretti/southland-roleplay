using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using EntityStreamer;
using Server.Extensions;

namespace Server.Groups.Police
{
    public class SpikeStrip
    {
        public Models.Character Character { get; }
        public int PlayerId { get; }
        public Position Position { get; }
        public int Dimension { get; }

        public Prop Object { get; }

        public IColShape ColShape { get; }

        public SpikeStrip()
        {
        }

        public SpikeStrip(IPlayer player, Position position, Prop prop, IColShape colShape)
        {
            Character = player.FetchCharacter();
            PlayerId = player.GetPlayerId();
            Position = position;
            Dimension = player.Dimension;
            Object = prop;
            ColShape = colShape;
        }
    }
}