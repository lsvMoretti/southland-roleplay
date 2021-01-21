if ('alt' in window) {
    alt.on('previewVehicleInfo', PreviewVehicleInfo);
}

var currentRotation = 0;
var currentRotationValue = document.getElementById('currentRotationValue');
var rotationSlider = document.getElementById('previewRotationSlider');

function previewVehicleLoaded() {
    alt.emit('dealership:fetchPreviewVehicleInfo');
}

rotationSlider.addEventListener("input", rotationSliderChange);

function rotationSliderChange() {
    currentRotationValue.innerHTML = this.value;
    currentRotation = this.value;
    alt.emit('dealership:rotationchange', this.value);
}

function ClosePage() {
    alt.emit('Dealership:ClosePage');
}

function PreviewVehicleInfo(vehName, vehPrice, hasVoucher) {
    rotationSlider.value = currentRotation;
    currentRotationValue.innerHTML = rotationSlider.value;

    var title = document.getElementById('heading');
    var subtitle = document.getElementById('subtitle');

    title.innerText = vehName;
    subtitle.innerText = formatter.format(vehPrice);

    if (!hasVoucher) {
        var voucherDiv = document.getElementById("voucherRow");

        voucherDiv.classList.add('hideme');
    }
}

const formatter = new Intl.NumberFormat('en-US', {
    style: 'currency',
    currency: 'USD',
    minimumFractionDigits: 0
});
function redPreviewClick() {
    console.log('Red Preview Click');
    alt.emit('vehiclePreviewColorChange', 'red');
}

function bluePreviewClick() {
    alt.emit('vehiclePreviewColorChange', 'blue');
}

function whitePreviewClick() {
    alt.emit('vehiclePreviewColorChange', 'white');
}

function greenPreviewClick() {
    alt.emit('vehiclePreviewColorChange', 'green');
}

function orangePreviewClick() {
    alt.emit('vehiclePreviewColorChange', 'orange');
}

function blackPreviewClick() {
    alt.emit('vehiclePreviewColorChange', 'black');
}

function purchaseClick() {
    alt.emit('vehiclePreviewPurchaseVehicle');
}

function voucherClick() {
    alt.emit('vehiclePreviewVoucherPurchase');
}