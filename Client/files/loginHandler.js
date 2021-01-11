import * as alt from 'alt-client';
import * as native from 'natives';
var loginView = null;
var disableMenu = false;
alt.onServer('showLogin', showLogin);
alt.onServer('closeLogin', closeLogin);
function loginRequest(user, password) {
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
    alt.setTimeout(() => {
        loginView.emit("hideLoginScreen");
        loginView.destroy();
        loginView = null;
        disableMenu = false;
    }, 1000);
}
alt.everyTick(() => {
    if (disableMenu) {
        native.disableControlAction(0, 199, true);
    }
    else {
        native.disableControlAction(0, 199, false);
    }
});
