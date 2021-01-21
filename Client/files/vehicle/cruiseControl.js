import * as alt from 'alt-client';
import * as native from 'natives';
import { SendNotification } from "../noty/notyHandler";
export let cruiseControlStatus = false;
let vehicleVelocity = null;
export function OnCruiseControlPress() {
    let localPlayer = alt.Player.local;
    if (localPlayer.vehicle == null)
        return;
    let pedInDriverSeat = native.getPedInVehicleSeat(localPlayer.vehicle.scriptID, -1, null);
    if (pedInDriverSeat != localPlayer.scriptID)
        return;
    let vehicleId = alt.Player.local.vehicle.scriptID;
    let engineRunning = native.getIsVehicleEngineRunning(vehicleId);
    if (!engineRunning)
        return;
    let inAir = native.isEntityInAir(vehicleId);
    if (inAir)
        return;
    let vehicleClass = native.getVehicleClass(vehicleId);
    if (vehicleClass == 13 || vehicleClass == 8 || vehicleClass == 16 || vehicleClass == 15) {
        SendNotification('You must be in a suitable vehicle.', 3000, 'warning', 'topCenter');
        return;
    }
    if (!cruiseControlStatus) {
        vehicleVelocity = alt.Player.local.vehicle.speed;
        cruiseControlStatus = true;
        let mph = Math.round(vehicleVelocity * 2.236936);
        if (mph < 15) {
            SendNotification('You must be doing more than 15 MPH.', 3000, 'warning', 'topCenter');
            return;
        }
        SendNotification('Cruise Control set to: ' + mph + ' MPH.', 3000, 'info', 'topCenter');
        return;
    }
    vehicleVelocity = null;
    cruiseControlStatus = false;
    SendNotification('Cruise Control Disabled', 3000, 'info', 'topCenter');
}
alt.everyTick(() => {
    if (!cruiseControlStatus)
        return;
    if (alt.Player.local.vehicle == null) {
        cruiseControlStatus = false;
        vehicleVelocity = null;
        return;
    }
    let localPlayer = alt.Player.local;
    let pedInDriverSeat = native.getPedInVehicleSeat(localPlayer.vehicle.scriptID, -1, null);
    if (pedInDriverSeat != localPlayer.scriptID)
        return;
    let vehicleId = localPlayer.vehicle.scriptID;
    let velocity = alt.Player.local.vehicle.speed;
    let mph = Math.round(velocity * 2.236936);
    if (native.isControlJustPressed(0, 63) && mph > 40) {
        vehicleVelocity = null;
        cruiseControlStatus = false;
        SendNotification('Cruise Control Disabled', 3000, 'info', 'topCenter');
        return;
    }
    if (native.isControlJustPressed(0, 64) && mph > 40) {
        vehicleVelocity = null;
        cruiseControlStatus = false;
        SendNotification('Cruise Control Disabled', 3000, 'info', 'topCenter');
        return;
    }
    if (native.isControlJustPressed(0, 71)) {
        vehicleVelocity = null;
        cruiseControlStatus = false;
        SendNotification('Cruise Control Disabled', 3000, 'info', 'topCenter');
        return;
    }
    if (native.isControlJustPressed(0, 72)) {
        vehicleVelocity = null;
        cruiseControlStatus = false;
        SendNotification('Cruise Control Disabled', 3000, 'info', 'topCenter');
        return;
    }
    if (native.isControlJustPressed(0, 76)) {
        vehicleVelocity = null;
        cruiseControlStatus = false;
        SendNotification('Cruise Control Disabled', 3000, 'info', 'topCenter');
        return;
    }
    let engineRunning = native.getIsVehicleEngineRunning(vehicleId);
    if (!engineRunning) {
        vehicleVelocity = null;
        cruiseControlStatus = false;
        SendNotification('Cruise Control Disabled', 3000, 'info', 'topCenter');
        return;
    }
    let inAir = native.isEntityInAir(vehicleId);
    if (inAir) {
        vehicleVelocity = null;
        cruiseControlStatus = false;
        SendNotification('Cruise Control Disabled', 3000, 'info', 'topCenter');
        return;
    }
    native.setVehicleForwardSpeed(vehicleId, vehicleVelocity);
});
