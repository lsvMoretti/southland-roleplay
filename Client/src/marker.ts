import * as alt from 'alt-client';
import * as native from 'natives';
import * as extensions from './extensions';

var markerJsonArray = new Array();
var markerArray = new Array();
alt.onServer('createMarker', createNewMarker);
alt.onServer('deleteMarker', deleteMarker);

alt.onServer('deleteAllMarkers', () => {
    markerJsonArray = new Array();
    markerArray = new Array();
});
function createNewMarker(markerJson: string) {
    markerJsonArray.push(markerJson);
    var marker = JSON.parse(markerJson);
    marker.pos = new alt.Vector3(marker.PosX, marker.PosY, marker.PosZ);
    markerArray.push(marker);
}
function displayMarker(marker: any) {
    let playerPos = alt.Player.local.pos;
    let playerDist = extensions.Distance(playerPos, marker.pos);
    if (playerDist < marker.Range) {
        native.drawMarker(marker.Type, marker.PosX, marker.PosY, marker.PosZ, marker.DirX, marker.DirY, marker.DirZ, marker.RotX, marker.RotY, marker.RotZ, marker.Scale, marker.Scale, marker.Scale, marker.Color.R, marker.Color.G, marker.Color.B, marker.Color.A, marker.Bob, marker.FaceCamera, 2, marker.Rotate, undefined, undefined, false);
    }
}
function deleteMarker(markerJson: string) {
    for (let index = 0; index < markerJsonArray.length; index++) {
        const element = markerJsonArray[index];
        if (element === markerJson) {
            markerJsonArray.splice(index, 1);
        }
    }
    for (let index = 0; index < markerArray.length; index++) {
        const element = markerArray[index];
        if (element === JSON.parse(markerJson)) {
            markerArray.splice(index, 1);
        }
    }
}
function processMarkers() {
    if (markerArray.length == 0)
        return;
    markerArray.forEach(element => {
        displayMarker(element);
    });
}
alt.setInterval(() => {
    processMarkers();
}, 0);
