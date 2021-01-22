using System;
using AltV.Net.Data;

namespace Server.Character
{
    public class BodyDamage
    {
        public BodyPart BodyPart { get; }
        public int Count { get; set; }
        public ushort DamageAmount { get; set; }
        public uint Weapon { get; set; }

        public BodyDamage()
        {
        }

        public BodyDamage(BodyPart bodyPart, int count, ushort damageAmount, uint weapon)
        {
            BodyPart = bodyPart;
            Count = count;
            DamageAmount = damageAmount;
            Weapon = weapon;
        }
    }
}