import * as alt from 'alt-client';
import * as native from 'natives';
var LastAmmo;
var WeaponHash;
alt.onServer('fetchCurrentAmmo', (returnEvent, weaponHash) => {
    var ammo = native.getAmmoInPedWeapon(alt.Player.local.scriptID, weaponHash);
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
    var scriptId = alt.Player.local.scriptID;
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
