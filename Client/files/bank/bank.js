var bankAccountJson = undefined;

if ('alt' in window) {
    alt.on('LoadBankAccounts', LoadBankAccounts);
    alt.on('currentBankData', ShowCurrentBankData);
    alt.on('transactionHistory', ShowTransactionHistory);
    alt.on('TransactionState', transactionState);
}

function OpenNewAccountPage() {
    window.location.href = "http://resource/files/bank/newBankAccount.html";
}

function createNewBankAccount() {
    var selectedRadio = document.querySelector('input[name = bankType]:checked').value;

    if (selectedRadio === null) {
        return;
    }

    alt.emit('requestNewBankAccount', selectedRadio);
}

var currentBankAccount = undefined;

function ShowCurrentBankData(currentBankJson) {
    currentBankAccount = JSON.parse(currentBankJson);

    var bankAccountValue = document.getElementById('bankAccountNumber');
    bankAccountValue.innerHTML = "Bank Account Number: " + currentBankAccount.AccountNumber;

    var accountTypeValue = document.getElementById('bankAccountType');
    var accountType = "Debit";
    if (currentBankAccount.AccountType === 0) {
        accountType = "Debit";
    }
    if (currentBankAccount.AccountType === 1) {
        accountType = "Credit";
    }
    if (currentBankAccount.AccountType === 2) {
        accountType = "Savings";
        var newCardPinButton = document.getElementById('newCardPinButton');
        newCardPinButton.classList.add("hideme");

        var cardInformation = document.getElementById('cardInformation');
        cardInformation.classList.add("hideme");
    }
    accountTypeValue.innerHTML = "Bank Account Type: " + accountType;

    var cardNumberValue = document.getElementById('activeCardNumber');
    cardNumberValue.innerHTML = "Active Card Number: " + currentBankAccount.CardNumber;

    var cardPinValue = document.getElementById('activeCardPin');
    cardPinValue.innerHTML = "Active Card Pin: " + currentBankAccount.Pin;

    var accountBalanceValue = document.getElementById('accountBalance');
    accountBalanceValue.innerHTML = "Balance: $" + parseFloat(currentBankAccount.Balance).toFixed(2);
}

function LoadBankAccounts(bankJson) {
    bankAccountJson = bankJson;

    var BankAccounts = JSON.parse(bankJson);

    var table = document.getElementById('BankAccountTable');

    BankAccounts.forEach(bankAccount => {
        var row = table.insertRow(1);
        var cell0 = row.insertCell(0);
        var cell1 = row.insertCell(1);
        var cell2 = row.insertCell(2);

        cell0.innerHTML = bankAccount.AccountNumber;

        var accountType = "Debit";

        if (bankAccount.AccountType === 0) {
            accountType = "Debit";
        }
        if (bankAccount.AccountType === 1) {
            accountType = "Credit";
        }
        if (bankAccount.AccountType === 2) {
            accountType = "Savings";
        }
        cell1.innerHTML = accountType;

        var button = document.createElement("BUTTON");
        var t = document.createTextNode("View");
        button.appendChild(t);
        button.classList.add('btn', 'btn-success');
        var index = BankAccounts.indexOf(bankAccount);
        button.onclick = function () {
            viewBankInfo(index);
        }
        cell2.appendChild(button);
    });
}

function viewBankInfo(index) {
    alt.emit('ViewBankInfo', index);
    window.location.href = "http://resource/files/bank/bankAccount.html";
}

function CloseBankHome() {
    alt.emit('closeBankHomePage');
}

function bankHomeLoaded() {
    alt.emit('bankHomePageLoaded');
}

function bankAccountLoaded() {
    alt.emit('bankAccountPageLoaded');
}

function accountWithdraw() {
    alt.emit('BankTransactionState', 1);
    window.setTimeout(() => {
        window.location.href = "http://resource/files/bank/transactionInput.html";
    }, 1000);
}

function accountDeposit() {
    alt.emit('BankTransactionState', 0);
    window.setTimeout(() => {
        window.location.href = "http://resource/files/bank/transactionInput.html";
    }, 1000);
}

function accountTransfer() {
    window.location.href = "http://resource/files/bank/transferAmount.html";
}

function requestTransactionHistory() {
    window.location.href = "http://resource/files/bank/transactionHistory.html";
}

function requestNewCard() {
    alt.emit('RequestNewBankCard');
}

function showCloseConfirm() {
    window.location.href = "http://resource/files/bank/confirmClosure.html";
}

function requestCloseAccount() {
    alt.emit('CloseBankAccount');
}

//#region Transaction History

function displayTransactions() {
    alt.emit('requestTransactions');
}

function ShowTransactionHistory(json) {
    var TransactionHistory = JSON.parse(json);

    var table = document.getElementById('TransactionTable');

    var header = table.createTHead();

    var headerRow = header.insertRow(0);

    var transactionTypeRow = headerRow.insertCell(0);

    var amountTypeRow = headerRow.insertCell(1);

    var transactionTimeRow = headerRow.insertCell(2);

    transactionTypeRow.innerHTML = "Transaction Type";

    amountTypeRow.innerHTML = "Transaction Amount";

    transactionTimeRow.innerHTML = "Transaction Time";

    TransactionHistory.forEach(transaction => {
        var tableRow = table.insertRow(1);

        var transactionTypeCell = tableRow.insertCell(0);

        var amountCell = tableRow.insertCell(1);

        var timeCell = tableRow.insertCell(2);

        var transactionType = 'ATM';

        switch (transaction.TransactionType) {
            case 0:
                transactionType = 'ATM';
                break;
            case 1:
                transactionType = 'Withdraw';
                break;
            case 2:
                transactionType = 'Deposit';
                break;
            case 3:
                transactionType = 'Transfer';

                headerRow.insertCell(3).innerHTML = "Send To";
                headerRow.insertCell(4).innerHTML = "Receive From";

                tableRow.insertCell(3).innerHTML = transaction.ReceiverAccount;
                tableRow.insertCell(4).innerHTML = transaction.SenderAccount;
                break;
            case 4:
                transactionType = 'Purchase';
                break;
            default:
                transactionType = 'ATM';
                break;
        }

        transactionTypeCell.innerHTML = transactionType;
        amountCell.innerHTML = '$' + transaction.Amount;

        var dateTime = new Date(Date.parse(transaction.TransactionTime));

        timeCell.innerHTML = dateTime.getDay() + "/" + dateTime.getMonth() + "/" + dateTime.getFullYear() + " - " + dateTime.getHours() + ":" + dateTime.getMinutes();
    });
}

//#endregion

//#region Transaction Input
function transactionInputLoaded() {
    alt.emit('requestTransactionState');
}

function transactionState(state) {
    // 0 = Deposit, 1 = Withdraw

    var numberInputLabel = document.getElementById('numberInputLabel');

    switch (state) {
        case 0:
            numberInputLabel.innerHTML = "Enter the amount you wish to deposit!";
            break;
        case 1:
            numberInputLabel.innerHTML = "Enter the amount you wish to withdraw!";
            break;
        default:
            numberInputLabel.innerHTML = "Enter the amount you wish to deposit!";
            break;
    }

    var transactionInputSubmit = document.getElementById('transactionInputSubmit');

    transactionInputSubmit.onclick = function () {
        var numberInput = document.getElementById('numberInput');

        var inputAmount = numberInput.value;

        console.log('Input Amount: ' + inputAmount);

        alt.emit('HandleBankTransaction', state, inputAmount);
    };
}
//#endregion

function transferInputSubmit() {
    var accountNumber = document.getElementById('accountInput').value;
    var amount = document.getElementById('amountInput').value;

    alt.emit('HandleBankTransfer', accountNumber, amount);
}

function setAsActive() {
    alt.emit('SetAsActive');
}