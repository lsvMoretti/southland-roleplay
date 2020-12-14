import * as alt from 'alt-client';
import * as native from 'natives';

var helpWindow: alt.WebView = undefined;
var commandJson: string = undefined;
var currentOption: any = undefined;
var showAdminOption = false;
var showLawOption = false;

alt.onServer('helpMenu:ShowHelpMenu', showHelpMenu);

alt.onServer('helpMenu:ReturnAnim', returnAnimList);

function showHelpMenu(showAdmin: boolean, showLaw: boolean) {
    if (helpWindow !== undefined) {
        closeHelpMenu();
    }
    showAdminOption = showAdmin;
    showLawOption = showLaw;

    helpWindow = new alt.WebView("http://resource/files/help/helpMenu.html", false);
    helpWindow.focus();
    alt.showCursor(true);
    helpWindow.on('HelpMenu:helpTableLoaded', helpTableLoaded);
    helpWindow.on('HelpMenu:FetchCommands', fetchCommands);
    helpWindow.on('HelpMenu:CloseHelpMenu', closeHelpMenu);
    helpWindow.on('HelpMenu:MainPageLoaded', mainPageLoaded);
}

function mainPageLoaded() {
    helpWindow.emit('HelpMenu:ShowAdminOption', showAdminOption);
    helpWindow.emit('HelpMenu:ShowLawOption', showLawOption);
}

function fetchCommands(option: string) {
    if (option === 'anim') {
        currentOption = 'Animations';
    }
    if (option === 'character') {
        currentOption = 'Character';
    }
    if (option === 'bank') {
        currentOption = 'Bank';
    }
    if (option === 'faction') {
        currentOption = 'Faction';
    }
    if (option === 'focus') {
        currentOption = 'Focus';
    }
    if (option === 'job') {
        currentOption = 'Job';
    }
    if (option === 'phone') {
        currentOption = 'Phone';
    }
    if (option === 'vehicle') {
        currentOption = 'Vehicle';
    }
    if (option === 'chat') {
        currentOption = 'Chat';
    }
    if (option === 'admin') {
        currentOption = 'Admin';
    }
    if (option === 'property') {
        currentOption = 'Property';
    }
    if (option === 'law') {
        currentOption = 'Law';
    }

    alt.emitServer('HelpMenu:FetchCommands', option);
}

function returnAnimList(json: string) {
    commandJson = json;

    helpWindow.emit('HelpMenu:LoadListPage');
}

function helpTableLoaded() {
    helpWindow.emit('HelpMenu:ReceiveListData', currentOption, commandJson);
}

function closeHelpMenu() {
    if (helpWindow === undefined) return;

    commandJson = undefined;
    currentOption = undefined;
    helpWindow.destroy();
    helpWindow = undefined;
    alt.showCursor(false);
    alt.emitServer('helpMenu:CloseHelpMenu');
}