import * as alt from 'alt-client';
import * as native from 'natives';

var bankView:alt.WebView = undefined;
var BankJson:any = undefined;
var currentBankAccount:any = undefined;

alt.onServer('ShowBankMenu', ShowBankMenu);

function ShowBankMenu(bankJson:string) {
    BankJson = bankJson

    if (bankView !== undefined) {
        CloseBankView();
    }

    bankView = new alt.WebView('http://resource/files/bank/bankHome.html', false);
    bankView.focus();

    // Home Page
    bankView.on('bankHomePageLoaded', BankHomePageLoaded);
    bankView.on('ViewBankInfo', ViewBankInfo);
    bankView.on('closeBankHomePage', CloseBankHomePage);

    // Account View
    bankView.on('bankAccountPageLoaded', BankAccountPageLoaded);
    bankView.on('requestTransactions', RequestTransactions);

    // Transaction Input
    bankView.on('BankTransactionState', SetBankTransactionState);
    bankView.on('requestTransactionState', RequestTransactionState);
    bankView.on('HandleBankTransaction', HandleBankTransaction);

    bankView.on('HandleBankTransfer', HandleBankTransfer);

    bankView.on('RequestNewBankCard', RequestNewBankCard);

    bankView.on('CloseBankAccount', CloseBankAccount);

    bankView.on('requestNewBankAccount', RequestNewBankAccount);

    bankView.on('SetAsActive', setAsActive);

    alt.showCursor(true);
    native.freezeEntityPosition(alt.Player.local.scriptID, true);
}

function setAsActive() {
    var currentAccount = JSON.parse(currentBankAccount);

    alt.emitServer('SetBankAccountActive', currentAccount.AccountNumber.toString());

    CloseBankView();
}

function RequestNewBankAccount(accountType:any) {
    alt.emitServer('RequestNewBankAccount', accountType.toString());
    CloseBankView();
}

function CloseBankAccount() {
    var currentAccount = JSON.parse(currentBankAccount);

    alt.emitServer('RequestBankAccountClosure', currentAccount.AccountNumber.toString());

    CloseBankView();
}

function RequestNewBankCard() {
    var currentAccount = JSON.parse(currentBankAccount);

    alt.emitServer('RequestNewBankCardPin', currentAccount.AccountNumber.toString());

    CloseBankView();
}

function HandleBankTransfer(targetAccountNumber:any, amount:any) {
    var currentAccount = JSON.parse(currentBankAccount);
    alt.emitServer('BankTransfer', currentAccount.AccountNumber.toString(), targetAccountNumber.toString(), amount.toString());
    CloseBankView();
}

function HandleBankTransaction(state:any, amount:any) {
    var currentAccount = JSON.parse(currentBankAccount);
    // States: 0 = Deposit, 1 = Withdraw
    alt.emitServer('BankTransaction', currentAccount.AccountNumber.toString(), state.toString(), amount.toString());
    CloseBankView();
}

var transactionState:any = undefined;
function SetBankTransactionState(state:any) {
    transactionState = state;
}

function RequestTransactionState() {
    if (bankView === undefined) return;

    bankView.emit('TransactionState', transactionState);
}

function RequestTransactions() {
    var currentAccount = JSON.parse(currentBankAccount);
    bankView.emit('transactionHistory', currentAccount.TransactionHistoryJson);
}

function BankAccountPageLoaded() {
    bankView.emit('currentBankData', currentBankAccount);
}

function CloseBankHomePage() {
    CloseBankView();
}

function ViewBankInfo(index:number) {
    var BankAccounts = JSON.parse(BankJson);

    var BankAccount = BankAccounts[index];
    currentBankAccount = JSON.stringify(BankAccount);
    if (BankAccount == undefined) {
        CloseBankView();
        alt.showCursor(false);
        return;
    }
}

function BankHomePageLoaded() {
    bankView.emit('LoadBankAccounts', BankJson);
}

function CloseBankView() {
    if (bankView !== undefined) {
        bankView.destroy();
        bankView = undefined;
        native.freezeEntityPosition(alt.Player.local.scriptID, false);
        alt.showCursor(false);
        alt.emitServer('bankViewClosed');
    }
}