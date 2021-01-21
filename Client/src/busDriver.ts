import * as alt from 'alt-client';
import * as native from 'natives';
import * as extensions from './extensions';

var busStops: any = undefined;

var currentMarker: any = undefined;
var currentBusStop = undefined;
var busStopPos = undefined;
var nextStopId: any = undefined;

alt.onServer('bus:startJob', startBusJob);
alt.onServer('bus:endRoute', endBusRoute);

function startBusJob(stopJson: string) {
    busStops = JSON.parse(stopJson);

    nextStopId = 0;

    var currentBusStop = busStops[nextStopId];

    busStopPos = new alt.Vector3(currentBusStop.PosX, currentBusStop.PosY, currentBusStop.PosZ);

    currentMarker = new BusMarker(nextStopId);
}

alt.everyTick(() => {
    updatePosition();

    if (currentMarker !== undefined) {
        currentMarker.draw();
    }
});

function updatePosition() {
    if (currentMarker === undefined) return;

    const vehiclePos = alt.Player.local.vehicle.pos;
    let playerPos = alt.Player.local.pos;

    const vehicleDist = extensions.Distance(vehiclePos, currentMarker.pos);

    if (alt.Player.local.vehicle == undefined) return;

    if (alt.Player.local.vehicle.model != 0xD577C962) return;

    if (vehicleDist < 5) {
        var currentId = nextStopId;
        nextStopId++;
        if (nextStopId === busStops.length) {
            // At end
            currentMarker = undefined;
            alt.emitServer('bus:finishedRoute');
            return;
        }
        currentMarker = new BusMarker(nextStopId);
        native.setNewWaypoint(currentMarker.pos.x, currentMarker.pos.y);
        alt.emitServer('bus:EnteredMarker', currentId, busStops.length);
    }
}

function endBusRoute() {
    currentMarker = undefined;
}

class BusMarker {
    Type: number;
    PosX: number;
    PosY: number;
    PosZ: number;
    DirX: number;
    DirY: number;
    DirZ: number;
    RotX: number;
    RotY: number;
    RotZ: number;
    Scale: number;
    ColorR: number;
    ColorG: number;
    ColorB: number;
    ColorA: number;
    Bob: boolean;
    FaceCamera: boolean;
    Rotate: boolean;
    pos: alt.Vector3;

    constructor(pos: number) {
        this.Type = 1;
        this.PosX = busStops[pos].PosX;
        this.PosY = busStops[pos].PosY;
        this.PosZ = busStops[pos].PosZ - 1.5;
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
        this.pos = new alt.Vector3(busStops[pos].PosX, busStops[pos].PosY, busStops[pos].PosZ);
    }
    draw() {
        native.drawMarker(
            this.Type,
            this.PosX,
            this.PosY,
            this.PosZ,
            this.DirX,
            this.DirY,
            this.DirZ,
            this.RotX,
            this.RotY,
            this.RotZ,
            this.Scale,
            this.Scale,
            this.Scale,
            this.ColorR,
            this.ColorG,
            this.ColorB,
            this.ColorA,
            this.Bob,
            this.FaceCamera,
            2,
            this.Rotate,
            undefined,
            undefined,
            false
        );
    }
}