import * as alt from 'alt-client';
import * as native from 'natives';
let LastAmmo;
let WeaponHash;
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
