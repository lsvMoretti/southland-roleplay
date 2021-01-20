import * as alt from 'alt-client';
import * as native from 'natives';

var mdcWindow: any = undefined;

var officerName = "";
var officerRank = "";
var officerUnit = "";
var dutyStatus = false;

var personResults: any = undefined;
var vehicleResults: any = undefined;

alt.onServer('showMDC', showMDC);
alt.onServer('closeMDC', closeMdc);

alt.onServer('911CallList', send911Calls);

alt.onServer('MdcPersonSearchResult', personSearchResult);
alt.onServer('MdcPersonSearchNoResult', mdcPersonSearchNoResult);
alt.onServer('MdcPersonProfileInformation', MdcPersonProfileInformation);

alt.onServer('MdcVehicleSearchNoResult', mdcVehicleSearchNoResult);
alt.onServer('mdc:vehicleSearchResult', vehicleSearchResult);

function showMDC(name: string, rank: string, unit: string) {
    alt.showCursor(true);

    if (unit !== undefined) {
        officerUnit = unit;
    }

    officerName = name;
    officerRank = rank;

    alt.log('Showing MDT for ' + name + rank);

    // Shows the MDC
    if (mdcWindow !== undefined) {
        alt.setTimeout(() => {
            mdcWindow.destroy();
            mdcWindow = undefined;
        },
            1000);
    }

    mdcWindow = new alt.WebView("http://resource/files/mdc/home.html", false);

    mdcWindow.on('closeMDC', closeMdc);

    //#region MDC Home

    //MDC Home page loaded
    mdcWindow.on('mdcHomeLoaded', mdcHomePageLoaded);

    // Change patrol duty status
    mdcWindow.on('changePatrolDutyStatus', changePatrolDutyStatus);

    // Fired when Submit button pressed for Unit
    mdcWindow.on('setPatrolUnit', setPatrolUnit);

    mdcWindow.on('respondLast911', respondLast911);

    mdcWindow.on('requestBackup', requestBackup);

    //#endregion

    //#region MDC 911 Call Window
    mdcWindow.on('fetch911Calls', fetch911Calls);
    mdcWindow.on('respond911', respond911);
    //#endregion

    //#region MDC Person Search Page
    mdcWindow.on('mdcPersonSearch', mdcPersonSearch);
    mdcWindow.on('MdcSerchResultProfileSelect', MdcSearchResultProfileSelect);
    mdcWindow.on('fetchProfileInformation', fetchProfileInformation);
    mdcWindow.on('mdc:personPropertySelected', MdcPersonSearchPropertySelected);
    //#endregion

    //#region MDC Vehicle Search Page

    mdcWindow.on('mdc:plateSearch', mdcVehicleSearch);
    mdcWindow.on('vehicleSearchResultSelect', vehicleSearchResultSelect);
    mdcWindow.on('vehicleProfileLoaded', vehicleProfilePageLoaded);

    //#endregion

    mdcWindow.focus();
}

alt.everyTick(() => {
    if (mdcWindow !== undefined) {
        for (let index = 0; index < 31; index++) {
            native.disableInputGroup(index);
        }
    }
});

//#region MDC Persons Search Page
var profileJson: string = null;

function fetchProfileInformation() {
    mdcWindow.emit('loadPersonProfileInformation', profileJson);
}

function MdcPersonProfileInformation(json: string) {
    profileJson = json;
    mdcWindow.emit('loadPersonProfilePage');
}

function MdcSearchResultProfileSelect(index: number) {
    var selectedResult = personResults[index];

    if (selectedResult === null) {
        alt.log("Invalid Selection");
        return;
    }

    alt.log("Selected: " + selectedResult.Name);

    alt.emitServer('MdcSearchResultProfileSelected', index.toString());
}

function personSearchResult(json: string) {
    personResults = JSON.parse(json);

    mdcWindow.emit('personSearchResult', json);
}

function mdcPersonSearch(name: string) {
    alt.emitServer('mdcPersonSearch', name.toString());
}

function mdcPersonSearchNoResult() {
    mdcWindow.emit('mdcPersonSearchNoResult');
}

function MdcPersonSearchPropertySelected(propertyIndex: number) {
    var profile = JSON.parse(profileJson);

    var selectedProperty = profile.OwnedProperties[propertyIndex];

    alt.emitServer('mdc:personSearchPropertySelected', selectedProperty.toString());

    closeMdc();
}

//#endregion

//#region MDC Vehicle Search

function mdcVehicleSearch(plate: string) {
    alt.emitServer('mdc:vehicleSearch', plate.toString());
}

function mdcVehicleSearchNoResult() {
    mdcWindow.emit('mdcVehicleSearchNoResult');
}

function vehicleSearchResult(json: string) {
    vehicleResults = JSON.parse(json);

    mdcWindow.emit('vehicleSearchResult', json);
}

var selectedVehicleResult: any = undefined;

function vehicleSearchResultSelect(index: number) {
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

//#endregion

//#region MDC 911 Call Window
function fetch911Calls() {
    alt.emitServer('fetch911Calls');
}

// Send 911 Call list to page
function send911Calls(json: string) {
    mdcWindow.emit("loadCalls", json);
}

function respond911(index: number) {
    alt.emitServer('Responding911', index);
}

//#endregion

//#region Home MDC Window

function requestBackup() {
    if (mdcWindow === undefined) return;

    alt.emitServer('requestBackup');
}

function respondLast911() {
    if (mdcWindow === undefined) return;

    alt.emitServer('respondLast911');
}

function setPatrolUnit(unit: string) {
    if (mdcWindow === undefined) return;
    //Set variable
    officerUnit = unit;

    alt.emitServer('mdc:setPatrolUnit', unit);

    // Resend information
    mdcWindow.emit('sendMDCHomeInfo', dutyStatus, officerName, officerRank, officerUnit);
}

function changePatrolDutyStatus() {
    if (mdcWindow === undefined) return;
    //Toggle status
    dutyStatus = !dutyStatus;
    alt.log('new duty status: ' + dutyStatus);
    // Resend information
    mdcWindow.emit('sendMDCHomeInfo', dutyStatus, officerName, officerRank, officerUnit);
}

function mdcHomePageLoaded() {
    if (mdcWindow === undefined) return;
    //Return information to display
    mdcWindow.emit('sendMDCHomeInfo', dutyStatus, officerName, officerRank, officerUnit);
}
//#endregion

function closeMdc() {
    if (mdcWindow === undefined) return;
    alt.setTimeout(() => {
        mdcWindow.destroy();
        mdcWindow = undefined;
        alt.emitServer('mdcWindowClose');
    }, 1000);
    alt.showCursor(false);
}