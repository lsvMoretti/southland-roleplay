using System;
using AltV.Net.Data;

namespace Server.Character
{
    public class BodyDamage
    {
        public BodyPart BodyPart { get; }
        public int Count { get; set; }
        public ushort DamageAmount { get; set; }

        public BodyDamage()
        {
        }

        public BodyDamage(BodyPart bodyPart, int count, ushort damageAmount)
        {
            Console.WriteLine($"new BodyDamage: {bodyPart}");
            BodyPart = bodyPart;
            Console.WriteLine($"Body Part set: {BodyPart}");
            Count = count;
            DamageAmount = damageAmount;
        }
    }
}