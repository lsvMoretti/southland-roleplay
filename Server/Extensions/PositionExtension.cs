using System;
using System.Runtime.CompilerServices;
using System.Transactions;
using AltV.Net.Data;

namespace Server.Extensions
{
    public static class PositionExtension
    {
        private static readonly Random randInstance = new Random();

        public static Position RandomXy()
        {
            Position position = new Position();

            double num = randInstance.NextDouble() * 2.0 * Math.PI;

            position.X = (float)Math.Cos(num);
            position.Y = (float)Math.Sin(num);
            position.Normalize();
            return position;
        }

        public static Position Around(this Position position, float distance)
        {
            return position + RandomXy() * distance;
        }
    }
}