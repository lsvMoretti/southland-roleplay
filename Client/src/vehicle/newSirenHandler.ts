import * as alt from 'alt-client';
import * as native from 'natives';

// Classes

class VehicleSounds {
    constructor(public vehicle:alt.Vehicle, public soundId:number) {
    }
}

class HornTypes {
    constructor(public vehicleModel:string, public vehicleType:VehicleType) {
    }
}

enum VehicleType{
    EMS,
    Fire,
    FBI,
    Police
}

// Variables

let nativeUiMenuOpen:boolean = false;
let activeHorns:Array<VehicleSounds> = new Array<VehicleSounds>();

let hornTypes:Array<HornTypes> = new Array<HornTypes>(
    new HornTypes("AMBULANCE", VehicleType.EMS),
    new HornTypes("LGUARD", VehicleType.EMS),
    new HornTypes("FIRETRUK", VehicleType.Fire),
    new HornTypes("PBFD_E2", VehicleType.Fire),
    new HornTypes("FBI", VehicleType.FBI),
    new HornTypes("FBI2", VehicleType.FBI),
)

let hornKeyDown:boolean = false;

// Keys

alt.on('keyup', (key) => {
    
});

alt.on('keydown', (key) => {
    
});

alt.everyTick(() => {
    // Fetch Vehicle
    let playerVehicle = alt.Player.local.vehicle;
    
    // Check if in vehicle
    if(playerVehicle == undefined) return;
    
    // Check if Emergency Vehicle Class
    if(native.getVehicleClass(playerVehicle.scriptID) != 18) return;

    // Fetch if in front of vehicle
    
    let isDriver:boolean = native.getPedInVehicleSeat(alt.Player.local.vehicle.scriptID, -1, 0) == alt.Player.local.scriptID;
    let isFrontPass:boolean = native.getPedInVehicleSeat(alt.Player.local.vehicle.scriptID, 0, 0) == alt.Player.local.scriptID;

    // If both false, return
    if(!isDriver && !isFrontPass) return;
    
    // Check if NativeUi Open
    if(nativeUiMenuOpen) return;
    
    // Disable GTA Horn Key
    native.disableControlAction(0, 86, true);

    // Check if GTA Horn key is pressed
    if(native.isDisabledControlPressed(0, 86)){
        
        hornKeyDown = true;
        
        // Emit horn function
        alt.emitServer('newSirenHandler:HornPress', playerVehicle);
    }
    if(native.isDisabledControlJustReleased(0, 86)){
        if(!hornKeyDown) return;
        
        hornKeyDown = false;
        
        // Emit Horn Release Function
        alt.emitServer('newSirenHandler:HornRelease', playerVehicle);
    }
    
});

// When Horn Key is pressed, call below function
alt.onServer('newSirenHandler:HornActive', hornActive);
function hornActive(vehicle:alt.Vehicle){
    
    let hornAlreadyActive:boolean = false;
    // Check if Horn already active
    activeHorns.forEach((vehicleSound) => {
       if(vehicleSound.vehicle == vehicle){
           hornAlreadyActive = true;
           return;
       } 
    });
    
    if(hornAlreadyActive) return;
    
    // Create Sound Id
    let soundId:number = native.getSoundId();
    
    // Fetch Vehicle Model
    let vehModel:number = native.getEntityModel(vehicle.scriptID);
    
    // Set default vehicle type
    let vehicleType:VehicleType = VehicleType.Police;
    
    // Check list of vehicle types
    hornTypes.forEach((hornType) => {
        let hashKey = native.getHashKey(hornType.vehicleModel);
        if(hashKey == vehModel){
           vehicleType = hornType.vehicleType;
       } 
    });
    
    // If vehicle type is Fire
    // @ts-ignore
    if (vehicleType == VehicleType.Fire) {
        native.playSoundFromEntity(soundId, "VEHICLES_HORNS_FIRETRUCK_WARNING", vehicle.scriptID, null, false, 0);
    } else {
        native.playSoundFromEntity(soundId, "SIRENS_AIRHORN", vehicle.scriptID, null, false, 0);
    }

    activeHorns.push(new VehicleSounds(vehicle, soundId));
}

// When Horn Key is released, call below function
alt.onServer('newSirenHandler:HornRelease', onHornRelease)
function onHornRelease(vehicle:alt.Vehicle){
    let activeHorn:VehicleSounds = undefined;
    
    // Check through horns to see if the horn is active
    activeHorns.forEach((vehicleSound) => {
       if(vehicleSound.vehicle == vehicle){
           activeHorn = vehicleSound;
       } 
    });
    
    // If horn isn't active, return
    if(activeHorn == undefined) return;
    
    // Stop horn sound
    native.stopSound(activeHorn.soundId);
    native.releaseSoundId(activeHorn.soundId);
    
    // Remove from Array
    const index = activeHorns.indexOf(activeHorn, 0);
    if(index > -1){
        activeHorns.splice(index, 1);
    }
}

// Exported Functions

export function SetNativeUiState(state:boolean){
    nativeUiMenuOpen = state;
}
