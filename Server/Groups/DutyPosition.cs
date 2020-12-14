using AltV.Net.Data;

namespace Server.Groups
{
    public class DutyPosition
    {
        public Position Position { get; set; }
        public DutyPositionType PositionType { get; set; }

        public DutyPosition(Position position, DutyPositionType positionType)
        {
            Position = position;
            PositionType = positionType;
        }
    }
}