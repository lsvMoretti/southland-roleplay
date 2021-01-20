import * as alt from 'alt-client';
var tfaWebView = undefined;
var manualEntryCode = undefined;
alt.onServer('TFA:ShowWindow', showTwoFactorSetup);
function showTwoFactorSetup(manualCode, qrUri) {
    manualEntryCode = manualCode;
    tfaWebView = new alt.WebView('http://resource/files/authenticate/twoFactorSetup.html', false);
    tfaWebView.focus();
    alt.showCursor(true);
    tfaWebView.on('TFA:PageLoaded', () => {
        tfaWebView.emit('TFA:SendData', manualCode, qrUri);
    });
    tfaWebView.on('TFA:ClosePage', () => {
        alt.setTimeout(() => {
            tfaWebView.destroy();
            tfaWebView = undefined;
        }, 1000);
        manualEntryCode = undefined;
        alt.showCursor(false);
        alt.emitServer('TFA:ClosePage');
    });
    tfaWebView.on('TFA:Finish', () => {
        alt.setTimeout(() => {
            tfaWebView.destroy();
            tfaWebView = undefined;
        }, 1000);
        manualEntryCode = undefined;
        alt.showCursor(false);
        alt.emitServer('TFA:Complete');
    });
}
alt.onServer('2FA:GetInput', showGetInput);
function showGetInput() {
    tfaWebView = new alt.WebView('http://resource/files/authenticate/twoFactorInput.html', false);
    tfaWebView.focus();
    alt.showCursor(true);
    tfaWebView.on('TFA:CloseInputPage', () => {
        alt.emitServer('TFA:CloseInputPage');
    });
    tfaWebView.on('TFA:SendCodeInput', (input) => {
        alt.emitServer('TFA:SendCodeInput', input.toString());
    });
}
alt.onServer('2FA:InvalidCode', onInvalidCode);
function onInvalidCode() {
    if (tfaWebView !== undefined) {
        tfaWebView.emit("InvalidCode");
    }
}
alt.onServer('2FA:CloseInput', onCloseInput);
function onCloseInput() {
    alt.setTimeout(() => {
        tfaWebView.destroy();
        tfaWebView = undefined;
    }, 1000);
    alt.showCursor(false);
    alt.emitServer('TFA:ClosePage');
}
