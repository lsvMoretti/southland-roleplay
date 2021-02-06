import * as alt from 'alt-client';
import * as native from 'natives';

let LastAmmo: number;
let WeaponHash: any;

native.requestAnimDict("rcmjosh4");
native.requestAnimDict("reaction@intimidation@cop@unarmed");
native.requestAnimDict("reaction@intimidation@1h");

alt.onServer('fetchCurrentAmmo', (returnEvent: string, weaponHash: any) => {
    let ammo = native.getAmmoInPedWeapon(alt.Player.local.scriptID, weaponHash);
    alt.log('Current Ammo: ' + ammo);
    alt.emitServer(returnEvent, ammo);
});

alt.onServer('WeaponEquipped', (weaponHash: any) => {
    native.setPedDropsWeaponsWhenDead(alt.Player.local.scriptID, false);
    alt.log('Weapon Hash: ' + weaponHash);

    WeaponHash = weaponHash;

    LastAmmo = native.getAmmoInPedWeapon(alt.Player.local.scriptID, weaponHash);
});

alt.setInterval(() => {
    let scriptId: number = alt.Player.local.scriptID;

    if (native.isPedArmed(scriptId, 6)) {
        native.disableControlAction(1, 140, true);
        native.disableControlAction(1, 141, true);
        native.disableControlAction(1, 142, true);
    }
}, 0);
/*
alt.setInterval(() => {
    let player: number = alt.Player.local.scriptID;
    let fistHash: number = alt.hash('weapon_unarmed');
    let currentWeapon: [boolean, number] = native.getCurrentPedWeapon(player, fistHash, true);
    if (currentWeapon[0] == true) return;

    let currentAmmo: number = native.getAmmoInPedWeapon(alt.Player.local.scriptID, currentWeapon[1]);

    alt.emitServer('CurrentWeaponAmmo', currentAmmo);
}, 50);*/

alt.onServer('WeaponSwitchAnim', (type: number) => {
    const ped: number = alt.Player.local.scriptID;

    native.clearPedTasks(ped);

    if (type === 0) {
        // Weapon has been unholstered && player is cop
        native.setPedCurrentWeaponVisible(ped, false, true, true, true);
        native.taskPlayAnim(ped,
            "reaction@intimidation@cop@unarmed",
            "intro",
            8.0,
            2.0,
            -1,
            50,
            2.0,
            false,
            false,
            false); // -- Change 50 to 30 if you want to stand still when removing weapon

        alt.setTimeout(() => {
            native.setPedCurrentWeaponVisible(ped, true, true, true, true);
            native.taskPlayAnim(ped, "rcmjosh4", "josh_leadout_cop2", 8.0, 2.0, -1, 48, 10, false, false, false);

            alt.setTimeout(() => {
                native.clearPedTasks(ped);
            }, 400);
        }, 700);
    }

    if (type === 1) {
        // Weapon has been holstered && player is cop
        native.taskPlayAnim(ped,
            "rcmjosh4",
            "josh_leadout_cop2",
            8.0,
            2.0,
            -1,
            48,
            10,
            false,
            false,
            false); // -- Change 50 to 30 if you want to stand still when removing weapon

        alt.setTimeout(() => {
            native.taskPlayAnim(ped, "rcmjosh4", "josh_leadout_cop2", 8.0, 2.0, -1, 48, 10, false, false, false);

            alt.setTimeout(() => {
                native.taskPlayAnim(ped, "reaction@intimidation@cop@unarmed", "outro", 8.0, 2.0, -1, 50, 2, false, false, false);
                alt.setTimeout(() => {
                    native.clearPedTasks(ped);
                }, 60);
            }, 500);
        }, 500);
    }

    if (type === 2) {
        // Weapon has been un holstered
        native.setPedCurrentWeaponVisible(ped, false, true, true, true);
        native.taskPlayAnim(ped,
            "reaction@intimidation@1h",
            "outro",
            8.0,
            3.0,
            -1,
            50,
            0.125,
            false,
            false,
            false); // -- Change 50 to 30 if you want to stand still when removing weapon

        alt.setTimeout(() => {
            native.setPedCurrentWeaponVisible(ped, true, true, true, true);
            alt.setTimeout(() => {
                native.clearPedTasks(ped);
            }, 60);
        }, 700);
    }

    if (type === 3) {
        // Weapon has been holstered
        native.taskPlayAnim(ped,
            "reaction@intimidation@1h",
            "outro",
            8.0,
            3.0,
            -1,
            50,
            0.125,
            false,
            false,
            false); // -- Change 50 to 30 if you want to stand still when removing weapon

        alt.setTimeout(() => {
            native.clearPedTasks(ped);
        }, 1700);
    }
});