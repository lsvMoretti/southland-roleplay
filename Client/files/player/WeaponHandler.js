import * as alt from 'alt-client';
import * as native from 'natives';
let LastAmmo;
let WeaponHash;
native.requestAnimDict("rcmjosh4");
native.requestAnimDict("reaction@intimidation@cop@unarmed");
native.requestAnimDict("reaction@intimidation@1h");
alt.onServer('fetchCurrentAmmo', (returnEvent, weaponHash) => {
    let ammo = native.getAmmoInPedWeapon(alt.Player.local.scriptID, weaponHash);
    alt.log('Current Ammo: ' + ammo);
    alt.emitServer(returnEvent, ammo);
});
alt.onServer('WeaponEquipped', (weaponHash) => {
    native.setPedDropsWeaponsWhenDead(alt.Player.local.scriptID, false);
    alt.log('Weapon Hash: ' + weaponHash);
    WeaponHash = weaponHash;
    LastAmmo = native.getAmmoInPedWeapon(alt.Player.local.scriptID, weaponHash);
});
alt.everyTick(() => {
    let scriptId = alt.Player.local.scriptID;
    if (native.isPedArmed(scriptId, 6)) {
        native.disableControlAction(1, 140, true);
        native.disableControlAction(1, 141, true);
        native.disableControlAction(1, 142, true);
    }
});
alt.setInterval(() => {
    let player = alt.Player.local.scriptID;
    let fistHash = alt.hash('weapon_unarmed');
    let currentWeapon = native.getCurrentPedWeapon(player, fistHash, true);
    if (currentWeapon[0] == true)
        return;
    let currentAmmo = native.getAmmoInPedWeapon(alt.Player.local.scriptID, currentWeapon[1]);
    alt.emitServer('CurrentWeaponAmmo', currentAmmo);
}, 1000);
alt.onServer('WeaponSwitchAnim', (type) => {
    const ped = alt.Player.local.scriptID;
    native.clearPedTasks(ped);
    if (type === 0) {
        native.setPedCurrentWeaponVisible(ped, false, true, true, true);
        native.taskPlayAnim(ped, "reaction@intimidation@cop@unarmed", "intro", 8.0, 2.0, -1, 50, 2.0, false, false, false);
        alt.setTimeout(() => {
            native.setPedCurrentWeaponVisible(ped, true, true, true, true);
            native.taskPlayAnim(ped, "rcmjosh4", "josh_leadout_cop2", 8.0, 2.0, -1, 48, 10, false, false, false);
            alt.setTimeout(() => {
                native.clearPedTasks(ped);
            }, 400);
        }, 700);
    }
    if (type === 1) {
        native.taskPlayAnim(ped, "rcmjosh4", "josh_leadout_cop2", 8.0, 2.0, -1, 48, 10, false, false, false);
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
        native.setPedCurrentWeaponVisible(ped, false, true, true, true);
        native.taskPlayAnim(ped, "reaction@intimidation@1h", "outro", 8.0, 3.0, -1, 50, 0.125, false, false, false);
        alt.setTimeout(() => {
            native.setPedCurrentWeaponVisible(ped, true, true, true, true);
            alt.setTimeout(() => {
                native.clearPedTasks(ped);
            }, 60);
        }, 700);
    }
    if (type === 3) {
        native.taskPlayAnim(ped, "reaction@intimidation@1h", "outro", 8.0, 3.0, -1, 50, 0.125, false, false, false);
        alt.setTimeout(() => {
            native.clearPedTasks(ped);
        }, 1700);
    }
});
