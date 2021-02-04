import * as alt from 'alt-client';
import * as native from 'natives';
import * as camera from './camera';

var loginView: any = null;
var disableMenu: boolean = false;

alt.onServer('showLogin', showLogin);
alt.onServer('closeLogin', closeLogin);

function loginRequest(user: string, password: string) {
    alt.emitServer('recieveLoginRequest', user, password);
}

function showLogin() {
    loginView = new alt.WebView("http://resource/files/login/loginNew.html", false);
    loginView.on('loginRequest', loginRequest);
    loginView.on('LoginScreenLoaded', () => {
        alt.emitServer('LoginScreenLoaded');
    });
    loginView.focus();
    disableMenu = true;
}

function closeLogin() {
    loginView.emit("hideLoginScreen");
    alt.setTimeout(() => {
        loginView.destroy();
        loginView = null;
        disableMenu = false;
    }, 1000);
}

alt.setInterval(() => {
    if (disableMenu) {
        native.disableControlAction(0, 199, true);
    }
    else {
        native.disableControlAction(0, 199, false);
    }
}, 0);