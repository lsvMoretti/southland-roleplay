import * as alt from 'alt-client';

export function Distance(positionOne: alt.Vector3, positionTwo: alt.Vector3) {
    return Math.sqrt(Math.pow(positionOne.x - positionTwo.x, 2) + Math.pow(positionOne.y - positionTwo.y, 2) + Math.pow(positionOne.z - positionTwo.z, 2));
}
