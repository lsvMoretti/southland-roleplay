function homeClick() {
    //alt.emit('mdcHomeClick');
}

function callClick() {
    //alt.emit('mdcCallClick');
}

function personsClick() {
}

function vehicleClick() {
}

function boloClick() {
}

function closeClick() {
    alt.emit('closeMDC');

    document.getElementById("bodyClass").classList.add("hideme");
}

if ('alt' in window) {
    //#region Home Page
    //Once page is loaded, clientside will call this to load information
    alt.on('sendMDCHomeInfo', setOfficerDuty);
    //#endregion

    //#region 911 Page

    alt.on('loadCalls', loadCalls);
    //#endregion

    //#region Person Search
    alt.on('personSearchResult', personSearchResult);
    alt.on('mdcPersonSearchNoResult', mdcPersonSearchNoResult);
    alt.on('loadPersonProfilePage', loadPersonProfilePage);
    alt.on('loadPersonProfileInformation', loadPersonProfileInformation);

    //#endregion

    //#region Vehicle Search

    alt.on('mdcVehicleSearchNoResult', mdcVehicleSearchNoResult);

    alt.on('vehicleSearchResult', vehicleSearchResult);

    alt.on('loadVehicleProfilePage', loadVehicleProfilePage);

    alt.on('vehicleProfileData', vehicleProfileData);

    //#endregion
}

//#region Persons Search Page
function loadPersonProfilePage() {
    window.location.href = "http://resource/files/mdc/personProfile.html";
}

function loadPersonProfileInformation(json) {
    var profileInformation = JSON.parse(json);
    var nameField = document.getElementById('nameInput');

    nameField.textContent = "Name: " + profileInformation.Name;

    var ageField = document.getElementById('ageInput');

    ageField.textContent = "Age: " + profileInformation.Age;

    var genderField = document.getElementById('genderInput');

    genderField.textContent = "Gender: " + profileInformation.Gender;

    var driversLicenseField = document.getElementById('driversLicense');

    if (profileInformation.DriversLicense) {
        driversLicenseField.textContent = "Drivers License: Yes";
    }
    else {
        driversLicenseField.textContent = "Drivers License: No";
    }

    var pistolLicenseField = document.getElementById('pistolLicense');

    if (profileInformation.PistolLicense) {
        pistolLicenseField.textContent = "Pistol License: Yes";
    }
    else {
        pistolLicenseField.textContent = "Pistol License: No";
    }

    var propertyTable = document.getElementById('propertyTable');

    profileInformation.OwnedProperties.forEach(property => {
        var row = propertyTable.insertRow(1);

        var addressCell = row.insertCell(0);
        var navigateCell = row.insertCell(1);

        addressCell.innerHTML = property;

        var button = document.createElement("BUTTON");
        var t = document.createTextNode("Navigate");
        button.appendChild(t);
        button.classList.add('btn', 'btn-success');
        var index = profileInformation.OwnedProperties.indexOf(property);
        button.onclick = function () {
            alt.emit('mdc:personPropertySelected', index);
        };
        navigateCell.appendChild(button);
    });

    var vehicleTable = document.getElementById('vehicleTable');

    profileInformation.OwnedVehicles.forEach(vehicle => {
        var row = vehicleTable.insertRow(1);

        var modelCell = row.insertCell(0);
        var plateCell = row.insertCell(1);

        modelCell.innerHTML = vehicle.Name;
        plateCell.innerHTML = vehicle.Plate;
    });
}

function personProfileLoaded() {
    alt.emit('fetchProfileInformation');
}

function mdcPersonSearchNoResult() {
    var infoStatus = document.getElementById("infoBox");

    infoStatus.classList.add("text-danger");

    infoStatus.textContent = "ERROR! No Result!";

    window.setTimeout(() => {
        infoStatus.classList.remove("text-danger");
        infoStatus.textContent = "";
    },
        3000);
}

function personSearchResult(json) {
    var personResults = JSON.parse(json);

    document.getElementById("nameDiv").classList.add("hideme");
    document.getElementById("resultTable").classList.remove("hideme");

    var table = document.getElementById('nameResultTable');

    personResults.forEach(person => {
        var row = table.insertRow(1);

        //Name
        var cell0 = row.insertCell(0);
        //Age
        var cell1 = row.insertCell(1);
        //Gender
        var cell2 = row.insertCell(2);
        //Button
        var cell3 = row.insertCell(3);

        cell0.innerHTML = person.Name;
        cell1.innerHTML = person.Age;
        cell2.innerHTML = person.Gender;

        var button = document.createElement("BUTTON");
        var t = document.createTextNode("Profile");
        button.appendChild(t);
        button.classList.add('btn', 'btn-success');
        var index = personResults.indexOf(person);
        button.onclick = function () {
            personProfileSelect(index);
        };
        cell3.appendChild(button);
    });
}

function personProfileSelect(index) {
    alt.emit("MdcSerchResultProfileSelect", index);
}

function personNameSearch() {
    var inputText = document.getElementById("nameInput").value;

    if (inputText.length === 0) return;

    alt.emit('mdcPersonSearch', inputText);
}

//#endregion

//#region Vehicle Search

function plateSearchSubmit() {
    var inputText = document.getElementById("plateInput").value;

    if (inputText.length === 0) return;

    alt.emit("mdc:plateSearch", inputText);
}

function mdcVehicleSearchNoResult() {
    var infoStatus = document.getElementById("infoBox");

    infoStatus.classList.add("text-danger");

    infoStatus.textContent = "ERROR! No Result!";

    window.setTimeout(() => {
        infoStatus.classList.remove("text-danger");
        infoStatus.textContent = "";
    },
        3000);
}

function vehicleSearchResult(json) {
    var vehicleResults = JSON.parse(json);

    document.getElementById("nameDiv").classList.add("hideme");
    document.getElementById("resultTable").classList.remove("hideme");

    var table = document.getElementById('nameResultTable');

    vehicleResults.forEach(vehicle => {
        var row = table.insertRow(1);

        //Plate
        var cell0 = row.insertCell(0);
        //Owner
        var cell1 = row.insertCell(1);
        //Model
        var cell2 = row.insertCell(2);
        //Button
        var cell3 = row.insertCell(3);

        cell0.innerHTML = vehicle.Plate;
        cell1.innerHTML = vehicle.Owner;
        cell2.innerHTML = vehicle.Model;

        var button = document.createElement("BUTTON");
        var t = document.createTextNode("Profile");
        button.appendChild(t);
        button.classList.add('btn', 'btn-success');
        var index = vehicleResults.indexOf(vehicle);
        button.onclick = function () {
            vehicleProfileSelect(index);
        };
        cell3.appendChild(button);
    });
}

function vehicleProfileSelect(index) {
    alt.emit('vehicleSearchResultSelect', index);
}

function loadVehicleProfilePage() {
    window.location.href = "http://resource/files/mdc/vehicleProfile.html";
}

function vehicleProfileLoaded() {
    alt.emit('vehicleProfileLoaded');
}

function vehicleProfileData(vehicle) {
    var plateInput = document.getElementById('plateInput');

    var ownerInput = document.getElementById('ownerInput');

    var modelInput = document.getElementById('modelInput');

    plateInput.innerText = 'Plate: ' + vehicle.Plate;

    ownerInput.innerText = 'Owner: ' + vehicle.Owner;

    modelInput.innerText = 'Model: ' + vehicle.Model;
}

//#endregion

//#region 911 Call Page

function fetchCalls() {
    alt.emit('fetch911Calls');
}

function loadCalls(jsonCalls) {
    var callList = JSON.parse(jsonCalls);

    console.log(callList);

    var table = document.getElementById('911table');

    callList.forEach(call => {
        var row = table.insertRow(1);
        var cell0 = row.insertCell(0);
        var cell1 = row.insertCell(1);
        var cell2 = row.insertCell(2);
        var cell3 = row.insertCell(3);
        var cell4 = row.insertCell(4);

        cell0.innerHTML = call.Caller;
        cell1.innerHTML = call.Number;
        cell2.innerHTML = call.CallInformation;
        cell3.innerHTML = call.Location;

        var button = document.createElement("BUTTON");
        var t = document.createTextNode("Respond");
        button.appendChild(t);
        button.classList.add('btn', 'btn-success');
        var index = callList.indexOf(call);
        button.onclick = function () {
            respondCallClick(index);
        };
        cell4.appendChild(button);
    });
}

function respondCallClick(index) {
    //alt.emit('respondCall', index);
    console.log('Clicked Index: ' + index);
    callResponse(index);
}

//#endregion

//#region Home Page

function backupRequest() {
    var statusBox = document.getElementById('infoStatus');

    statusBox.classList.replace('text-white', 'text-warning');
    statusBox.classList.replace('text-danger', 'text-warning');

    statusBox.textContent = 'Requesting Backup!';
    window.setTimeout(closeClick, 1000);

    alt.emit('requestBackup');
}

function callResponse(index) {
    var statusBox = document.getElementById('infoStatus');

    statusBox.classList.replace('text-white', 'text-danger');

    statusBox.textContent = 'Responding to call!';

    window.setTimeout(closeClick, 1000);

    if(index == -1){
        alt.emit('respondLast911');
        return;
    }

    alt.emit('respond911', index);
}

function unitSubmit() {
    var officerUnit = document.getElementById('unitInput').value;

    var unitText = document.getElementById('officerUnit');

    unitText.textContent = 'Designated Unit: ' + officerUnit;

    var statusBox = document.getElementById('infoStatus');

    statusBox.textContent = "Unit set to: " + officerUnit;

    window.setTimeout(() => {
        statusBox.textContent = "";
    }, 6000);

    alt.emit('setPatrolUnit', officerUnit);
}

function setOfficerDuty(status, name, rank, unit) {
    var button = document.getElementById("dutyButton");

    var callMenuItem = document.getElementById("911MenuItem");
    var personMenuItem = document.getElementById("personSearchMenuItem");
    var vehicleMenuItem = document.getElementById("vehicleSearchMenuItem");
    var boloMenuItem = document.getElementById("activeBoloMenuItem");

    if (status === true) {
        // Set on Duty
        loadOfficerNameRank(name, rank, unit);
        button.classList.add("btn-success");
        button.classList.remove("btn-outline-success");
        button.textContent = "Go off Duty";

        callMenuItem.classList.remove('hideme');
        personMenuItem.classList.remove('hideme');
        vehicleMenuItem.classList.remove('hideme');
        boloMenuItem.classList.remove('hideme');

        return;
    }

    //Set off duty
    loadOfficerNameRank(name, rank, unit);
    button.classList.add("btn-outline-success");
    button.classList.remove("btn-success");
    button.textContent = "Go on Duty";

    callMenuItem.classList.add('hideme');
    personMenuItem.classList.add('hideme');
    vehicleMenuItem.classList.add('hideme');
    boloMenuItem.classList.add('hideme');
}

function loadOfficerNameRank(name, rank, unit) {
    //Load information once home page is loaded
    var nameText = document.getElementById('officerName');

    nameText.textContent = 'Officer Name: ' + name;

    var rankText = document.getElementById('officerRank');

    rankText.textContent = 'Officer Rank: ' + rank;

    var unitText = document.getElementById('officerUnit');

    unitText.textContent = 'Designated Unit: ' + unit;
}

function mdcHomeLoaded() {
    // Send message page loaded
    alt.emit('mdcHomeLoaded');
}
//#endregion