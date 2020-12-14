import * as alt from 'alt-client';
import * as native from 'natives';
let sirenSoundEnable = false;
let oldSirenSoundState = false;
let sirenLightEnable = false;
let oldSirenLightState = false;
let lastSirenState = 0;
let EMSModels = [
    "AMBULANCE",
    "FIRETRUK",
    "LGUARD"
];
let FireModels = [
    "FIRETRUK",
    "PBFD_E2"
];
let FBIModels = [
    "FBI",
    "FBI2"
];
alt.onServer('OnSetSirenSoundState', (vehicle, state) => {
    setSirenStateForVehicle(vehicle.scriptID, state);
});
alt.onServer('OnSetSirenLightState', (vehicle, state) => {
    setSirenLightStateForVehicle(vehicle.scriptID, state);
});
export function toggleSirenState() {
    sirenSoundEnable = !sirenSoundEnable;
}
let soundIds = [];
let hornSoundIds = [];
let vehicleHorns = [];
let oldVehicleHorns = [];
alt.everyTick(() => {
    let vehicle = alt.Player.local.vehicle;
    if (vehicle == undefined)
        return;
    if (sirenSoundEnable != oldSirenSoundState) {
        if (sirenSoundEnable) {
            alt.emitServer('SetSirenStateForVehicle', vehicle, 1);
        }
        if (!sirenSoundEnable) {
            alt.emitServer('SetSirenStateForVehicle', vehicle, 0);
        }
        oldSirenSoundState = sirenSoundEnable;
    }
    if (sirenLightEnable != oldSirenLightState) {
        if (sirenLightEnable) {
            alt.emitServer('SetSirenLightState', vehicle, 1);
        }
        if (!sirenLightEnable) {
            alt.emitServer('SetSirenLightState', vehicle, 0);
        }
        oldSirenLightState = sirenLightEnable;
    }
    if (vehicleHorns.length > 0) {
        try {
            vehicleHorns.forEach((state, index) => {
                let soundId = hornSoundIds[index];
                if (soundId === undefined) {
                    soundId = native.getSoundId();
                    hornSoundIds[index] = soundId;
                }
                let oldState = oldVehicleHorns[index];
                if (state && !oldState) {
                    let vehModel = native.getEntityModel(index);
                    let isFireVehicle = false;
                    FireModels.forEach(fireModel => {
                        let hashKey = native.getHashKey(fireModel);
                        if (vehModel == hashKey) {
                            isFireVehicle = true;
                        }
                    });
                    if (isFireVehicle) {
                        native.playSoundFromEntity(soundId, "VEHICLES_HORNS_FIRETRUCK_WARNING", index, null, false, 0);
                    }
                    else {
                        native.playSoundFromEntity(soundId, "SIRENS_AIRHORN", index, null, false, 0);
                    }
                }
                if (!state && oldState) {
                    native.stopSound(soundId);
                    hornSoundIds[index] = undefined;
                }
                oldVehicleHorns[index] = state;
            });
        }
        catch (e) {
            alt.logError(e);
            return;
        }
    }
});
function setSirenStateForVehicle(vehicle, newState) {
    lastSirenState = newState;
    alt.log('Siren State: ' + newState);
    if (soundIds[vehicle] != undefined) {
        native.stopSound(soundIds[vehicle]);
        soundIds[vehicle] = native.getSoundId();
    }
    if (newState == 0) {
        native.stopSound(soundIds[vehicle]);
        soundIds[vehicle] = undefined;
    }
    if (newState == 1) {
        if (!native.isVehicleSirenOn(vehicle))
            return;
        if (soundIds[vehicle] === undefined || soundIds[vehicle] === null) {
            soundIds[vehicle] = native.getSoundId();
        }
        else {
            native.stopSound(soundIds[vehicle]);
        }
        let vehModel = native.getEntityModel(vehicle);
        let isEmsVehicle = false;
        EMSModels.forEach(emsModel => {
            let hashKey = native.getHashKey(emsModel);
            if (vehModel == hashKey) {
                isEmsVehicle = true;
            }
        });
        if (isEmsVehicle) {
            native.playSoundFromEntity(soundIds[vehicle], "VEHICLES_HORNS_SIREN_1", vehicle, null, false, 0);
        }
        let isFireVehicle = false;
        FireModels.forEach(fireModel => {
            let hashKey = native.getHashKey(fireModel);
            if (vehModel == hashKey) {
                isFireVehicle = true;
            }
        });
        if (isFireVehicle) {
            native.playSoundFromEntity(soundIds[vehicle], "collision_i8o7bp", vehicle, null, false, 0);
        }
        let isFBIVehicle = false;
        FBIModels.forEach(fbiModel => {
            let hashKey = native.getHashKey(fbiModel);
            if (vehModel == hashKey) {
                isFBIVehicle = true;
            }
        });
        if (isFBIVehicle) {
            native.playSoundFromEntity(soundIds[vehicle], "collision_jzwc1b", vehicle, null, false, 0);
        }
        else {
            native.playSoundFromEntity(soundIds[vehicle], "VEHICLES_HORNS_SIREN_1", vehicle, null, false, 0);
        }
    }
    if (newState == 2) {
        if (!native.isVehicleSirenOn(vehicle))
            return;
        if (soundIds[vehicle] === undefined || soundIds[vehicle] === null) {
            soundIds[vehicle] = native.getSoundId();
        }
        else {
            native.stopSound(soundIds[vehicle]);
        }
        let vehModel = native.getEntityModel(vehicle);
        let isEmsVehicle = false;
        EMSModels.forEach(emsModel => {
            let hashKey = native.getHashKey(emsModel);
            if (vehModel == hashKey) {
                isEmsVehicle = true;
            }
        });
        if (isEmsVehicle) {
            native.playSoundFromEntity(soundIds[vehicle], "VEHICLES_HORNS_SIREN_2", vehicle, null, false, 0);
        }
        let isFireVehicle = false;
        FireModels.forEach(fireModel => {
            let hashKey = native.getHashKey(fireModel);
            if (vehModel == hashKey) {
                isFireVehicle = true;
            }
        });
        if (isFireVehicle) {
            native.playSoundFromEntity(soundIds[vehicle], "VEHICLES_HORNS_SIREN_2", vehicle, null, false, 0);
        }
        let isFBIVehicle = false;
        FBIModels.forEach(fbiModel => {
            let hashKey = native.getHashKey(fbiModel);
            if (vehModel == hashKey) {
                isFBIVehicle = true;
            }
        });
        if (isFBIVehicle) {
            native.playSoundFromEntity(soundIds[vehicle], "collision_c5ass2", vehicle, null, false, 0);
        }
        else {
            native.playSoundFromEntity(soundIds[vehicle], "VEHICLES_HORNS_SIREN_2", vehicle, null, false, 0);
        }
    }
    if (newState == 3) {
        if (!native.isVehicleSirenOn(vehicle))
            return;
        if (soundIds[vehicle] === undefined || soundIds[vehicle] === null) {
            soundIds[vehicle] = native.getSoundId();
        }
        else {
            native.stopSound(soundIds[vehicle]);
        }
        let vehModel = native.getEntityModel(vehicle);
        let isEmsVehicle = false;
        EMSModels.forEach(emsModel => {
            let hashKey = native.getHashKey(emsModel);
            if (vehModel == hashKey) {
                isEmsVehicle = true;
            }
        });
        if (isEmsVehicle) {
            native.playSoundFromEntity(soundIds[vehicle], "VEHICLES_HORNS_AMBULANCE_WARNING", vehicle, null, false, 0);
        }
        let isFireVehicle = false;
        FireModels.forEach(fireModel => {
            let hashKey = native.getHashKey(fireModel);
            if (vehModel == hashKey) {
                isFireVehicle = true;
            }
        });
        if (isFireVehicle) {
            native.playSoundFromEntity(soundIds[vehicle], "VEHICLES_HORNS_AMBULANCE_WARNING", vehicle, null, false, 0);
        }
        let isFBIVehicle = false;
        FBIModels.forEach(fbiModel => {
            let hashKey = native.getHashKey(fbiModel);
            if (vehModel == hashKey) {
                isFBIVehicle = true;
            }
        });
        if (isFBIVehicle) {
            native.playSoundFromEntity(soundIds[vehicle], "VEHICLES_HORNS_POLICE_WARNING", vehicle, null, false, 0);
        }
        else {
            native.playSoundFromEntity(soundIds[vehicle], "VEHICLES_HORNS_POLICE_WARNING", vehicle, null, false, 0);
        }
    }
    if (newState == 4) {
        if (!native.isVehicleSirenOn(vehicle))
            return;
        if (soundIds[vehicle] === undefined || soundIds[vehicle] === null) {
            soundIds[vehicle] = native.getSoundId();
        }
        else {
            native.stopSound(soundIds[vehicle]);
        }
        let vehModel = native.getEntityModel(vehicle);
        let isEmsVehicle = false;
        EMSModels.forEach(emsModel => {
            let hashKey = native.getHashKey(emsModel);
            if (vehModel == hashKey) {
                isEmsVehicle = true;
            }
        });
        if (isEmsVehicle) {
            native.playSoundFromEntity(soundIds[vehicle], "VEHICLES_HORNS_AMBULANCE_WARNING", vehicle, null, false, 0);
        }
        let isFireVehicle = false;
        FireModels.forEach(fireModel => {
            let hashKey = native.getHashKey(fireModel);
            if (vehModel == hashKey) {
                isFireVehicle = true;
            }
        });
        if (isFireVehicle) {
            native.playSoundFromEntity(soundIds[vehicle], "collision_q3nurz", vehicle, null, false, 0);
        }
        let isFBIVehicle = false;
        FBIModels.forEach(fbiModel => {
            let hashKey = native.getHashKey(fbiModel);
            if (vehModel == hashKey) {
                isFBIVehicle = true;
            }
        });
        if (isFBIVehicle) {
            native.playSoundFromEntity(soundIds[vehicle], "collision_jzwc1b", vehicle, null, false, 0);
        }
        else {
            native.playSoundFromEntity(soundIds[vehicle], "VEHICLES_HORNS_SIREN_1", vehicle, null, false, 0);
        }
    }
}
export function toggleSirenLightState() {
    sirenLightEnable = !sirenLightEnable;
}
function setSirenLightStateForVehicle(vehicle, state) {
    if (state == 1) {
        native.setVehicleSiren(vehicle, true);
        native.setVehicleHasMutedSirens(vehicle, true);
    }
    if (state == 0) {
        native.setVehicleSiren(vehicle, false);
        setSirenStateForVehicle(vehicle, 0);
    }
}
export function nextSirenStage() {
    if (lastSirenState >= 4) {
        alt.emitServer('SetSirenStateForVehicle', alt.Player.local.vehicle, 1);
    }
    else {
        let nextStage = lastSirenState + 1;
        alt.emitServer('SetSirenStateForVehicle', alt.Player.local.vehicle, nextStage);
    }
}
export function previousSirenStage() {
    if (lastSirenState <= 1) {
        alt.emitServer('SetSirenStateForVehicle', alt.Player.local.vehicle, 4);
    }
    else {
        let previousStage = lastSirenState - 1;
        alt.emitServer('SetSirenStateForVehicle', alt.Player.local.vehicle, previousStage);
    }
}
var hornState = false;
export function toggleHornState(state) {
    if (state != hornState) {
        hornState = state;
        alt.emitServer('VehicleHornState', alt.Player.local.vehicle, state);
    }
}
alt.onServer('OnVehicleHornState', (vehicle, state) => {
    if (vehicleHorns[vehicle.scriptID] == null || vehicleHorns[vehicle.scriptID] == undefined) {
        vehicleHorns[vehicle.scriptID] = state;
    }
    else {
        vehicleHorns[vehicle.scriptID] = state;
    }
});
export function blipSiren() {
    alt.emitServer('BlipVehicleSiren', alt.Player.local.vehicle);
}
alt.onServer('OnBlipSiren', (vehicle) => {
    native.blipSiren(vehicle.scriptID);
});
