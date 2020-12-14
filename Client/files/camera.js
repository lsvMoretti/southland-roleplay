import * as alt from 'alt-client';
import * as native from 'natives';
var camera = null;
var interpolCam = null;
alt.onServer('interpolateCamera', interpolateCamera);
alt.onServer('destroyCamera', destroyCamera);
alt.onServer('createCamera', createCamera);
alt.onServer('CreateCameraAtEntity', createCameraAtEntity);
alt.onServer('dev:fetchGamePlayCamRot', () => {
    var rot = native.getGameplayCamRot(0);
    alt.emitServer('dev:returnGameplayCamRot', rot.z);
});
function interpolateCamera(pos1X, pos1Y, pos1Z, rot1, fov, pos2X, pos2Y, pos2Z, rot2, fov2, duration) {
    if (camera !== null || interpolCam !== null) {
        destroyCamera();
    }
    camera = native.createCamWithParams("DEFAULT_SCRIPTED_CAMERA", pos1X, pos1Y, pos1Z, 0, 0, rot1, fov, false, 0);
    interpolCam = native.createCamWithParams("DEFAULT_SCRIPTED_CAMERA", pos2X, pos2Y, pos2Z, 0, 0, rot2, fov2, false, 0);
    native.setCamActiveWithInterp(interpolCam, camera, duration, 1, 1);
    native.renderScriptCams(true, false, 0, false, false, null);
    native.setFocusPosAndVel(pos2X, pos2Y, pos2Z, 0.0, 0.0, 0.0);
    native.setHdArea(pos2X, pos2Y, pos2Z, 30);
    alt.log(camera);
    alt.log(interpolCam);
}
export function destroyCamera() {
    native.renderScriptCams(false, false, 0, true, false, null);
    native.destroyCam(camera, true);
    native.destroyCam(interpolCam, true);
    native.destroyAllCams(true);
    camera = null;
    interpolCam = null;
    native.setFollowPedCamViewMode(1);
    native.clearFocus();
}
export function createCamera(pos1X, pos1Y, pos1Z, rot1, fov) {
    if (camera !== null || interpolCam !== null) {
        destroyCamera();
    }
    camera = native.createCam("DEFAULT_SCRIPTED_CAMERA", true);
    native.setCamActive(camera, true);
    native.setCamCoord(camera, pos1X, pos1Y, pos1Z);
    native.renderScriptCams(true, false, 0, true, false, null);
    alt.log(rot1);
    native.setCamRot(camera, 0, 0, rot1, 2);
    native.setCamFov(camera, fov);
    native.setFocusPosAndVel(pos1X, pos1Y, pos1Z, 0.0, 0.0, 0.0);
    native.setHdArea(pos1X, pos1Y, pos1Z, 30);
}
function createCameraAtEntity(pos1X, pos1Y, pos1Z, rotZ, fov, entity) {
    if (camera !== null || interpolCam !== null) {
        destroyCamera();
    }
    camera = native.createCamWithParams("DEFAULT_SCRIPTED_CAMERA", pos1X, pos1Y, pos1Z, 0, 0, rotZ, fov, false, 2);
    native.setCamActive(camera, true);
    native.renderScriptCams(true, false, 0, false, false, null);
    native.pointCamAtEntity(camera, entity.scriptID, 0, 0, 0, true);
    native.setFocusPosAndVel(pos1X, pos1Y, pos1Z, 0.0, 0.0, 0.0);
    native.setHdArea(pos1X, pos1Y, pos1Z, 30);
}
export function createCameraPointToPos(pos1, fov1, pos2) {
    if (camera !== null || interpolCam !== null) {
        destroyCamera();
    }
    camera = native.createCam("DEFAULT_SCRIPTED_CAMERA", true);
    native.setCamActive(camera, true);
    native.setCamCoord(camera, pos1.x, pos1.y, pos1.z);
    native.renderScriptCams(true, false, 0, true, false, null);
    native.pointCamAtCoord(camera, pos2.x, pos2.y, pos2.z);
    native.setFocusPosAndVel(pos1.x, pos1.y, pos1.z, 0.0, 0.0, 0.0);
    native.setHdArea(pos1.x, pos1.y, pos1.z, 30);
    native.setCamFov(camera, fov1);
}
