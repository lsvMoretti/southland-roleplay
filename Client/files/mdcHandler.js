import * as alt from 'alt-client';
import * as native from 'natives';
var mdcWindow = undefined;
var officerName = "";
var officerRank = "";
var officerUnit = "";
var dutyStatus = false;
var personResults = undefined;
var vehicleResults = undefined;
alt.onServer('showMDC', showMDC);
alt.onServer('closeMDC', closeMdc);
alt.onServer('911CallList', send911Calls);
alt.onServer('MdcPersonSearchResult', personSearchResult);
alt.onServer('MdcPersonSearchNoResult', mdcPersonSearchNoResult);
alt.onServer('MdcPersonProfileInformation', MdcPersonProfileInformation);
alt.onServer('MdcVehicleSearchNoResult', mdcVehicleSearchNoResult);
alt.onServer('mdc:vehicleSearchResult', vehicleSearchResult);
function showMDC(name, rank, unit) {
    alt.showCursor(true);
    if (unit !== undefined) {
        officerUnit = unit;
    }
    officerName = name;
    officerRank = rank;
    alt.log('Showing MDT for ' + name + rank);
    if (mdcWindow !== undefined) {
        alt.setTimeout(() => {
            mdcWindow.destroy();
            mdcWindow = undefined;
        }, 1000);
    }
    mdcWindow = new alt.WebView("http://resource/files/mdc/home.html", false);
    mdcWindow.on('closeMDC', closeMdc);
    mdcWindow.on('mdcHomeLoaded', mdcHomePageLoaded);
    mdcWindow.on('changePatrolDutyStatus', changePatrolDutyStatus);
    mdcWindow.on('setPatrolUnit', setPatrolUnit);
    mdcWindow.on('respondLast911', respondLast911);
    mdcWindow.on('requestBackup', requestBackup);
    mdcWindow.on('fetch911Calls', fetch911Calls);
    mdcWindow.on('respond911', respond911);
    mdcWindow.on('mdcPersonSearch', mdcPersonSearch);
    mdcWindow.on('MdcSerchResultProfileSelect', MdcSearchResultProfileSelect);
    mdcWindow.on('fetchProfileInformation', fetchProfileInformation);
    mdcWindow.on('mdc:personPropertySelected', MdcPersonSearchPropertySelected);
    mdcWindow.on('mdc:plateSearch', mdcVehicleSearch);
    mdcWindow.on('vehicleSearchResultSelect', vehicleSearchResultSelect);
    mdcWindow.on('vehicleProfileLoaded', vehicleProfilePageLoaded);
    mdcWindow.focus();
}
alt.everyTick(() => {
    if (mdcWindow !== undefined) {
        for (let index = 0; index < 31; index++) {
            native.disableInputGroup(index);
        }
    }
});
var profileJson = null;
function fetchProfileInformation() {
    mdcWindow.emit('loadPersonProfileInformation', profileJson);
}
function MdcPersonProfileInformation(json) {
    profileJson = json;
    mdcWindow.emit('loadPersonProfilePage');
}
function MdcSearchResultProfileSelect(index) {
    var selectedResult = personResults[index];
    if (selectedResult === null) {
        alt.log("Invalid Selection");
        return;
    }
    alt.log("Selected: " + selectedResult.Name);
    alt.emitServer('MdcSearchResultProfileSelected', index.toString());
}
function personSearchResult(json) {
    personResults = JSON.parse(json);
    mdcWindow.emit('personSearchResult', json);
}
function mdcPersonSearch(name) {
    alt.emitServer('mdcPersonSearch', name.toString());
}
function mdcPersonSearchNoResult() {
    mdcWindow.emit('mdcPersonSearchNoResult');
}
function MdcPersonSearchPropertySelected(propertyIndex) {
    var profile = JSON.parse(profileJson);
    var selectedProperty = profile.OwnedProperties[propertyIndex];
    alt.emitServer('mdc:personSearchPropertySelected', selectedProperty.toString());
    closeMdc();
}
function mdcVehicleSearch(plate) {
    alt.emitServer('mdc:vehicleSearch', plate.toString());
}
function mdcVehicleSearchNoResult() {
    mdcWindow.emit('mdcVehicleSearchNoResult');
}
function vehicleSearchResult(json) {
    vehicleResults = JSON.parse(json);
    mdcWindow.emit('vehicleSearchResult', json);
}
var selectedVehicleResult = undefined;
function vehicleSearchResultSelect(index) {
    selectedVehicleResult = vehicleResults[index];
    if (selectedVehicleResult === null) {
        alt.log("Invalid Selection");
        return;
    }
    alt.log("Selected: " + selectedVehicleResult.Plate);
    mdcWindow.emit('loadVehicleProfilePage');
}
function vehicleProfilePageLoaded() {
    mdcWindow.emit('vehicleProfileData', selectedVehicleResult);
}
function fetch911Calls() {
    alt.emitServer('fetch911Calls');
}
function send911Calls(json) {
    mdcWindow.emit("loadCalls", json);
}
function respond911(index) {
    alt.emitServer('Responding911', index);
}
function requestBackup() {
    if (mdcWindow === undefined)
        return;
    alt.emitServer('requestBackup');
}
function respondLast911() {
    if (mdcWindow === undefined)
        return;
    alt.emitServer('respondLast911');
}
function setPatrolUnit(unit) {
    if (mdcWindow === undefined)
        return;
    officerUnit = unit;
    alt.emitServer('mdc:setPatrolUnit', unit);
    mdcWindow.emit('sendMDCHomeInfo', dutyStatus, officerName, officerRank, officerUnit);
}
function changePatrolDutyStatus() {
    if (mdcWindow === undefined)
        return;
    dutyStatus = !dutyStatus;
    alt.log('new duty status: ' + dutyStatus);
    mdcWindow.emit('sendMDCHomeInfo', dutyStatus, officerName, officerRank, officerUnit);
}
function mdcHomePageLoaded() {
    if (mdcWindow === undefined)
        return;
    mdcWindow.emit('sendMDCHomeInfo', dutyStatus, officerName, officerRank, officerUnit);
}
function closeMdc() {
    if (mdcWindow === undefined)
        return;
    alt.setTimeout(() => {
        mdcWindow.destroy();
        mdcWindow = undefined;
        alt.emitServer('mdcWindowClose');
    }, 1000);
    alt.showCursor(false);
}
