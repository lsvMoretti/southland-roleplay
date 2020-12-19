import * as alt from 'alt-client';
import * as native from 'natives';

let LastAmmo: number;
let WeaponHash: any;

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

alt.everyTick(() => {
    let scriptId: number = alt.Player.local.scriptID;

    if (native.isPedArmed(scriptId, 6)) {
        native.disableControlAction(1, 140, true);
        native.disableControlAction(1, 141, true);
        native.disableControlAction(1, 142, true);
    }
});

alt.setInterval(() => {
    let player: number = alt.Player.local.scriptID;
    let fistHash: number = alt.hash('weapon_unarmed');
    let currentWeapon: [boolean, number] = native.getCurrentPedWeapon(player, fistHash, true);
    if (currentWeapon[0] == true) return;

    let currentAmmo: number = native.getAmmoInPedWeapon(alt.Player.local.scriptID, currentWeapon[1]);

    alt.emitServer('CurrentWeaponAmmo', currentAmmo);
}, 1000);