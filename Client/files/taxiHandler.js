import * as alt from 'alt-client';
import * as native from 'natives';
alt.onServer('setTaxiLight', setTaxiLight);
function setTaxiLight(status) {
    if (alt.Player.local.vehicle === undefined)
        return;
    var vehId = alt.Player.local.vehicle.scriptID;
    native.setTaxiLights(vehId, status);
}
