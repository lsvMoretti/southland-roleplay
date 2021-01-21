if ('alt' in window) {
    alt.on('SetHudData', SetHudData);
}

var windowLoaded = false;

window.setInterval(() => {
    if (windowLoaded) {
        FetchHudData();
    }
    // 100
}, 100);

function FetchHudData() {
    if (!windowLoaded) {
        windowLoaded = true;
    }
    alt.emit('FetchHudData');
}

function SetHudData(health, money, heading, hour, minute, streetName, zoneName, meterPSec, fuelLevel, odoReading) {
    var moneyText = document.getElementById("moneyText");

    var formatter = new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency: 'USD',
        minimumFractionDigits: 2
    });

    moneyText.innerHTML = formatter.format(money);

    var directionText = document.getElementById("directionText");

    directionText.innerText = "[" + degToCard(heading) + "] " + streetName + ", " + zoneName;

    var timeText = document.getElementById("timeText");

    timeText.innerHTML = ("0" + hour).slice(-2) + ":" + ("0" + minute).slice(-2);

    var speedo = document.getElementById("speedo");
    var fuelArea = document.getElementById("fuelArea");
    var odoArea = document.getElementById("odo");

    if (meterPSec !== -1) {
        // In vehicle
        if (speedo.classList.contains("hideme")) {
            speedo.classList.remove("hideme");
        }
        if (fuelArea.classList.contains("hideme")) {
            fuelArea.classList.remove("hideme");
        }
        if (odoArea.classList.contains("hideme")) {
            odoArea.classList.remove("hideme");
        }
        var mph = meterPSec * 2.236936;

        speedo.innerText = Math.round(mph) + " MPH";

        if (fuelLevel !== undefined) {
            var fuelBar = document.getElementById("fuelBar");

            fuelBar.value = fuelLevel;
        }

        var miles = odoReading / 1609;
        var milesRounded = miles.toFixed(1);

        odoArea.innerText = milesRounded + " Miles";
    }

    else if (meterPSec === -1) {
        // Not in vehicle
        if (!speedo.classList.contains("hideme")) {
            speedo.classList.add("hideme");
        }
        if (!fuelArea.classList.contains("hideme")) {
            fuelArea.classList.add("hideme");
        }
        if (!odoArea.classList.contains("hideme")) {
            odoArea.classList.add("hideme");
        }
    }
}

var degToCard = function (deg) {
    if (deg > 11.25 && deg < 33.75) {
        return "NNW";
    } else if (deg > 33.75 && deg < 56.25) {
        return "WNW";
    } else if (deg > 56.25 && deg < 78.75) {
        return "W";
    } else if (deg > 78.75 && deg < 101.25) {
        return "WSW";
    } else if (deg > 101.25 && deg < 123.75) {
        return "WSW";
    } else if (deg > 123.75 && deg < 146.25) {
        return "SW";
    } else if (deg > 146.25 && deg < 168.75) {
        return "SSW";
    } else if (deg > 168.75 && deg < 191.25) {
        return "S";
    } else if (deg > 191.25 && deg < 213.75) {
        return "SSE";
    } else if (deg > 213.75 && deg < 236.25) {
        return "SE";
    } else if (deg > 236.25 && deg < 258.75) {
        return "ESE";
    } else if (deg > 258.75 && deg < 281.25) {
        return "E";
    } else if (deg > 281.25 && deg < 303.75) {
        return "ENE";
    } else if (deg > 303.75 && deg < 326.25) {
        return "NE";
    } else if (deg > 326.25 && deg < 348.75) {
        return "NNE";
    } else {
        return "N";
    }
}