import * as alt from 'alt-client';
import * as native from 'natives';
var atm_01 = 3424098598;
var atm_02 = 3168729781;
var atm_03 = 2930269768;
var atm_04 = 506770882;
var atmObject = undefined;
var atmLoaded = false;
var atmView = undefined;
alt.onServer("atAtm", atAtm);
function atAtm() {
    var pos = alt.Player.local.pos;
    atmObject = native.getClosestObjectOfType(pos.x, pos.y, pos.z, 1, atm_01, false, true, true);
    if (atmObject === 0) {
        atmObject = native.getClosestObjectOfType(pos.x, pos.y, pos.z, 1, atm_02, false, true, true);
    }
    if (atmObject === 0) {
        atmObject = native.getClosestObjectOfType(pos.x, pos.y, pos.z, 1, atm_03, false, true, true);
    }
    if (atmObject === 0) {
        atmObject = native.getClosestObjectOfType(pos.x, pos.y, pos.z, 1, atm_04, false, true, true);
    }
    if (atmObject === 0)
        return;
    loadAtmView();
}
function loadAtmView() {
    alt.log('Atm View Loading');
    atmView = new alt.WebView('http://resource/files/bank/atmHome.html', false);
    atmView.focus();
    atmView.on('atmPageLoaded', AtmPageLoaded);
    atmView.on('closeAtmPage', CloseAtmPage);
    atmView.on('selectedBankCard', SelectedBankCard);
    atmView.on('PinInput', InputPin);
    atmView.on('AtmRequestBalance', AtmRequestBalance);
    atmView.on('atmWithdrawal', AtmWithdrawal);
    alt.showCursor(true);
    native.taskStartScenarioInPlace(alt.Player.local.scriptID, "PROP_HUMAN_ATM", 0, true);
}
function AtmWithdrawal(balance) {
    if (atmView === undefined)
        return;
    var selectedAccount = JSON.parse(selectedBankAccountJson);
    if (selectedAccount.Balance < balance) {
        atmView.emit("withdraw:errorMessage", "You don't have this much in the account!");
        return;
    }
    if (balance < 0) {
        atmView.emit("withdraw:errorMessage", "You can't go negative fool!");
        return;
    }
    if (balance > 1000) {
        atmView.emit("withdraw:errorMessage", "You can only withdraw upto $500.");
        return;
    }
    alt.emitServer('AtmWithdrawFunds', selectedAccount.AccountNumber.toString(), balance.toString());
    CloseAtmPage();
}
function AtmRequestBalance() {
    if (atmView === undefined)
        return;
    var selectedAccount = JSON.parse(selectedBankAccountJson);
    atmView.emit('AtmBalance', selectedAccount.Balance);
}
function AtmPageLoaded() {
    alt.emitServer('AtmPageLoaded');
}
function CloseAtmPage() {
    if (atmView !== undefined) {
        alt.showCursor(false);
        atmView.destroy();
        atmView = undefined;
        alt.emitServer("atm:pageClosed");
        native.clearPedTasksImmediately(alt.Player.local.scriptID);
        alt.setInterval(() => {
            atmLoaded = false;
        }, 3000);
    }
}
alt.onServer('atm:NoBankCards', NoBankCards);
function NoBankCards() {
    if (atmView === undefined)
        return;
    atmView.emit('noBankCards');
}
alt.onServer('atm:loadCardData', LoadCardData);
var playerCards = undefined;
var cardAccounts = undefined;
var selectedCard = undefined;
var selectedBankAccountJson = undefined;
function LoadCardData(cards, bankAccounts) {
    if (atmView === undefined)
        return;
    alt.log(cards);
    alt.log(bankAccounts);
    playerCards = cards;
    cardAccounts = bankAccounts;
    atmView.emit('SendCardData', cards);
}
var selectedCardJson = undefined;
function SelectedBankCard(cardJson) {
    selectedCardJson = cardJson;
}
function InputPin(pin) {
    var selectedBankCard = JSON.parse(selectedCardJson);
    var accNo = selectedBankCard.ItemValue;
    alt.log("Account No: " + accNo);
    alt.log("Card Accounts JSON: " + cardAccounts);
    var bankAccounts = JSON.parse(cardAccounts);
    var SelectedBankAccount = undefined;
    for (var bankAccount of bankAccounts) {
        if (bankAccount.AccountNumber == accNo) {
            SelectedBankAccount = bankAccount;
            break;
        }
    }
    if (SelectedBankAccount === undefined) {
        alt.log("Selected Bank Account Undefined");
        return;
    }
    alt.log("Selected Bank Account: " + JSON.stringify(SelectedBankAccount));
    if (SelectedBankAccount.Pin == pin) {
        if (atmView === undefined)
            return;
        selectedBankAccountJson = JSON.stringify(SelectedBankAccount);
        alt.setTimeout(() => {
            atmView.emit('LoadWithdrawScreen');
        }, 250);
    }
    else {
        alt.emitServer('ATMSystem:IncorrectPin', SelectedBankAccount.AccountNumber.toString());
    }
}
var paymentReturnEvent = undefined;
var paymentCards = undefined;
var paymentBankAccounts = undefined;
var paymentView = undefined;
alt.onServer('showPaymentScreen', showPaymentScreen);
function showPaymentScreen(cards, bankAccount, returnEvent) {
    paymentReturnEvent = returnEvent;
    paymentCards = cards;
    paymentBankAccounts = bankAccount;
    alt.showCursor(true);
    if (paymentView !== undefined) {
        closePaymentView();
    }
    paymentView = new alt.WebView('http://resource/files/bank/paymentHome.html', false);
    paymentView.focus();
    paymentView.on('paymentHomeLoaded', paymentHomeLoaded);
    paymentView.on('selectedPaymentBankCard', selectedPaymentBankCard);
    paymentView.on('paymentPinInput', paymentPinInput);
    paymentView.on('cashPayment', cashPayment);
    paymentView.on('closePaymentPage', closePaymentView);
}
function cashPayment() {
    closePaymentView();
    alt.emitServer(paymentReturnEvent, 'cash');
}
function paymentHomeLoaded() {
    paymentView.emit('SendPaymentCardData', paymentCards);
}
function closePaymentView() {
    alt.log('PaymentClosed');
    alt.showCursor(false);
    paymentView.destroy();
    paymentView = undefined;
    alt.emitServer(paymentReturnEvent, 'close');
}
function selectedPaymentBankCard(selectedCard) {
    selectedCardJson = selectedCard;
}
function paymentPinInput(pin) {
    alt.log(pin);
    var selectedBankCard = JSON.parse(selectedCardJson);
    var accNo = selectedBankCard.ItemValue;
    var bankAccounts = JSON.parse(paymentBankAccounts);
    var selectedBankAccount;
    for (var bankAccount of bankAccounts) {
        if (bankAccount.AccountNumber == accNo) {
            selectedBankAccount = bankAccount;
            break;
        }
    }
    if (selectedBankAccount === undefined) {
        return;
    }
    alt.log('selectedBankAccount Pin: ' + selectedBankAccount.Pin);
    if (selectedBankAccount.Pin == pin) {
        if (paymentView === undefined) {
            return;
        }
        alt.setTimeout(() => {
            closePaymentView();
            alt.emitServer(paymentReturnEvent, accNo);
        }, 250);
        return;
    }
    closePaymentView();
    alt.emitServer(paymentReturnEvent, "IncorrectPin");
}
