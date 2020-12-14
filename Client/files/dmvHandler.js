import * as alt from 'alt-client';
import * as native from 'natives';
import * as extensions from 'files/extensions';
alt.onServer('startDrivingTest', startDrivingTest);
var checkpointList = undefined;
var currentPosition = -1;
var marker = undefined;
var speedCount = 0;
function startDrivingTest(checkpointJson) {
    checkpointList = JSON.parse(checkpointJson);
    alt.log(checkpointJson);
    currentPosition = 0;
    marker = new DmvMarker(currentPosition);
    native.setNewWaypoint(marker.pos.x, marker.pos.y);
}
alt.everyTick(() => {
    updatePosition();
    if (marker !== undefined) {
        marker.draw();
    }
});
alt.setInterval(() => {
    if (marker === undefined)
        return;
    if (alt.Player.local.vehicle !== null) {
        var meterPSec = alt.Player.local.vehicle.speed;
        var mph = Math.round(meterPSec * 2.236936);
        var allowedSpeed = 55;
        if (currentPosition >= 12 && currentPosition <= 14) {
            allowedSpeed = 155;
        }
        if (mph > allowedSpeed) {
            if (speedCount >= 5) {
                marker = undefined;
                speedCount = 0;
                alt.log('Speeding');
                alt.emitServer('dmv:finishedDriving', 0);
                return;
            }
            speedCount++;
            alt.emitServer('dmv:speeding', speedCount);
            alt.log('Watch your speed! You have ' + speedCount + " / 5 chances!");
        }
    }
}, 5000);
function updatePosition() {
    if (marker === undefined)
        return;
    let playerPos = alt.Player.local.pos;
    let playerDist = extensions.Distance(playerPos, marker.pos);
    if (playerDist < 5) {
        currentPosition++;
        if (currentPosition === 26) {
            marker = undefined;
            speedCount = 0;
            alt.log('Reached end of DMV test');
            alt.emitServer('dmv:finishedDriving', 1);
            return;
        }
        marker = new DmvMarker(currentPosition);
        native.setNewWaypoint(marker.pos.x, marker.pos.y);
    }
}
class DmvMarker {
    constructor(pos) {
        this.Type = 1;
        this.PosX = checkpointList[pos].X;
        this.PosY = checkpointList[pos].Y;
        this.PosZ = checkpointList[pos].Z - 1.5;
        this.DirX = 0;
        this.DirY = 0;
        this.DirZ = 0;
        this.RotX = 0;
        this.RotY = 0;
        this.RotZ = 0;
        this.Scale = 3;
        this.ColorR = 43;
        this.ColorG = 147;
        this.ColorB = 227;
        this.ColorA = 255;
        this.Bob = false;
        this.FaceCamera = false;
        this.Rotate = false;
        this.pos = new alt.Vector3(checkpointList[pos].X, checkpointList[pos].Y, checkpointList[pos].Z);
    }
    draw() {
        native.drawMarker(this.Type, this.PosX, this.PosY, this.PosZ, this.DirX, this.DirY, this.DirZ, this.RotX, this.RotY, this.RotZ, this.Scale, this.Scale, this.Scale, this.ColorR, this.ColorG, this.ColorB, this.ColorA, this.Bob, this.FaceCamera, 2, this.Rotate, undefined, undefined, false);
    }
}
