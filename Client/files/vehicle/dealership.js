if ('alt' in window) {
    //alt.on('noBankCards', NoBankCards);
    alt.on('loadDealershipVehicleInfo', LoadDealershipVehicleInfo);
}

function LoadDealershipVehicleInfo(json) {
    var vehicleList = JSON.parse(json);

    var table = document.getElementById("VehicleTable");

    vehicleList.forEach(vehicle => {
        var row = table.insertRow(1);
        var cell0 = row.insertCell(0);
        var cell1 = row.insertCell(1);
        var cell2 = row.insertCell(2);

        cell0.innerHTML = vehicle.VehName;

        cell1.innerHTML = formatter.format(vehicle.VehPrice);

        var button = document.createElement("BUTTON");
        var t = document.createTextNode("Preview");
        button.appendChild(t);
        button.classList.add('btn', 'btn-success');
        var index = vehicleList.indexOf(vehicle);
        button.onclick = function () {
            previewVehicleSelect(index);
        };
        cell2.appendChild(button);
    });
}

const formatter = new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
    minimumFractionDigits: 0
});

function previewVehicleSelect(index) {
    alt.emit('previewDealershipVehicle', index);
}

function viewDealershipLoaded() {
    alt.emit('viewDealershipLoaded');
}

function ClosePage() {
    alt.emit('Dealership:ClosePage');
}