$(document).ready(function () {
    $(window).keydown(function (event) {
        if (event.keyCode == 13) {
            event.preventDefault();
            return false;
        }
    });
});

if ('alt' in window) {
    alt.on('ReceiveInfo', ReceiveInfo);
}

var interval;
var timeout = 30;
var timeoutCount = 0;
var totalFailCount = 5;
var failCount = 0;

var correctWord;

function vehicleScrambleLoaded() {
    alt.emit('VehicleScrambleLoaded');
}

function ClosePage() {
    alt.emit('vehicleScrambleClosePage');
}

function vehicleScrambleSubmit() {
    var textInput = document.getElementById('textInput').value;
    console.log('Input: ' + textInput);
    var errorMessage = document.getElementById('errorMessage');

    if (textInput !== correctWord) {
        textInput = "";
        failCount += 1;
        console.log(failCount);
        if (failCount > totalFailCount) {
            alt.emit('VehicleScramble:MaxAttemptsReached');
            timeExpired();
            errorMessage.innerText = "Maximum Attempts Reached!";
            return;
        }
        errorMessage.innerText = "Incorrect";
        setTimeout(() => {
            errorMessage.innerText = "";
        }, 3000);
        return;
    }

    alt.emit('VehicleScramble:CorrectWord');
    timeExpired();
}

function ReceiveInfo(word, scrambled, time, attempts) {
    timeout = time;
    totalFailCount = attempts;
    var mainMessage = document.getElementById('textInputLabel');

    mainMessage.innerText = "Descramble This Word: " + scrambled;

    var timerMessage = document.getElementById('timerMessage');

    correctWord = word;

    interval = setInterval(() => {
        if (timeoutCount >= timeout) {
            alt.emit('VehicleScramble:TimeExpired');
            timeExpired();
            timerMessage.innerText = "Time Expired!!!";
            return;
        }
        var timeLeft = timeout - timeoutCount;

        timerMessage.innerText = "Time Left: " + timeLeft + " seconds";
        timeoutCount += 1;
    },
        1000);
}

function timeExpired() {
    clearInterval(interval);
    ClosePage();
}