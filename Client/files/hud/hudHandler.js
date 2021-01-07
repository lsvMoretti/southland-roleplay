import * as alt from 'alt-client';
import * as native from 'natives';
var hudWebView = undefined;
var localPlayer = undefined;
alt.everyTick(() => {
    if (native.isHudComponentActive(14)) {
        native.hideHudComponentThisFrame(14);
    }
    if (HudState) {
        native.hideHudComponentThisFrame(9);
        native.hideHudComponentThisFrame(7);
    }
    if (HudState === false) {
        native.hideHudAndRadarThisFrame();
    }
});
var HudState = false;
var allowHud = false;
alt.onServer('hud:SetState', (input) => {
    HudState = input;
});
alt.onServer('hud:CharacterLoaded', () => {
    allowHud = true;
    HudState = true;
});
alt.setInterval(() => {
    if (allowHud) {
        if (HudState && hudWebView === undefined) {
            ShowHud();
            return;
        }
        if (!HudState && hudWebView !== undefined) {
            HideHud();
        }
    }
}, 500);
function ShowHud() {
    if (hudWebView !== undefined) {
        HideHud();
    }
    localPlayer = alt.Player.local.scriptID;
    hudWebView = new alt.WebView('http://resource/files/hud/hud.html', false);
    hudWebView.on('FetchHudData', FetchHudData);
}
function FetchHudData() {
    alt.emitServer('hud:FetchMoney');
}
alt.onServer('hud:RecieveMoneyUpdate', RecieveMoneyUpdate);
function RecieveMoneyUpdate(hudJson) {
    if (hudWebView === undefined)
        return;
    localPlayer = alt.Player.local.scriptID;
    var hudInfo = JSON.parse(hudJson);
    var streetNameValue = 0;
    var crossingPoint = 0;
    var pos = alt.Player.local.pos;
    var zoneId = native.getNameOfZone(pos.x, pos.y, pos.z);
    var streetResult = native.getStreetNameAtCoord(pos.x, pos.y, pos.z, streetNameValue, crossingPoint);
    var streetName = native.getStreetNameFromHashKey(streetResult[1]);
    var zoneNameShort = ["AIRP", "ALAMO", "ALTA", "ARMYB", "BANHAMC", "BANNING", "BEACH", "BHAMCA", "BRADP", "BRADT", "BURTON", "CALAFB", "CANNY", "CCREAK", "CHAMH", "CHIL", "CHU", "CMSW", "CYPRE", "DAVIS", "DELBE", "DELPE", "DELSOL", "DESRT", "DOWNT", "DTVINE", "EAST_V", "EBURO", "ELGORL", "ELYSIAN", "GALFISH", "GOLF", "GRAPES", "GREATC", "HARMO", "HAWICK", "HORS", "HUMLAB", "JAIL", "KOREAT", "LACT", "LAGO", "LDAM", "LEGSQU", "LMESA", "LOSPUER", "MIRR", "MORN", "MOVIE", "MTCHIL", "MTGORDO", "MTJOSE", "MURRI", "NCHU", "NOOSE", "OCEANA", "PALCOV", "PALETO", "PALFOR", "PALHIGH", "PALMPOW", "PBLUFF", "PBOX", "PROCOB", "RANCHO", "RGLEN", "RICHM", "ROCKF", "RTRAK", "SANAND", "SANCHIA", "SANDY", "SKID", "SLAB", "STAD", "STRAW", "TATAMO", "TERMINA", "TEXTI", "TONGVAH", "TONGVAV", "VCANA", "VESP", "VINE", "WINDF", "WVINE", "ZANCUDO", "ZP_ORT", "ZQ_UAR"];
    var zoneListComp = ["Los Santos International Airport", "Alamo Sea", "Alta", "Fort Zancudo", "Banham Canyon Dr", "Banning", "Vespucci Beach", "Banham Canyon", "Braddock Pass", "Braddock Tunnel", "Burton", "Calafia Bridge", "Raton Canyon", "Cassidy Creek", "Chamberlain Hills", "Vinewood Hills", "Chumash", "Chiliad Mountain State Wilderness", "Cypress Flats", "Davis", "Del Perro Beach", "Del Perro", "La Puerta", "Grand Senora Desert", "Downtown", "Downtown Vinewood", "East Vinewood", "El Burro Heights", "El Gordo Lighthouse", "Elysian Island", "Galilee", "GWC and Golfing Society", "Grapeseed", "Great Chaparral", "Harmony", "Hawick", "Vinewood Racetrack", "Humane Labs and Research", "Bolingbroke Penitentiary", "Little Seoul", "Land Act Reservoir", "Lago Zancudo", "Land Act Dam", "Legion Square", "La Mesa", "La Puerta", "Mirror Park", "Morningwood", "Richards Majestic", "Mount Chiliad", "Mount Gordo", "Mount Josiah", "Murrieta Heights", "North Chumash", "N.O.O.S.E", "Pacific Ocean", "Paleto Cove", "Paleto Bay", "Paleto Forest", "Palomino Highlands", "Palmer - Taylor Power Station", "Pacific Bluffs", "Pillbox Hill", "Procopio Beach", "Rancho", "Richman Glen", "Richman", "Rockford Hills", "Redwood Lights Track", "San Andreas", "San Chianski Mountain Range", "Sandy Shores", "Mission Row", "Stab City", "Maze Bank Arena", "Strawberry", "Tataviam Mountains", "Terminal", "Textile City", "Tongva Hills", "Tongva Valley", "Vespucci Canals", "Vespucci", "Vinewood", "Ron Alternates Wind Farm", "West Vinewood", "Zancudo River", "Port of South Los Santos", "Davis Quartz"];
    var zoneIndex = zoneNameShort.indexOf(zoneId);
    var zoneName = zoneListComp[zoneIndex];
    var heading = native.getEntityHeading(alt.Player.local.scriptID);
    var health = native.getEntityHealth(alt.Player.local.scriptID);
    var meterPSec = -1;
    var fuelLevel = 0;
    var odoReading = 0;
    if (alt.Player.local.vehicle !== null) {
        meterPSec = native.getEntitySpeed(alt.Player.local.vehicle.scriptID);
        fuelLevel = alt.Player.local.vehicle.getSyncedMeta("FUELLEVEL");
        odoReading = alt.Player.local.vehicle.getSyncedMeta("ODOREADING");
    }
    hudWebView.emit('SetHudData', health, hudInfo.Money, heading, hudInfo.Hour, hudInfo.Minute, streetName, zoneName, meterPSec, fuelLevel, odoReading);
}
function HideHud() {
    if (hudWebView === undefined)
        return;
    alt.setTimeout(() => {
        hudWebView.destroy();
        hudWebView = undefined;
    }, 1000);
}
