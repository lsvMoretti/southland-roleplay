function fetchCommands(option) {
    console.log(option);
    alt.emit('HelpMenu:FetchCommands', option);
}

if ('alt' in window) {
    alt.on('HelpMenu:LoadListPage', loadListPage);
    alt.on('HelpMenu:ReceiveListData', receiveListData);
    alt.on('HelpMenu:ShowAdminOption', showAdminOption);
    alt.on('HelpMenu:ShowLawOption', showLawOption);
    alt.on('HelpMenu:ShowHelperOption', showHelperOption);
}

function showHelperOption(option){
    if(option === undefined || option === false) return;
    
    let helperDiv = document.getElementById('helperDiv');
    helperDiv.classList.remove('hideme');
}

function showLawOption(option) {
    if (option === undefined || option === false) return;

    var lawDiv = document.getElementById('lawDiv');
    console.log('Showing Law');
    lawDiv.classList.remove('hideme');
}

function showAdminOption(option) {
    if (option === undefined || option === false) return;

    var adminDiv = document.getElementById('adminDiv');
    console.log('Showing Admin')
    adminDiv.classList.remove('hideme');
}

function loadListPage() {
    window.location.href = "http://resource/files/help/helpTable.html";
}

function helpTableLoaded() {
    alt.emit('HelpMenu:helpTableLoaded');
}

function receiveListData(name, json) {
    var heading = document.getElementById('heading');

    heading.innerText = name + ' Commands';

    var table = document.getElementById('ResultTable');

    var commandList = JSON.parse(json);

    commandList.forEach(command => {
        var row = table.insertRow(1);
        //Command
        var cell0 = row.insertCell(0);
        //Description
        var cell1 = row.insertCell(1);

        cell0.innerText = '/' + command.Command;
        cell1.innerText = command.Description;
    });
}

function CloseHelpMenu() {
    alt.emit('HelpMenu:CloseHelpMenu');
}

function MainPageLoaded() {
    alt.emit('HelpMenu:MainPageLoaded');
}