import * as alt from 'alt-client';
import * as native from "natives";
import * as extensions from "../extensions";

alt.onServer('Clerk:StartJob', startClerkJob);
alt.onServer('Clerk:StopJob', stopClerkJob);
alt.onServer('Clerk:SetGotoPosition', setClerkPosition);

var fiveMinuteTimer: number;
var currentMarker: ClerkMarker = undefined;
var currentPos: alt.Vector3;

function startClerkJob() {
    fiveMinuteTimer = alt.setInterval(() => {
        alt.emitServer('Clerk:FiveMinute');
    }, 300000);
    // 300000
}

function stopClerkJob() {
    currentMarker = undefined;
    alt.clearInterval(fiveMinuteTimer);
}

function setClerkPosition(pos: alt.Vector3) {
    alt.log('clerkPos: ' + pos);
    currentPos = pos;
    currentMarker = new ClerkMarker(pos);
}

function atMarkerPosition() {
    currentMarker = undefined;
    alt.log('AtClerkPos');
    alt.emitServer("Clerk:ReachedPosition");
}

alt.everyTick(() => {
    if (currentMarker == undefined) return;

    var distance = extensions.Distance(alt.Player.local.pos, currentPos);

    currentMarker.draw();

    if (distance < 1) {
        atMarkerPosition();
    }
});

class ClerkMarker {
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

    constructor(pos: alt.Vector3) {
        this.Type = 1;
        this.PosX = pos.x;
        this.PosY = pos.y;
        this.PosZ = pos.z;
        this.DirX = 0;
        this.DirY = 0;
        this.DirZ = 0;
        this.RotX = 0;
        this.RotY = 0;
        this.RotZ = 0;
        this.Scale = 1;
        this.ColorR = 0;
        this.ColorG = 200;
        this.ColorB = 0;
        this.ColorA = 200;
        this.Bob = false;
        this.FaceCamera = false;
        this.Rotate = false;
    }
    draw() {
        native.drawMarker(
            this.Type,
            this.PosX,
            this.PosY,
            this.PosZ - 1,
            this.DirX,
            this.DirY,
            this.DirZ,
            this.RotX,
            this.RotY,
            this.RotZ,
            this.Scale,
            this.Scale,
            this.Scale + 1,
            this.ColorR,
            this.ColorG,
            this.ColorB,
            this.ColorA,
            this.Bob,
            this.FaceCamera,
            2,
            this.Rotate,
            null,
            null,
            false
        );
    }
}