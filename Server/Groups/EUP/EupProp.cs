using System;

namespace Server.Groups.EUP
{
    public class EupProp
    {
        public string Name { get; set; }
        public EupPropType PropType { get; set; }
        public int Slot { get; set; }
        public int[] Data { get; set; }

        public EupProp(string name, EupPropType propType, int slot, int[] data)
        {
            Name = name;
            PropType = propType;
            Slot = slot;
            Data = data;
        }

        
    }

    public enum EupPropType
    {
        Clothing,
        Prop
    }
}