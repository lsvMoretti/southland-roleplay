import * as alt from 'alt-client';
import * as native from 'natives';
var fuelBlip = undefined;
var atmBlip = undefined;
alt.everyTick(() => {
    var pos = alt.Player.local.pos;
    var fuel_01 = 1339433404;
    var fuel_02 = 1694452750;
    var fuel_03 = 1933174915;
    var fuel_04 = 2287735495;
    var fuel_05 = 3825272565;
    var fuel_06 = 4130089803;
    var fuelObject = native.getClosestObjectOfType(pos.x, pos.y, pos.z, 200, fuel_01, false, true, true);
    if (fuelObject == 0) {
        fuelObject = native.getClosestObjectOfType(pos.x, pos.y, pos.z, 200, fuel_02, false, true, true);
    }
    if (fuelObject == 0) {
        fuelObject = native.getClosestObjectOfType(pos.x, pos.y, pos.z, 200, fuel_03, false, true, true);
    }
    if (fuelObject == 0) {
        fuelObject = native.getClosestObjectOfType(pos.x, pos.y, pos.z, 200, fuel_04, false, true, true);
    }
    if (fuelObject == 0) {
        fuelObject = native.getClosestObjectOfType(pos.x, pos.y, pos.z, 200, fuel_05, false, true, true);
    }
    if (fuelObject == 0) {
        fuelObject = native.getClosestObjectOfType(pos.x, pos.y, pos.z, 200, fuel_06, false, true, true);
    }
    if (fuelObject > 0) {
        if (fuelBlip !== undefined) {
            native.removeBlip(fuelBlip);
            fuelBlip = undefined;
        }
        var fuelPos = native.getEntityCoords(fuelObject, false);
        fuelBlip = native.addBlipForCoord(fuelPos.x, fuelPos.y, fuelPos.z);
        native.setBlipSprite(fuelBlip, 361);
        native.setBlipColour(fuelBlip, 1);
        native.setBlipScale(fuelBlip, 0.5);
        native.beginTextCommandSetBlipName('STRING');
        native.addTextComponentSubstringPlayerName('Fuel Station');
        native.endTextCommandSetBlipName(fuelBlip);
    }
    if (fuelObject == 0 && fuelBlip != undefined) {
        native.removeBlip(fuelBlip);
        fuelBlip = undefined;
    }
    var atm_01 = 3424098598;
    var atm_02 = 3168729781;
    var atm_03 = 2930269768;
    var prop_flecca_atm = 506770882;
    var atmObject = native.getClosestObjectOfType(pos.x, pos.y, pos.z, 200, atm_01, false, true, true);
    if (atmObject == 0) {
        atmObject = native.getClosestObjectOfType(pos.x, pos.y, pos.z, 200, atm_02, false, true, true);
    }
    if (atmObject == 0) {
        atmObject = native.getClosestObjectOfType(pos.x, pos.y, pos.z, 200, atm_03, false, true, true);
    }
    if (atmObject == 0) {
        atmObject = native.getClosestObjectOfType(pos.x, pos.y, pos.z, 200, prop_flecca_atm, false, true, true);
    }
    if (atmObject > 0) {
        if (atmBlip !== undefined) {
            native.removeBlip(atmBlip);
            atmBlip = undefined;
        }
        var atmPos = native.getEntityCoords(atmObject, false);
        atmBlip = native.addBlipForCoord(atmPos.x, atmPos.y, atmPos.z);
        native.setBlipSprite(atmBlip, 207);
        native.setBlipColour(atmBlip, 1);
        native.setBlipScale(atmBlip, 0.5);
        native.beginTextCommandSetBlipName('STRING');
        native.addTextComponentSubstringPlayerName('Fuel Station');
        native.endTextCommandSetBlipName(atmBlip);
    }
    if (atmObject == 0 && atmBlip != undefined) {
        native.removeBlip(atmBlip);
        atmBlip = undefined;
    }
});
