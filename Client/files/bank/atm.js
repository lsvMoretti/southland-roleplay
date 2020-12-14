if ('alt' in window) {
    alt.on('noBankCards', NoBankCards);
    alt.on('SendCardData', SendCardData);
    alt.on('LoadWithdrawScreen', LoadWithdrawScreen);
    alt.on('AtmBalance', AtmBalance);
    alt.on('withdraw:errorMessage', WithdrawErrorMessage);
    alt.on('SendPaymentCardData', SendPaymentCardData);
}

function WithdrawErrorMessage(message) {
    var errorText = document.getElementById("errorMessage");
    errorText.innerText = message;
    window.setTimeout(() => {
        errorText.innerText = "";
    }, 5000);
}

function atmWithdrawSubmit() {
    var value = document.getElementById("numberInput").value;

    var errorText = document.getElementById("errorMessage");

    if (value > 1000) {
        errorText.innerText = "You can only withdraw up to $1,000";
        window.setTimeout(() => {
            errorText.innerText = "";
        }, 5000);
        return;
    }

    alt.emit('atmWithdrawal', value);
}

function AtmBalance(balance) {
    var balanceValue = document.getElementById("bankBalance");

    balanceValue.innerText = "Bank Balance: $" + balance;
}

function withdrawalPageLoaded() {
    alt.emit('AtmRequestBalance');
}

function LoadWithdrawScreen() {
    window.location.href = "http://resource/files/bank/atmWithdrawal.html";
}

var bankCardJson = undefined;

function SendCardData(cardData) {
    bankCardJson = cardData;

    var bankCards = JSON.parse(cardData);

    var table = document.getElementById("CardTable");

    bankCards.forEach(bankCard => {
        var row = table.insertRow(1);
        var cell0 = row.insertCell(0);
        var cell1 = row.insertCell(1);

        cell0.innerHTML = bankCard.ItemValue;

        var button = document.createElement("BUTTON");
        var t = document.createTextNode("Access");
        button.appendChild(t);
        button.classList.add('btn', 'btn-success');
        var index = bankCards.indexOf(bankCard);
        button.onclick = function () {
            cardAttempt(index);
        };
        cell1.appendChild(button);
    });
}

function cardAttempt(index) {
    var cards = JSON.parse(bankCardJson);
    var selectedCard = cards[index];

    alt.emit('selectedBankCard', JSON.stringify(selectedCard));

    window.setTimeout(() => {
        window.location.href = "http://resource/files/bank/atmPinInput.html";
    }, 500);
}

function NoBankCards() {
    var errorText = document.getElementById('errorText');

    errorText.innerHTML = "You have no cards on you!";

    window.setTimeout(() => {
        ClosePage();
    }, 3000)
}

function atmHomeLoaded() {
    alt.emit('atmPageLoaded');
}

function ClosePage() {
    alt.emit('closeAtmPage');
}

function pinInput(inputValue) {
    var pinValue = document.getElementById("pinValue");

    if (pinValue.innerText.length > 4) return;

    pinValue.innerText = pinValue.innerText + inputValue;
}

function pinClear() {
    var pinValue = document.getElementById("pinValue");

    var currentHtml = pinValue.innerText;

    if (currentHtml.length <= 1) return;

    pinValue.innerText = currentHtml.slice(0, -1);
}

function pinAccept() {
    var pinValue = document.getElementById("pinValue").innerText;

    var spliced = pinValue.slice(1);

    alt.emit("PinInput", spliced);
}

function setTwoNumberDecimal(event) {
    event.value = parseFloat(event.value).toFixed(2);
}

function paymentHomeLoaded() {
    alt.emit('paymentHomeLoaded');
}

function SendPaymentCardData(cardData) {
    bankCardJson = cardData;

    var bankCards = JSON.parse(cardData);

    var table = document.getElementById("CardTable");

    bankCards.forEach(bankCard => {
        var row = table.insertRow(1);
        var cell0 = row.insertCell(0);
        var cell1 = row.insertCell(1);

        cell0.innerHTML = bankCard.ItemValue;

        var button = document.createElement("BUTTON");
        var t = document.createTextNode("Access");
        button.appendChild(t);
        button.classList.add('btn', 'btn-success');
        var index = bankCards.indexOf(bankCard);
        button.onclick = function () {
            paymentCardAttempt(index);
        }
        cell1.appendChild(button);
    });
}

function paymentCardAttempt(index) {
    var cards = JSON.parse(bankCardJson);
    var selectedCard = cards[index];

    alt.emit('selectedPaymentBankCard', JSON.stringify(selectedCard));

    window.setTimeout(() => {
        window.location.href = "http://resource/files/bank/paymentPinInput.html";
    }, 500);
}

function paymentPinAccept() {
    var pinValue = document.getElementById("pinValue").innerText;

    var spliced = pinValue.slice(1);
    alt.emit("paymentPinInput", spliced);
}

function cashPayment() {
    alt.emit('cashPayment');
}

function ClosePaymentPage() {
    alt.emit('closePaymentPage');
}