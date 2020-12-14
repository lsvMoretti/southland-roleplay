import * as alt from 'alt-client';
import * as native from 'natives';
let doorList = undefined;
alt.onServer('receiveDoorList', (doors) => {
    doorList = doors;
});
let doorArray = [];
alt.setInterval(() => {
    if (doorList == undefined)
        return;
    updateDoorStatus();
}, 1000);
function updateDoorStatus() {
    doorList.forEach((door) => {
        let entity = native.getClosestObjectOfType(door.PosX, door.PosY, door.PosZ, 1, Number(door.Model), false, false, false);
        if (doorArray[entity] != null || doorArray != undefined) {
            if (doorArray[entity] != door.Locked) {
                doorArray[entity] = door.Locked;
                native.freezeEntityPosition(entity, door.Locked);
            }
        }
        else {
            doorArray[entity] = door.Locked;
            native.freezeEntityPosition(entity, door.Locked);
        }
    });
}
alt.onServer('getClosestDoor', () => {
    let position = alt.Player.local.pos;
    let forwardVector = native.getEntityForwardVector(alt.Player.local.scriptID);
    let frontPos = new alt.Vector3(position.x + forwardVector.x * 3, position.y + forwardVector.y * 3, position.z + forwardVector.z * 3);
    let testRay = native.startShapeTestRay(position.x, position.y, position.z, frontPos.x, frontPos.y, frontPos.z, 16, alt.Player.local.scriptID, undefined);
    let result = native.getShapeTestResult(testRay, undefined, undefined, undefined, undefined);
    alt.log('Result: ' + result[0] + ',' + result[1] + ',' + result[2] + ',' + result[3] + ',' + result[4] + ',');
    let entityPos = native.getEntityCoords(result[4], false);
    alt.log('entityPos: ' + entityPos.x + ',' + entityPos.y + ',' + entityPos.z);
    let entityModel = native.getEntityModel(result[4]);
    alt.log('entityModel: ' + entityModel);
    alt.emitServer('sendClosestDoor', entityModel.toString(), entityPos.x, entityPos.y, entityPos.z);
});
alt.onServer('SetDoorStatus', (model, posX, posY, posZ, locked) => {
    let entity = native.getClosestObjectOfType(posX, posY, posZ, 1, Number(model), false, false, false);
    native.freezeEntityPosition(entity, locked);
});
