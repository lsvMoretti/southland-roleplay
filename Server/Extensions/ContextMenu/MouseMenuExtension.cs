using System;
using System.Collections.Generic;
using System.Linq;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.Enums;
using Server.Property;

namespace Server.Extensions
{
    public class MouseMenuExtension
    {
        /// <summary>
        /// Triggered when a player clicks the mouse with the Alt Key toggled
        /// </summary>
        /// <param name="player"></param>
        /// <param name="position"></param>
        public static void OnMouseClick(IPlayer player, Position position)
        {
            if (player.FetchCharacter() == null) return;

            IVehicle targetVehicle = Alt.Server.GetVehicles().FirstOrDefault(x => x.Position.Distance(position) < 10f);

            if (targetVehicle != null)
            {
                OnVehicleClick(player, targetVehicle);
                return;
            }

            if (PropertyEntrances.EnterProperty(player)) return;

            if (PropertyEntrances.ExitProperty(player)) return;

            player.AllowMouseContextMenu(true);
        }

        private static void OnVehicleClick(IPlayer player, IVehicle vehicle)
        {
            Models.Vehicle vehicleData = vehicle.FetchVehicleData();

            if (vehicleData == null) return;
            Console.WriteLine("VehicleData not null");
            List<string> menuItems = new List<string>();

            if (vehicleData.OwnerId == player.FetchCharacterId())
            {
                if (vehicle.LockState == VehicleLockState.Locked)
                {
                    menuItems.Add("Unlock");
                }
                else if (vehicle.LockState == VehicleLockState.Unlocked)
                {
                    menuItems.Add("Lock");
                }
            }

            VehicleDoorState hoodState = (VehicleDoorState)vehicle.GetDoorState((byte)VehicleDoor.Hood);

            if (hoodState == VehicleDoorState.Closed)
            {
                menuItems.Add("Open Hood");
            }
            else if ((int)hoodState >= 1 && (int)hoodState <= 7)
            {
                menuItems.Add("Close Hood");
            }

            VehicleDoorState trunkState = (VehicleDoorState)vehicle.GetDoorState((byte)VehicleDoor.Trunk);

            if (trunkState == VehicleDoorState.Closed)
            {
                menuItems.Add("Open Trunk");
            }
            else if ((int)trunkState >= 1 && (int)trunkState <= 7)
            {
                menuItems.Add("Close Trunk");
            }

            menuItems.Add("Close Menu");

            ContextMenu contextMenu = new ContextMenu("OnVehicleClickMenu", vehicle.Position + new Position(0, 0, 0), menuItems);

            ContextMenu.ShowContextMenu(player, contextMenu);

            player.SetData("OnVehicleClickMenuVehicle", vehicle.Id);
        }

        public static void OnVehicleClickMenu(IPlayer player, string selectedItem)
        {
            if (selectedItem == "Close Menu") return;

            player.GetData("OnVehicleClickMenuVehicle", out ushort vehicleId);

            player.SetData("OnVehicleClickMenuVehicle", null);

            IVehicle targetVehicle = Alt.Server.GetVehicles().FirstOrDefault(x => x.Id == vehicleId);

            if (targetVehicle == null) return;

            if (selectedItem == "Open Hood")
            {
                targetVehicle.SetDoorState((byte)VehicleDoor.Hood, (byte)VehicleDoorState.OpenedLevel7);
                return;
            }

            if (selectedItem == "Close Hood")
            {
                targetVehicle.SetDoorState((byte)VehicleDoor.Hood, (byte)VehicleDoorState.Closed);
                return;
            }

            if (selectedItem == "Open Trunk")
            {
                targetVehicle.SetDoorState((byte)VehicleDoor.Trunk, (byte)VehicleDoorState.OpenedLevel7);
                return;
            }

            if (selectedItem == "Close Trunk")
            {
                targetVehicle.SetDoorState((byte)VehicleDoor.Trunk, (byte)VehicleDoorState.Closed);
                return;
            }

            using Context context = new Context();
            var vehicleDb = context.Vehicle.Find(targetVehicle.GetVehicleId());

            if (vehicleDb == null) return;

            if (selectedItem == "Unlock")
            {
                targetVehicle.LockState = VehicleLockState.Unlocked;
                vehicleDb.Locked = false;
                context.SaveChanges();
                return;
            }

            if (selectedItem == "Lock")
            {
                targetVehicle.LockState = VehicleLockState.Locked;
                vehicleDb.Locked = true;
                context.SaveChanges();
            }
        }
    }
}