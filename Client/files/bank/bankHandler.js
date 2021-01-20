import * as alt from 'alt-client';
import * as native from 'natives';
var bankView = undefined;
let BankJson = undefined;
let currentBankAccount = undefined;
alt.onServer('ShowBankMenu', showBankMenu);
function closeBankView() {
    try {
        if (bankView != undefined) {
            alt.setTimeout(() => {
                bankView.destroy();
                bankView = undefined;
            }, 1100);
        }
        native.freezeEntityPosition(alt.Player.local.scriptID, false);
        alt.showCursor(false);
        alt.emitServer('bankViewClosed');
        return;
    }
    catch (e) {
        alt.log(e);
        return;
    }
}
function showBankMenu(bankJson) {
    BankJson = bankJson;
    bankView = new alt.WebView('http://resource/files/bank/bankHome.html', false);
    bankView.focus();
    bankView.on('bankHomePageLoaded', bankHomePageLoaded);
    bankView.on('ViewBankInfo', viewBankInfo);
    bankView.on('closeBankHomePage', closeBankHomePage);
    bankView.on('bankAccountPageLoaded', bankAccountPageLoaded);
    bankView.on('requestTransactions', requestTransactions);
    bankView.on('BankTransactionState', setBankTransactionState);
    bankView.on('requestTransactionState', requestTransactionState);
    bankView.on('HandleBankTransaction', handleBankTransaction);
    bankView.on('HandleBankTransfer', handleBankTransfer);
    bankView.on('RequestNewBankCard', RequestNewBankCard);
    bankView.on('CloseBankAccount', CloseBankAccount);
    bankView.on('requestNewBankAccount', RequestNewBankAccount);
    bankView.on('SetAsActive', setAsActive);
    alt.showCursor(true);
    native.freezeEntityPosition(alt.Player.local.scriptID, true);
}
function setAsActive() {
    const currentAccount = JSON.parse(currentBankAccount);
    alt.emitServer('SetBankAccountActive', currentAccount.AccountNumber.toString());
    closeBankView();
}
function RequestNewBankAccount(accountType) {
    alt.emitServer('RequestNewBankAccount', accountType.toString());
    closeBankView();
}
function CloseBankAccount() {
    const currentAccount = JSON.parse(currentBankAccount);
    alt.emitServer('RequestBankAccountClosure', currentAccount.AccountNumber.toString());
    closeBankView();
}
function RequestNewBankCard() {
    const currentAccount = JSON.parse(currentBankAccount);
    alt.emitServer('RequestNewBankCardPin', currentAccount.AccountNumber.toString());
    closeBankView();
}
function handleBankTransfer(targetAccountNumber, amount) {
    const currentAccount = JSON.parse(currentBankAccount);
    alt.emitServer('BankTransfer', currentAccount.AccountNumber.toString(), targetAccountNumber.toString(), amount.toString());
    closeBankView();
}
function handleBankTransaction(state, amount) {
    alt.log('TransactionHandle Event');
    const currentAccount = JSON.parse(currentBankAccount);
    alt.log('Data:' + currentAccount);
    alt.emitServer('BankTransaction', currentAccount.AccountNumber.toString(), state.toString(), amount.toString());
    alt.log('emit Event: ' + currentAccount.AccountNumber.toString() + '-' + state.toString() + '-' + amount.toString());
    closeBankView();
}
let transactionState = undefined;
function setBankTransactionState(state) {
    transactionState = state;
}
function requestTransactionState() {
    if (bankView === undefined)
        return;
    bankView.emit('TransactionState', transactionState);
}
function requestTransactions() {
    var currentAccount = JSON.parse(currentBankAccount);
    bankView.emit('transactionHistory', currentAccount.TransactionHistoryJson);
}
function bankAccountPageLoaded() {
    bankView.emit('currentBankData', currentBankAccount);
}
function closeBankHomePage() {
    closeBankView();
}
function viewBankInfo(index) {
    var BankAccounts = JSON.parse(BankJson);
    var BankAccount = BankAccounts[index];
    currentBankAccount = JSON.stringify(BankAccount);
    if (BankAccount == undefined) {
        closeBankView();
        alt.showCursor(false);
        return;
    }
}
function bankHomePageLoaded() {
    bankView.emit('LoadBankAccounts', BankJson);
}
