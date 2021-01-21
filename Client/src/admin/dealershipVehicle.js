var vehicleListJson = undefined;

function dealershipVehicleLoaded() {
    alt.emit('admin:dealership:dealershipVehicleLoaded');
}

if ('alt' in window) {
    alt.on('admin:dealership:loadVehicleList', loadVehicleList);
    alt.on('admin:dealership:editPageInfo', editPageLoaded);
}

function loadVehicleList(dealerName, json) {
    vehicleListJson = json;

    var dealershipName = document.getElementById('dealerName');

    dealershipName.innerText = dealerName;

    var table = document.getElementById('vehicleTable');

    var vehicleList = JSON.parse(json);

    vehicleList.forEach(vehicle => {
        var row = table.insertRow(1);

        var cell0 = row.insertCell(0);
        var cell1 = row.insertCell(1);
        var cell2 = row.insertCell(2);

        cell0.innerText = vehicle.VehName;
        cell1.innerText = formatter.format(vehicle.VehPrice);

        var button = document.createElement("BUTTON");
        var t = document.createTextNode("Edit");
        button.appendChild(t);
        button.classList.add('btn', 'btn-success');
        var index = vehicleList.indexOf(vehicle);
        button.onclick = function () {
            editSelectedVehicle(index);
        };
        cell2.appendChild(button);
    });
}

function editSelectedVehicle(index) {
    alt.emit('admin:dealership:editSelectedVehicle', index);
    window.location.href = "http://resource/files/admin/editDealershipVehicle.html";
}

function addDealershipVehicleClick() {
    console.log('AddVehicleClick');
    window.location.href = "http://resource/files/admin/addDealershipVehicle.html";
}

function CloseAdminDealership() {
    console.log('ClosePage');
    alt.emit('admin:dealership:ClosePage');
}

const formatter = new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
    minimumFractionDigits: 0
});

// addDealershipVehicle.html

function addNewDealershipVehicleClick() {
    var vehicleName = document.getElementById('vehicleName').value;
    var vehicleModel = document.getElementById('vehicleModel').value;
    var vehiclePrice = document.getElementById('vehiclePrice').valueAsNumber;

    alt.emit('admin:dealership:addNewDealershipVehicle', vehicleName, vehicleModel, vehiclePrice.toString());
}

function editDealershipVehicleLoaded() {
    alt.emit('admin:dealership:editPageLoaded');
}

function editPageLoaded(index, vehicleListJson) {
    var vehicleList = JSON.parse(vehicleListJson);

    var vehicle = vehicleList[index];

    if (vehicle === undefined || vehicle === null) {
        CloseAdminDealership();
        return;
    }

    var vehicleName = document.getElementById('vehicleName');
    var vehicleModel = document.getElementById('vehicleModel');
    var vehiclePrice = document.getElementById('vehiclePrice');

    vehicleName.value = vehicle.VehName;
    vehicleModel.value = vehicle.VehModel;
    vehiclePrice.value = vehicle.VehPrice;
}

function editDealershipVehicle() {
    var vehicleName = document.getElementById('vehicleName').value;
    var vehicleModel = document.getElementById('vehicleModel').value;
    var vehiclePrice = document.getElementById('vehiclePrice').valueAsNumber;

    alt.emit('admin:dealership:editDealershipVehicle', vehicleName, vehicleModel, vehiclePrice);
}

function removeDealershipVehicle() {
    alt.emit('admin:dealership:removeDealershipVehicle');
}