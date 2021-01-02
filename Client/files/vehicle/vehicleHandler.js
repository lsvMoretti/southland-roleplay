import * as alt from 'alt-client';
import * as native from 'natives';
var currentView = undefined;
var dealershipJson = undefined;
var vehicleScriptId = undefined;
var originalHeading = undefined;
var selectedVehicleIndex = undefined;
alt.onServer('showDealershipCars', ShowDealershipCars);
alt.onServer('closeCurrentPage', CloseCurrentPage);
alt.onServer('FetchDealershipCamRot', FetchDealershipCamRot);
alt.onServer('ShowPreviewScreen', ShowPreviewScreen);
alt.onServer('vehicle:fuel:isNearPump', () => {
    var pos = alt.Player.local.pos;
    var fuel_01 = 1339433404;
    var fuel_02 = 1694452750;
    var fuel_03 = 1933174915;
    var fuel_04 = 2287735495;
    var fuel_05 = 3825272565;
    var fuel_06 = 4130089803;
    var fuelObject = 0;
    fuelObject = native.getClosestObjectOfType(pos.x, pos.y, pos.z, 5, fuel_01, false, true, true);
    if (fuelObject === 0) {
        fuelObject = native.getClosestObjectOfType(pos.x, pos.y, pos.z, 5, fuel_02, false, true, true);
    }
    if (fuelObject === 0) {
        fuelObject = native.getClosestObjectOfType(pos.x, pos.y, pos.z, 5, fuel_03, false, true, true);
    }
    if (fuelObject === 0) {
        fuelObject = native.getClosestObjectOfType(pos.x, pos.y, pos.z, 5, fuel_04, false, true, true);
    }
    if (fuelObject === 0) {
        fuelObject = native.getClosestObjectOfType(pos.x, pos.y, pos.z, 5, fuel_05, false, true, true);
    }
    if (fuelObject === 0) {
        fuelObject = native.getClosestObjectOfType(pos.x, pos.y, pos.z, 5, fuel_06, false, true, true);
    }
    var isNearPump = fuelObject > 0;
    alt.log('Near Pump: ' + isNearPump);
    alt.emitServer('returnIsByFuelPump', isNearPump);
});
function ShowPreviewScreen(entity) {
    vehicleScriptId = entity.scriptID;
    if (currentView !== undefined) {
        CloseDealershipView();
    }
    originalHeading = 270;
    native.setEntityHeading(vehicleScriptId, originalHeading);
    currentView = new alt.WebView('http://resource/files/vehicle/previewVehicle.html', false);
    currentView.on('Dealership:ClosePage', CloseDealershipView);
    currentView.on('dealership:rotationchange', PreviewVehicleRotationChange);
    currentView.on('dealership:fetchPreviewVehicleInfo', FetchPreviewVehicleInfo);
    currentView.on('vehiclePreviewColorChange', vehiclePreviewColorChange);
    currentView.on('vehiclePreviewPurchaseVehicle', vehiclePreviewPurchaseVehicle);
    currentView.on('vehiclePreviewVoucherPurchase', vehiclePreviewVoucherPurchase);
    currentView.focus();
}
function vehiclePreviewVoucherPurchase() {
    CloseCurrentPage();
    alt.emitServer('dealership:voucherVehiclePurchase');
}
function vehiclePreviewPurchaseVehicle() {
    CloseCurrentPage();
    alt.emitServer('dealership:purchaseVehicle');
}
function vehiclePreviewColorChange(color) {
    alt.log(color);
    alt.emitServer('dealership:previewVehicleColorSelect', color);
}
function FetchPreviewVehicleInfo() {
    var dealershipVehicles = JSON.parse(dealershipJson);
    var selectedVehicle = dealershipVehicles[selectedVehicleIndex];
    currentView.emit('previewVehicleInfo', selectedVehicle.VehName, selectedVehicle.VehPrice, hasVoucher);
}
function FetchDealershipCamRot(dealerId) {
    alt.emitServer('dealership:returnCamRot', dealerId.toString(), native.getEntityHeading(alt.Player.local.scriptID).toString());
}
function CloseCurrentPage() {
    if (currentView === undefined)
        return;
    alt.showCursor(false);
    currentView.destroy();
    currentView = undefined;
}
var hasVoucher = false;
function ShowDealershipCars(json, voucher) {
    dealershipJson = json;
    hasVoucher = voucher;
    if (currentView !== undefined) {
        CloseDealershipView();
    }
    currentView = new alt.WebView('http://resource/files/vehicle/viewDealership.html', false);
    currentView.focus();
    currentView.on('viewDealershipLoaded', ViewDealershipLoaded);
    currentView.on('previewDealershipVehicle', PreviewDealershipVehicle);
    currentView.on('Dealership:ClosePage', CloseDealershipView);
    currentView.on('vehiclePreviewColorChange', vehiclePreviewColorChange);
}
function PreviewVehicleRotationChange(newValue) {
    var stringRotation = newValue.toString();
    alt.log(vehicleScriptId);
    if (stringRotation.startsWith("-")) {
        var rotation = stringRotation.slice(1);
        var numberRotation = Number(rotation);
        var minus = originalHeading - numberRotation;
        alt.log(minus);
        native.setEntityHeading(vehicleScriptId, Number(minus));
        return;
    }
    else {
        if (Number(newValue) > 90) {
            var calculated = 0 + (Number(newValue) - 90);
            alt.log(calculated);
            native.setEntityHeading(vehicleScriptId, Number(calculated));
            return;
        }
        var plus = originalHeading + Number(newValue);
        alt.log(plus);
        native.setEntityHeading(vehicleScriptId, Number(plus));
    }
}
function PreviewDealershipVehicle(index) {
    var vehicleList = JSON.parse(dealershipJson);
    var selectedVehicle = vehicleList[index];
    selectedVehicleIndex = index;
    alt.log(selectedVehicle);
    alt.emitServer('dealership:selectedVehiclePreview', index);
}
function ViewDealershipLoaded() {
    if (dealershipJson === undefined) {
        CloseDealershipView();
        return;
    }
    currentView.emit('loadDealershipVehicleInfo', dealershipJson);
}
function CloseDealershipView() {
    currentView.destroy();
    currentView = undefined;
    alt.emitServer('dealership:pageclosed');
}
alt.onServer('setWindowState', (vehicle, window, state) => {
    var scriptId = vehicle.scriptID;
    if (state === true) {
        native.rollDownWindow(scriptId, window);
        return;
    }
    if (state === false) {
        native.rollUpWindow(scriptId, window);
    }
});
alt.onServer('setDoorState', (vehicle, door, state) => {
    var scriptId = vehicle.scriptID;
    if (state === true) {
        native.setVehicleDoorOpen(scriptId, door, true, true);
        return;
    }
    if (state === false) {
        native.setVehicleDoorShut(scriptId, door, true);
        return;
    }
});
alt.onServer('FixVehicle', (vehicle) => {
    native.setVehicleFixed(vehicle.scriptID);
});
alt.onServer('EnterVehicle', (vehicle, driver) => {
    alt.emit('EnteredVehicle');
    if (driver) {
        native.taskEnterVehicle(alt.Player.local.scriptID, vehicle.scriptID, 3000, -1, 1.0, 1, 0);
        return;
    }
    for (var i = 0; i < 4; i++) {
        if (native.isVehicleSeatFree(vehicle.scriptID, i, false)) {
            native.taskEnterVehicle(alt.Player.local.scriptID, vehicle.scriptID, 3000, i, 1.0, 1, 0);
            return;
        }
    }
});
alt.onServer('ExitVehicle', (vehicle) => {
    native.taskLeaveVehicle(alt.Player.local.scriptID, vehicle.scriptID, 0);
});
alt.onServer('DisableSeatSwitch', (vehicle) => {
    native.setPedConfigFlag(alt.Player.local.scriptID, 184, true);
    native.setPedConfigFlag(alt.Player.local.scriptID, 429, true);
    native.setPedHelmet(alt.Player.local.scriptID, false);
});
alt.onServer('FetchVehicleClass', (vehicle) => {
    const vehicleClass = native.getVehicleClass(vehicle.scriptID);
    alt.emitServer('ReturnVehicleClass', vehicleClass);
});
alt.onServer('Vehicle:SetEngineStatus', (vehicle, engineStatus, instant) => {
    native.setVehicleEngineOn(vehicle.scriptID, engineStatus, instant, true);
});
alt.onServer('Vehicle:FetchTrunkPosition', (vehicle, returnEvent) => {
    var entityHandle = vehicle.scriptID;
    var pos = getEntityRearPosition(entityHandle);
    alt.log('Pos: ' + pos.y);
    alt.emitServer(returnEvent, pos.y);
});
function getEntityFrontPosition(entityHandle) {
    let modelDimensions = native.getModelDimensions(native.getEntityModel(entityHandle), undefined, undefined);
    return getOffsetPositionInWorldCoords(entityHandle, new alt.Vector3(0, modelDimensions[2].y, 0));
}
function getEntityRearPosition(entityHandle) {
    let modelDimensions = native.getModelDimensions(native.getEntityModel(entityHandle), undefined, undefined);
    return getOffsetPositionInWorldCoords(entityHandle, new alt.Vector3(0, modelDimensions[1].y, 0));
}
function getOffsetPositionInWorldCoords(entityHandle, offset) {
    return native.getOffsetFromEntityInWorldCoords(entityHandle, offset.x, offset.y, offset.z);
}
alt.onServer('PlayVehicleLockSound', (vehicle) => {
    var scriptId = vehicle.scriptID;
    native.playSoundFromEntity(-1, "UNLOCK_DOOR", scriptId, "LESTER1A_SOUNDS", false, false);
    alt.log('Playing lock sound');
});
alt.on('syncedMetaChange', (entity, key, value) => {
    if (entity instanceof alt.Vehicle) {
        if (key === 'LeftIndicatorState') {
            native.setVehicleIndicatorLights(entity.scriptID, 1, value);
        }
        if (key === 'RightIndicatorState') {
            native.setVehicleIndicatorLights(entity.scriptID, 0, value);
        }
    }
});
alt.onServer('EjectFromVehicle', (vehicle) => {
    native.taskLeaveVehicle(alt.Player.local.scriptID, vehicle.scriptID, 16);
});
alt.onServer('SetIntoVehicle', (vehicle, seat) => {
    alt.log('SetIntoVehicle: ' + vehicle.scriptID);
    native.taskEnterVehicle(alt.Player.local.scriptID, vehicle.scriptID, -1, seat, 2.0, 16, 0);
});
alt.onServer('SetBombBayDoorState', (vehicle, state) => {
    if (!state) {
        native.openBombBayDoors(vehicle.scriptID);
    }
    else {
        native.closeBombBayDoors(vehicle.scriptID);
    }
});
alt.onServer('SetVehicleExtra', (vehicle, slot, state) => {
    native.setVehicleExtra(vehicle.scriptID, slot, state);
});
alt.onServer('CleanVehicle', (vehicle) => {
    native.setVehicleDirtLevel(vehicle.scriptID, 0);
});
alt.everyTick(() => {
    var vehicleList = alt.Vehicle.all;
    if (vehicleList.length === 0)
        return;
    for (let vehicle of vehicleList) {
        if (native.getVehicleClass(vehicle.scriptID) != 14)
            continue;
        var anchorStatus = vehicle.getSyncedMeta("VehicleAnchorStatus");
        if (anchorStatus == undefined || anchorStatus == null)
            continue;
        native.setBoatAnchor(vehicle.scriptID, anchorStatus);
    }
    ;
});