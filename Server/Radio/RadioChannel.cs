using System.Collections.Generic;
using Server.Models;

namespace Server.Radio
{
    public class RadioChannel
    {
        public int Channel { get; set; }
        public List<int> Factions { get; set; }
        
        public bool DutyCheck { get; set; }

        public RadioChannel(int channel, List<int> factions, bool dutyCheck = false)
        {
            Channel = channel;
            Factions = factions;
            DutyCheck = dutyCheck;
        }
    }
}