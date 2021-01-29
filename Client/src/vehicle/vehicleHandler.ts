import * as alt from 'alt-client';
import * as native from 'natives';
import * as keyHandler from "../keyHandler";

var vehicleScrambleWebView: alt.WebView = undefined;
var scrambleText: string = undefined;
let opened: boolean = false;

export function IsScrambleOpen() {
    return opened;
}

alt.onServer('Vehicle:RemoveFromVehicle', () => {
    native.taskLeaveVehicle(alt.Player.local.scriptID, alt.Player.local.vehicle.scriptID, 16);
});

alt.onServer('VehicleScramble:LoadPage', (word: string, jumbleWord: string, time: number, attempts: number) => {
    if (vehicleScrambleWebView != undefined) {
        alt.setTimeout(() => {
            vehicleScrambleWebView.destroy();
        },
            1000);
    }

    alt.showCursor(true);

    vehicleScrambleWebView = new alt.WebView('http://resource/files/vehicle/vehicleScramble.html');
    vehicleScrambleWebView.focus();
    opened = true;
    vehicleScrambleWebView.on('VehicleScrambleLoaded',
        () => {
            vehicleScrambleWebView.emit('ReceiveInfo', word, jumbleWord, time, attempts);
        });

    vehicleScrambleWebView.on('vehicleScrambleClosePage',
        () => {
            alt.setTimeout(() => {
                vehicleScrambleWebView.destroy();
                vehicleScrambleWebView = undefined;
                opened = false;
                alt.showCursor(false);
                alt.emitServer('VehicleScramble:PageClosed');
            }, 1000);
        });

    vehicleScrambleWebView.on('VehicleScramble:MaxAttemptsReached',
        () => {
            alt.emitServer('VehicleScramble:MaxAttemptsReached');
        });

    vehicleScrambleWebView.on('VehicleScramble:TimeExpired',
        () => {
            alt.emitServer('VehicleScramble:TimeExpired');
        });

    vehicleScrambleWebView.on('VehicleScramble:CorrectWord',
        () => {
            alt.emitServer('VehicleScramble:CorrectWord');
        });
});

alt.onServer('VehicleScramble:ClosePage', () => {
    if (vehicleScrambleWebView != undefined) {
        alt.showCursor(false);
        opened = false;
        alt.setTimeout(() => {
            vehicleScrambleWebView.destroy();
            vehicleScrambleWebView = undefined;
        },
            1000);
    }
});

var currentView: alt.WebView = undefined;
var dealershipJson: string = undefined;
var vehicleScriptId: any = undefined;
var originalHeading: any = undefined;
var selectedVehicleIndex: any = undefined;

alt.onServer('showDealershipCars', ShowDealershipCars);
alt.onServer('closeCurrentPage', CloseCurrentPage);
alt.onServer('FetchDealershipCamRot', FetchDealershipCamRot);
alt.onServer('ShowPreviewScreen', ShowPreviewScreen);

alt.onServer('vehicle:fuel:isNearPump',
    () => {
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
        } if (fuelObject === 0) {
            fuelObject = native.getClosestObjectOfType(pos.x, pos.y, pos.z, 5, fuel_05, false, true, true);
        } if (fuelObject === 0) {
            fuelObject = native.getClosestObjectOfType(pos.x, pos.y, pos.z, 5, fuel_06, false, true, true);
        }

        var isNearPump = fuelObject > 0;

        alt.log('Near Pump: ' + isNearPump);

        alt.emitServer('returnIsByFuelPump', isNearPump);
    });

function ShowPreviewScreen(entity: any) {
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

function vehiclePreviewColorChange(color: any) {
    alt.log(color);
    alt.emitServer('dealership:previewVehicleColorSelect', color);
}

function FetchPreviewVehicleInfo() {
    var dealershipVehicles = JSON.parse(dealershipJson);
    var selectedVehicle = dealershipVehicles[selectedVehicleIndex];

    currentView.emit('previewVehicleInfo', selectedVehicle.VehName, selectedVehicle.VehPrice, hasVoucher);
}

function FetchDealershipCamRot(dealerId: any) {
    alt.emitServer('dealership:returnCamRot', dealerId.toString(), native.getEntityHeading(alt.Player.local.scriptID).toString());
}

function CloseCurrentPage() {
    if (currentView === undefined) return;

    alt.showCursor(false);
    alt.setTimeout(() => {
        currentView.destroy();
        currentView = undefined;
    },
        1000);
}

var hasVoucher = false;

function ShowDealershipCars(json: any, voucher: any) {
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

function PreviewVehicleRotationChange(newValue: any) {
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

function PreviewDealershipVehicle(index: any) {
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
    alt.setTimeout(() => {
        currentView.destroy();
        currentView = undefined;
    },
        1000);
    alt.emitServer('dealership:pageclosed');
}

alt.onServer('setWindowState', (vehicle: any, window: any, state: any) => {
    var scriptId = vehicle.scriptID;

    if (state === true) {
        native.rollDownWindow(scriptId, window);
        return;
    }

    if (state === false) {
        native.rollUpWindow(scriptId, window);
    }
});

alt.onServer('setDoorState', (vehicle: any, door: any, state: any) => {
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

alt.onServer('FixVehicle', (vehicle: any) => {
    native.setVehicleFixed(vehicle.scriptID);
});

alt.onServer('EnterVehicle', (vehicle: any, driver: any) => {
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

alt.onServer('ExitVehicle', (vehicle: any) => {
    native.taskLeaveVehicle(alt.Player.local.scriptID, vehicle.scriptID, 0);
});

alt.onServer('DisableSeatSwitch', (vehicle: any) => {
    native.setPedConfigFlag(alt.Player.local.scriptID, 184, true);
    native.setPedConfigFlag(alt.Player.local.scriptID, 429, true); // Engine
    native.setPedHelmet(alt.Player.local.scriptID, false); // Disables ped helmet
});

alt.onServer('FetchVehicleClass', (vehicle: any) => {
    const vehicleClass = native.getVehicleClass(vehicle.scriptID);

    alt.emitServer('ReturnVehicleClass', vehicleClass);
});

alt.onServer('Vehicle:SetEngineStatus', (vehicle: any, engineStatus: any, instant: any) => {
    native.setVehicleEngineOn(vehicle.scriptID, engineStatus, instant, true);
});

alt.onServer('Vehicle:FetchTrunkPosition', (vehicle: any, returnEvent: any) => {
    var entityHandle = vehicle.scriptID;

    var pos = getEntityRearPosition(entityHandle);

    alt.log('Pos: ' + pos.y);
    alt.emitServer(returnEvent, pos.y);
});

function getEntityFrontPosition(entityHandle: any) {
    let modelDimensions = native.getModelDimensions(native.getEntityModel(entityHandle), undefined, undefined);
    return getOffsetPositionInWorldCoords(entityHandle, new alt.Vector3(0, modelDimensions[2].y, 0));
}

function getEntityRearPosition(entityHandle: any) {
    let modelDimensions = native.getModelDimensions(native.getEntityModel(entityHandle), undefined, undefined);
    return getOffsetPositionInWorldCoords(entityHandle, new alt.Vector3(0, modelDimensions[1].y, 0));
}

function getOffsetPositionInWorldCoords(entityHandle: any, offset: any) {
    return native.getOffsetFromEntityInWorldCoords(entityHandle, offset.x, offset.y, offset.z);
}

alt.onServer('PlayVehicleLockSound', (vehicle: alt.Entity) => {
    var scriptId = vehicle.scriptID;

    native.playSoundFromEntity(-1, "UNLOCK_DOOR", scriptId, "LESTER1A_SOUNDS", false, false);

    alt.log('Playing lock sound');
});

alt.on('syncedMetaChange', (entity: alt.Entity, key: string, value: any) => {
    if (entity instanceof alt.Vehicle) {
        if (key === 'LeftIndicatorState') {
            native.setVehicleIndicatorLights(entity.scriptID, 1, value);
        }
        if (key === 'RightIndicatorState') {
            native.setVehicleIndicatorLights(entity.scriptID, 0, value);
        }
    }
});

alt.onServer('EjectFromVehicle', (vehicle: alt.Vehicle) => {
    native.taskLeaveVehicle(alt.Player.local.scriptID, vehicle.scriptID, 16);
});

alt.onServer('SetIntoVehicle', (vehicle: alt.Vehicle, seat: number) => {
    alt.log('SetIntoVehicle: ' + vehicle.scriptID);

    native.taskEnterVehicle(alt.Player.local.scriptID, vehicle.scriptID, -1, seat, 2.0, 16, 0);
});

alt.onServer('SetBombBayDoorState', (vehicle: alt.Vehicle, state: boolean) => {
    if (!state) {
        native.openBombBayDoors(vehicle.scriptID);
    }
    else {
        native.closeBombBayDoors(vehicle.scriptID);
    }
});

alt.onServer('SetVehicleExtra', (vehicle: alt.Vehicle, slot: number, state: boolean) => {
    native.setVehicleExtra(vehicle.scriptID, slot, state);
});

alt.onServer('CleanVehicle', (vehicle: alt.Vehicle) => {
    native.setVehicleDirtLevel(vehicle.scriptID, 0);
});

export function startIntervals() {
    alt.setInterval(anchorInterval, 0);
}

function anchorInterval() {
    var vehicleList = alt.Vehicle.all;

    if (vehicleList.length === 0) return;

    for (let vehicle of vehicleList) {
        if (!vehicle.valid) continue;
        if (native.getVehicleClass(vehicle.scriptID) != 14) continue;

        var anchorStatus: boolean = vehicle.getSyncedMeta("VehicleAnchorStatus");

        if (anchorStatus == undefined || anchorStatus == null) continue;

        native.setBoatAnchor(vehicle.scriptID, anchorStatus);
    };
}

alt.everyTick(() => {
});