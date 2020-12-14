import * as alt from 'alt-client';
import * as native from 'natives';

alt.onServer('vehicleMod:FetchVehicleMods', fetchVehicleModData);
alt.onServer('vehicleMod:FetchModNames', fetchVehicleModNames);
alt.onServer('modShop:fetchWheelTypes', fetchWheelTypes);

function fetchVehicleModData(json) {
    let player = alt.Player.local;
    let vehicle = player.vehicle.scriptID;
    let vMods = JSON.parse(json);

    alt.log('Vehicle Id: ' + alt.Player.local.vehicle.scriptID);

    vMods.Spoilers = native.getNumVehicleMods(alt.Player.local.vehicle.scriptID, 0);
    vMods.FBumper = native.getNumVehicleMods(alt.Player.local.vehicle.scriptID, 1);
    vMods.RBumper = native.getNumVehicleMods(alt.Player.local.vehicle.scriptID, 2);
    vMods.SSkirt = native.getNumVehicleMods(alt.Player.local.vehicle.scriptID, 3);
    vMods.Exhaust = native.getNumVehicleMods(alt.Player.local.vehicle.scriptID, 4);
    vMods.Frame = native.getNumVehicleMods(alt.Player.local.vehicle.scriptID, 5);
    vMods.Grille = native.getNumVehicleMods(alt.Player.local.vehicle.scriptID, 6);
    vMods.Hood = native.getNumVehicleMods(alt.Player.local.vehicle.scriptID, 7);
    vMods.Fender = native.getNumVehicleMods(alt.Player.local.vehicle.scriptID, 8);
    vMods.RFender = native.getNumVehicleMods(alt.Player.local.vehicle.scriptID, 9);
    vMods.Roof = native.getNumVehicleMods(alt.Player.local.vehicle.scriptID, 10);
    vMods.Engine = native.getNumVehicleMods(alt.Player.local.vehicle.scriptID, 11);
    vMods.Brakes = native.getNumVehicleMods(alt.Player.local.vehicle.scriptID, 12);
    vMods.Transmission = native.getNumVehicleMods(alt.Player.local.vehicle.scriptID, 13);
    vMods.Horns = native.getNumVehicleMods(alt.Player.local.vehicle.scriptID, 14);
    vMods.Suspension = native.getNumVehicleMods(alt.Player.local.vehicle.scriptID, 15);
    vMods.Turbo = native.getNumVehicleMods(alt.Player.local.vehicle.scriptID, 18);
    vMods.Xenon = native.getNumVehicleMods(alt.Player.local.vehicle.scriptID, 22);
    vMods.BWheels = native.getNumVehicleMods(alt.Player.local.vehicle.scriptID, 24);
    vMods.PlateHolders = native.getNumVehicleMods(alt.Player.local.vehicle.scriptID, 25);
    // Plate Vanity? - 26
    vMods.TrimDesign = native.getNumVehicleMods(alt.Player.local.vehicle.scriptID, 27);
    vMods.Ornaments = native.getNumVehicleMods(alt.Player.local.vehicle.scriptID, 28);
    vMods.DialDesign = native.getNumVehicleMods(alt.Player.local.vehicle.scriptID, 30);
    // Door Interior - 31
    // Seats - 32
    vMods.SteeringWheel = native.getNumVehicleMods(alt.Player.local.vehicle.scriptID, 33);
    vMods.ShiftLever = native.getNumVehicleMods(alt.Player.local.vehicle.scriptID, 34);
    vMods.Plaques = native.getNumVehicleMods(alt.Player.local.vehicle.scriptID, 35);
    // Rear Shelf 36
    // Trunk 37
    vMods.Hydraulics = native.getNumVehicleMods(alt.Player.local.vehicle.scriptID, 38);
    // Engine Block - 39
    // Air Filter - 40
    // Strut Bar - 41
    // Arch Cover - 42
    // Antenna - 43
    // Exterior parts - 44
    // Tank - 45
    // Door - 46
    // WHEELS_REAR_OR_HYDRAULICS - 47
    vMods.Livery = native.getNumVehicleMods(alt.Player.local.vehicle.scriptID, 48);
    vMods.Plate = native.getNumVehicleMods(alt.Player.local.vehicle.scriptID, 62);
    vMods.WTint = native.getNumVehicleMods(alt.Player.local.vehicle.scriptID, 69);
    vMods.DashColor = native.getNumVehicleMods(alt.Player.local.vehicle.scriptID, 74);
    vMods.TrimColor = native.getNumVehicleMods(alt.Player.local.vehicle.scriptID, 75);

    alt.emitServer('vehicleMod:ReturnVehicleMods', JSON.stringify(vMods));
}

function fetchVehicleModNames(modSlot) {
    var player = alt.Player.local;
    var vehicle = player.vehicle.scriptID;

    if (vehicle === null || vehicle === undefined) return;

    var modCount = native.getNumVehicleMods(vehicle, modSlot);

    var modNames = [];

    player.setMeta('vehicleMod:modSlotSelected', modSlot);

    if (modSlot === 69) {
        // Window Tint
        modNames.push("Pure Black");
        modNames.push("Dark Smoke");
        modNames.push("Light Smoke");
        modNames.push("Stock");
        modNames.push("Limo");
        modNames.push("Green");

        var windowJson = JSON.stringify(modNames);

        alt.emitServer('vehicleMod:returnModNames', windowJson, modSlot);
        return;
    }

    if (modSlot === 18) {
        // Turbo
        modNames.push("Turbo");

        var turboJson = JSON.stringify(modNames);

        alt.log(turboJson);

        alt.emitServer('vehicleMod:returnModNames', turboJson, modSlot);
        return;
    }

    if (modSlot === 11) {
        // Engine
        modNames.push("EMS-Improvement 1");
        modNames.push("EMS-Improvement 2");
        modNames.push("EMS-Improvement 3");
        modNames.push("EMS-Improvement 4");

        var engineJson = JSON.stringify(modNames);

        alt.emitServer('vehicleMod:returnModNames', engineJson, modSlot);
        return;
    }

    if (modSlot === 12) {
        // Brakes
        modNames.push("Street Brakes");
        modNames.push("Sport Brakes");
        modNames.push("Race Brakes");

        var brakesJson = JSON.stringify(modNames);

        alt.emitServer('vehicleMod:returnModNames', brakesJson, modSlot);
        return;
    }

    if (modSlot === 13) {
        // Transmission
        modNames.push("Street Transmission");
        modNames.push("Sport Transmission");
        modNames.push("Race Transmission");

        var transmissionJson = JSON.stringify(modNames);

        alt.emitServer('vehicleMod:returnModNames', transmissionJson, modSlot);
        return;
    }
    if (modSlot === 15) {
        // Suspension
        modNames.push("Lower Suspension");
        modNames.push("Street Suspension");
        modNames.push("Sport Suspension");
        modNames.push("Race Suspension");

        var suspensionJson = JSON.stringify(modNames);

        alt.emitServer('vehicleMod:returnModNames', suspensionJson, modSlot);
        return;
    }
    if (modSlot === 22) {
        // Xenon
        modNames.push("Standard Headlights");
        modNames.push("Xenon Headlights");

        var xenonJson = JSON.stringify(modNames);

        alt.emitServer('vehicleMod:returnModNames', xenonJson, modSlot);
        return;
    }

    for (var i = 0; i < modCount; i++) {
        alt.log('Mod Slot: ' + modSlot);
        alt.log('Mod Count: ' + modCount);

        var modLabel = native.getModTextLabel(vehicle, modSlot, i);

        alt.log('Mod Label: ' + modLabel);
        var modName = native.getLabelText(modLabel);
        alt.log('Mod Name: ' + modName);

        modNames.push(modName);
    }

    var json = JSON.stringify(modNames);

    if (modSlot === 23) {
        // Front Wheels
        alt.emitServer('vehicleMod:returnVehicleWheelNames', json);
        return;
    }

    alt.emitServer('vehicleMod:returnModNames', json, modSlot);
}

function fetchWheelTypes(type) {
    var player = alt.Player.local;
    var vehicle = player.vehicle.scriptID;

    if (vehicle === null || vehicle === undefined) return;

    native.setVehicleWheelType(vehicle, type);

    var count = native.getNumVehicleMods(vehicle, 23);

    fetchVehicleModNames(23);
}