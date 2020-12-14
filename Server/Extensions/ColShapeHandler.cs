using System;
using AltV.Net;
using AltV.Net.Elements.Entities;
using Server.Chat;

namespace Server.Extensions
{
    public class ColShapeHandler : IScript
    {
        [ScriptEvent(ScriptEventType.ColShape)]
        public static void OnEntityColshapeHit(IColShape shape, IEntity entity, bool state)
        {
            if (state)
            {
                if (shape.GetData("SpikeStrip", out bool isSpike))
                {
                    if (!isSpike) return;

                    if (entity is IVehicle vehicle)
                    {
                        byte count = vehicle.WheelsCount;
                    
                        for (byte i = 0; i <= count; i++)
                        {
                            vehicle.SetWheelBurst(i, true);
                        }
                    
                        vehicle.Driver.SendInfoMessage("You've hit a spike strip!");
                    }
                }
            }
        }
    }
}