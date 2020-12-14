import * as alt from 'alt-client';
import * as native from 'natives';
import * as cameraHandler from '../camera';
import { asyncModel } from './async-models';
import * as notification from '../noty/notyHandler';
const objectPreviewCameraPos = new alt.Vector3(1200.016, 4000.998, 86.05062);
const objectPreviewPos = new alt.Vector3(1200, 4000, 85);
let nextEntityId = 0;
let currentIndex = 0;
let lastObject = null;
let objectList = null;
let lastRotZ = 0;
alt.onServer('StartObjectPreview', startObjectPreview);
alt.onServer('ObjectPreview:ChangeIndex', onChangeIndex);
alt.onServer('ObjectPreview:Close', closePreview);
async function startObjectPreview(jsonList) {
    objectList = JSON.parse(jsonList);
    cameraHandler.createCameraPointToPos(objectPreviewCameraPos, 90, objectPreviewPos);
    currentIndex = 0;
    await showObject(0);
}
async function showObject(index) {
    if (lastObject != null) {
        native.setEntityVisible(lastObject, false, false);
        native.deleteEntity(lastObject);
        native.setEntityAsNoLongerNeeded(lastObject);
        native.deleteObject(lastObject);
        lastObject = null;
    }
    let objectInfo = objectList[index];
    let objectHash = alt.hash(objectInfo.ObjectName);
    let entityId = nextEntityId;
    nextEntityId++;
    if (!await asyncModel.load(entityId, objectHash)) {
        return alt.log(`[OBJECT-PREVIEW] Couldn't create model with ${objectHash} - ${objectInfo.ObjectName}.`);
    }
    lastObject = native.createObjectNoOffset(objectHash, objectPreviewPos.x, objectPreviewPos.y, objectPreviewPos.z, true, true, false);
}
alt.everyTick(() => {
    if (lastObject != null) {
        if (lastRotZ >= 360) {
            lastRotZ = 0;
        }
        lastRotZ += 2.5;
        let rotation = native.getEntityRotation(lastObject, 2);
        native.setEntityRotation(lastObject, rotation.x, rotation.y, lastRotZ, 2, true);
        return;
    }
    if (editObject != null) {
        let playerPos = alt.Player.local.pos;
        let playerRot = alt.Player.local.rot;
        let frontPos = PositionInFront(playerPos, playerRot, 0.5);
        native.setEntityCoordsNoOffset(editObject, frontPos.x + editPos.x, frontPos.y + editPos.y, frontPos.z + editPos.z, true, true, true);
        native.setEntityRotation(editObject, playerRot.x + editRot.x, playerRot.y + editRot.y, playerRot.z + editRot.z, 2, true);
    }
});
async function onChangeIndex(index) {
    await showObject(index);
}
function closePreview() {
    if (lastObject != null) {
        native.setEntityVisible(lastObject, false, false);
        native.deleteEntity(lastObject);
        native.setEntityAsNoLongerNeeded(lastObject);
        native.deleteObject(lastObject);
        lastObject = null;
    }
    cameraHandler.destroyCamera();
}
let editObject = null;
let editPos = null;
let editRot = null;
let placementType = 0;
alt.onServer('InteriorMapping:StartPlacement', startInteriorPlacement);
alt.onServer('InteriorMapping:StopPlacement', stopInteriorPlacement);
alt.onServer('InteriorMapping:MoveCurrentObject', moveCurrentInteriorObject);
async function startInteriorPlacement(objectName) {
    let objectHash = alt.hash(objectName);
    let playerPos = alt.Player.local.pos;
    let playerRot = alt.Player.local.rot;
    let frontPos = PositionInFront(playerPos, playerRot, 0.5);
    let entityId = nextEntityId;
    nextEntityId++;
    if (!await asyncModel.load(entityId, objectHash)) {
        return alt.log(`[OBJECT-PREVIEW] Couldn't create model with ${objectHash}.`);
    }
    editObject = native.createObjectNoOffset(objectHash, frontPos.x, frontPos.y, frontPos.z, true, true, false);
    native.setEntityRotation(editObject, playerRot.x, playerRot.y, playerRot.z, 2, true);
    editRot = new alt.Vector3(0, 0, 0);
    editPos = new alt.Vector3(0, 0, 0);
    editObjectStatus = true;
    placementType = 0;
}
async function moveCurrentInteriorObject(objectName, originalPosition) {
    let objectHash = alt.hash(objectName);
    let currentObject = native.getClosestObjectOfType(originalPosition.x, originalPosition.y, originalPosition.z, 0.5, objectHash, false, true, true);
    if (currentObject == null)
        return;
    editObject = currentObject;
    editRot = new alt.Vector3(0, 0, 0);
    editPos = new alt.Vector3(0, 0, 0);
    editObjectStatus = true;
    placementType = 1;
}
let editObjectStatus = false;
export function getEditObjectStatus() {
    return editObjectStatus;
}
function stopInteriorPlacement(returnEvent) {
    if (returnEvent) {
        let playerPos = alt.Player.local.pos;
        let playerRot = alt.Player.local.rot;
        let frontPos = PositionInFront(playerPos, playerRot, 0.5);
        let position = new alt.Vector3(frontPos.x + editPos.x, frontPos.y + editPos.y, frontPos.z + editPos.z);
        let rotation = new alt.Vector3(playerRot.x + editRot.x, playerRot.y + editRot.y, playerRot.z + editRot.z);
        alt.emitServer(returnEvent, position, rotation);
    }
    native.deleteEntity(editObject);
    native.setEntityAsNoLongerNeeded(editObject);
    native.setObjectAsNoLongerNeeded(editObject);
    native.deleteObject(editObject);
    native.setEntityVisible(editObject, false, false);
    editObject = null;
    editPos = null;
    editRot = null;
    editObjectStatus = false;
}
let movementType = false;
export function onKeyDownEvent(key) {
    alt.log(`Keydown: ${key}`);
    const value = 0.01;
    const rotValue = 5;
    if (key == 38) {
        if (!movementType) {
            editPos = new alt.Vector3(editPos.x + value, editPos.y, editPos.z);
        }
        if (movementType) {
            editRot = new alt.Vector3(editRot.x + rotValue, editRot.y, editRot.z);
        }
    }
    if (key == 40) {
        if (!movementType) {
            editPos = new alt.Vector3(editPos.x - value, editPos.y, editPos.z);
        }
        if (movementType) {
            editRot = new alt.Vector3(editRot.x - rotValue, editRot.y, editRot.z);
        }
    }
    if (key == 37) {
        if (!movementType) {
            editPos = new alt.Vector3(editPos.x, editPos.y + value, editPos.z);
        }
        if (movementType) {
            editRot = new alt.Vector3(editRot.x, editRot.y + rotValue, editRot.z);
        }
    }
    if (key == 39) {
        if (!movementType) {
            editPos = new alt.Vector3(editPos.x, editPos.y - value, editPos.z);
        }
        if (movementType) {
            editRot = new alt.Vector3(editRot.x, editRot.y - rotValue, editRot.z);
        }
    }
    if (key == 16) {
        if (!movementType) {
            editPos = new alt.Vector3(editPos.x, editPos.y, editPos.z + value);
        }
        if (movementType) {
            editRot = new alt.Vector3(editRot.x, editRot.y, editRot.z + rotValue);
        }
    }
    if (key == 17) {
        if (!movementType) {
            editPos = new alt.Vector3(editPos.x, editPos.y, editPos.z - value);
        }
        if (movementType) {
            editRot = new alt.Vector3(editRot.x, editRot.y, editRot.z - rotValue);
        }
    }
    if (key == 9) {
        movementType = !movementType;
        if (!movementType) {
            notification.SendNotification("Position Movement", 3000, 'alert', 'topCenter');
        }
        if (movementType) {
            notification.SendNotification("Rotation Movement", 3000, 'alert', 'topCenter');
        }
    }
    if (key == 112) {
        stopInteriorPlacement("");
    }
    if (key == 113) {
        if (placementType == 0) {
            stopInteriorPlacement("InteriorMapping:AddObject");
        }
        if (placementType == 1) {
            stopInteriorPlacement("InteriorMapping:MoveObject");
        }
    }
}
function ForwardVectorFromRotation(rotation) {
    let z = rotation.z * (Math.PI / 180.0);
    let x = rotation.x * (Math.PI / 180.0);
    let num = Math.abs(Math.cos(x));
    return new alt.Vector3(-Math.sin(z) * num, Math.cos(z) * num, Math.sin(x));
}
function PositionInFront(position, rotation, distance) {
    let forwardVector = ForwardVectorFromRotation(rotation);
    let scaledForwardVector = new alt.Vector3(forwardVector.x * distance, forwardVector.y * distance, forwardVector.z * distance);
    return new alt.Vector3(position.x + scaledForwardVector.x, position.y + scaledForwardVector.y, position.z + scaledForwardVector.z);
}
