import * as alt from 'alt-client';
import * as native from 'natives';
import {SendNotification} from "../noty/notyHandler";

export let cruiseControlStatus:boolean = false;
let vehicleVelocity:number = null;

export function OnCruiseControlPress(){
    
    let localPlayer:alt.Player = alt.Player.local;
    
    // Not in vehicle, return
    if(localPlayer.vehicle == null) return;
    
    // Fetch ped in drivers seat
    let pedInDriverSeat:number = native.getPedInVehicleSeat(localPlayer.vehicle.scriptID, -1, null);
    
    // If ped isn't driver
    if(pedInDriverSeat != localPlayer.scriptID) return;

    // ScriptId of vehicle
    let vehicleId:number = alt.Player.local.vehicle.scriptID;
    
    // Get if engine is running
    let engineRunning:boolean = native.getIsVehicleEngineRunning(vehicleId);
    
    // If engine isn't running, return
    if(!engineRunning) return;
    
    // Get if Vehicle is in the air
    let inAir:boolean = native.isEntityInAir(vehicleId);

    // If vehicleHeight is higher than 1m, return
    if(inAir) return;
    
    let vehicleClass:number = native.getVehicleClass(vehicleId);
    
    if(vehicleClass == 13 || vehicleClass == 8 || vehicleClass == 16 || vehicleClass == 15){

        SendNotification('You must be in a suitable vehicle.', 3000, 'warning',
            'topCenter');
        return;
    }
    
    if(!cruiseControlStatus){
        vehicleVelocity = alt.Player.local.vehicle.speed;
        cruiseControlStatus = true;
        let mph = Math.round(vehicleVelocity * 2.236936);
        
        if(mph < 15){
            SendNotification('You must be doing more than 15 MPH.', 3000, 'warning',
                'topCenter');
            return;
        }
        
        SendNotification('Cruise Control set to: '+mph+' MPH.', 3000, 'info', 
            'topCenter');
        return;
    }
    
    vehicleVelocity = null;
    cruiseControlStatus = false;
    SendNotification('Cruise Control Disabled', 3000, 'info', 'topCenter');
}

alt.setInterval(() => {
   if(!cruiseControlStatus) return;
    if(alt.Player.local.vehicle == null){
        cruiseControlStatus = false;
        vehicleVelocity = null;
        return;
    }

    let localPlayer:alt.Player = alt.Player.local;

    // Fetch ped in drivers seat
    let pedInDriverSeat:number = native.getPedInVehicleSeat(localPlayer.vehicle.scriptID, -1, null);

    // If ped isn't driver
    if(pedInDriverSeat != localPlayer.scriptID) return;

    // ScriptId of vehicle
    let vehicleId:number = localPlayer.vehicle.scriptID;

    let velocity = alt.Player.local.vehicle.speed;
    let mph = Math.round(velocity * 2.236936);
    
    if(native.isControlJustPressed(0, 63) && mph > 40){
        // Veh Move Left
        vehicleVelocity = null;
        cruiseControlStatus = false;
        SendNotification('Cruise Control Disabled', 3000, 'info', 'topCenter');
        return;
    }

    if(native.isControlJustPressed(0, 64) && mph > 40){
        // Veh Move Right
        vehicleVelocity = null;
        cruiseControlStatus = false;
        SendNotification('Cruise Control Disabled', 3000, 'info', 'topCenter');
        return;
    }
    
    if(native.isControlJustPressed(0, 71)){
        // Veh Accelerate
        vehicleVelocity = null;
        cruiseControlStatus = false;
        SendNotification('Cruise Control Disabled', 3000, 'info', 'topCenter');
        return;
    }
    
    if(native.isControlJustPressed(0, 72)){
        // Veh Brake
        vehicleVelocity = null;
        cruiseControlStatus = false;
        SendNotification('Cruise Control Disabled', 3000, 'info', 'topCenter');
        return;
    }

    if(native.isControlJustPressed(0, 76)){
        // Handbrake
        vehicleVelocity = null;
        cruiseControlStatus = false;
        SendNotification('Cruise Control Disabled', 3000, 'info', 'topCenter');
        return;
    }
    
    // Get if engine is running
    let engineRunning:boolean = native.getIsVehicleEngineRunning(vehicleId);

    // If engine isn't running, return
    if(!engineRunning){
        vehicleVelocity = null;
        cruiseControlStatus = false;
        SendNotification('Cruise Control Disabled', 3000, 'info', 'topCenter');
        return;
    }
    
    // Get vehicle height (Returns in meters)
    let inAir:boolean = native.isEntityInAir(vehicleId);

    if(inAir) {
        vehicleVelocity = null;
        cruiseControlStatus = false;
        SendNotification('Cruise Control Disabled', 3000, 'info', 'topCenter');
        return;
    }
    
    native.setVehicleForwardSpeed(vehicleId, vehicleVelocity);
}, 0);