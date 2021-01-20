import * as alt from 'alt-client';
import * as native from 'natives';
let fuelBlip = undefined;
let atmBlip = undefined;
alt.setInterval(() => {
    const pos = alt.Player.local.pos;
    const fuel01 = 1339433404;
    const fuel02 = 1694452750;
    const fuel03 = 1933174915;
    const fuel04 = 2287735495;
    const fuel05 = 3825272565;
    const fuel06 = 4130089803;
    let fuelObject = native.getClosestObjectOfType(pos.x, pos.y, pos.z, 200, fuel01, false, true, true);
    if (fuelObject === 0) {
        fuelObject = native.getClosestObjectOfType(pos.x, pos.y, pos.z, 200, fuel02, false, true, true);
    }
    if (fuelObject === 0) {
        fuelObject = native.getClosestObjectOfType(pos.x, pos.y, pos.z, 200, fuel03, false, true, true);
    }
    if (fuelObject === 0) {
        fuelObject = native.getClosestObjectOfType(pos.x, pos.y, pos.z, 200, fuel04, false, true, true);
    }
    if (fuelObject === 0) {
        fuelObject = native.getClosestObjectOfType(pos.x, pos.y, pos.z, 200, fuel05, false, true, true);
    }
    if (fuelObject === 0) {
        fuelObject = native.getClosestObjectOfType(pos.x, pos.y, pos.z, 200, fuel06, false, true, true);
    }
    if (fuelObject > 0) {
        if (fuelBlip !== undefined) {
            native.removeBlip(fuelBlip);
            fuelBlip = undefined;
        }
        const fuelPos = native.getEntityCoords(fuelObject, false);
        fuelBlip = native.addBlipForCoord(fuelPos.x, fuelPos.y, fuelPos.z);
        native.setBlipSprite(fuelBlip, 361);
        native.setBlipColour(fuelBlip, 1);
        native.setBlipScale(fuelBlip, 0.5);
        native.beginTextCommandSetBlipName('STRING');
        native.addTextComponentSubstringPlayerName('Fuel Station');
        native.endTextCommandSetBlipName(fuelBlip);
    }
    if (fuelObject === 0 && fuelBlip !== undefined) {
        native.removeBlip(fuelBlip);
        fuelBlip = undefined;
    }
    const atm01 = 3424098598;
    const atm02 = 3168729781;
    const atm03 = 2930269768;
    const propFleccaAtm = 506770882;
    let atmObject = native.getClosestObjectOfType(pos.x, pos.y, pos.z, 200, atm01, false, true, true);
    if (atmObject === 0) {
        atmObject = native.getClosestObjectOfType(pos.x, pos.y, pos.z, 200, atm02, false, true, true);
    }
    if (atmObject === 0) {
        atmObject = native.getClosestObjectOfType(pos.x, pos.y, pos.z, 200, atm03, false, true, true);
    }
    if (atmObject === 0) {
        atmObject = native.getClosestObjectOfType(pos.x, pos.y, pos.z, 200, propFleccaAtm, false, true, true);
    }
    if (atmObject > 0) {
        if (atmBlip !== undefined) {
            native.removeBlip(atmBlip);
            atmBlip = undefined;
        }
        const atmPos = native.getEntityCoords(atmObject, false);
        atmBlip = native.addBlipForCoord(atmPos.x, atmPos.y, atmPos.z);
        native.setBlipSprite(atmBlip, 207);
        native.setBlipColour(atmBlip, 1);
        native.setBlipScale(atmBlip, 0.5);
        native.beginTextCommandSetBlipName('STRING');
        native.addTextComponentSubstringPlayerName('ATM');
        native.endTextCommandSetBlipName(atmBlip);
    }
    if (atmObject === 0 && atmBlip !== undefined) {
        native.removeBlip(atmBlip);
        atmBlip = undefined;
    }
}, 0);
