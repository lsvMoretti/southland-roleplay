import * as alt from 'alt-client';
import * as native from 'natives';
alt.onServer('phone:showCallNumber', showCallNumber);
var callNumberView = undefined;
function showCallNumber() {
    if (callNumberView !== undefined) {
        closeCallNumber();
    }
    alt.showCursor(true);
    callNumberView = new alt.WebView('http://resource/files/phone/callNumber.html', false);
    callNumberView.focus();
    callNumberView.on('phone:CallNumber', handleCallNumber);
}
function handleCallNumber(number) {
    alt.emitServer('phone:handleCallNumber', number);
    closeCallNumber();
}
function closeCallNumber() {
    if (callNumberView !== undefined) {
        alt.setTimeout(() => {
            callNumberView.destroy();
            callNumberView = undefined;
        }, 1000);
        alt.showCursor(false);
    }
}
alt.onServer('phone:startAudioPhoneCall', startAudioPhoneCall);
function startAudioPhoneCall(dialTone) {
    if (dialTone === 0) {
        native.playPedRingtone("Dial_and_Remote_Ring", alt.Player.local.scriptID, true);
    }
    if (dialTone === 1) {
        native.playPedRingtone("Remote_Ring", alt.Player.local.scriptID, true);
    }
}
alt.onServer('phone:stopPhoneRinging', stopPhoneRinging);
function stopPhoneRinging() {
    native.stopPedRingtone(alt.Player.local.scriptID);
}
var smsPage = undefined;
alt.onServer('phone:showSMSPage', showSMSPage);
alt.onServer('phone:showSMSContact', showSMSContact);
function showSMSContact() {
    if (smsPage !== undefined) {
        closeSmsPage();
    }
    alt.showCursor(true);
    smsPage = new alt.WebView('http://resource/files/phone/smsContact.html', false);
    smsPage.focus();
    smsPage.on('phone:SmsContact', handleSmsContact);
}
function handleSmsContact(message) {
    alt.emitServer('phone:smsExistingContact', message);
    closeSmsPage();
}
function showSMSPage() {
    if (smsPage !== undefined) {
        closeSmsPage();
    }
    alt.showCursor(true);
    smsPage = new alt.WebView('http://resource/files/phone/smsNumber.html', false);
    smsPage.focus();
    smsPage.on('phone:SmsNumber', handleSmsNumber);
}
function closeSmsPage() {
    if (smsPage !== undefined) {
        alt.setTimeout(() => {
            smsPage.destroy();
            smsPage = undefined;
        }, 1000);
        alt.showCursor(false);
    }
}
function handleSmsNumber(number, message) {
    alt.emitServer('phone:smsNewNumber', number, message);
    closeSmsPage();
}
var addContactPage = undefined;
alt.onServer('phone:showAddContactPage', showAddContactPage);
function showAddContactPage() {
    if (addContactPage !== undefined) {
        closeAddContactPage();
    }
    alt.showCursor(true);
    addContactPage = new alt.WebView('http://resource/files/phone/addContact.html', false);
    addContactPage.focus();
    addContactPage.on('phone:addContact', handleAddContact);
}
function closeAddContactPage() {
    if (addContactPage !== undefined) {
        alt.setTimeout(() => {
            addContactPage.destroy();
            addContactPage = undefined;
        }, 1000);
        alt.showCursor(false);
    }
}
function handleAddContact(name, number) {
    alt.emitServer('phone:handleAddContact', name, number);
    closeAddContactPage();
}
alt.onServer('phone:playSmsTone', playSmsTone);
function playSmsTone() {
    native.playSoundFrontend(-1, "Menu_Accept", "Phone_SoundSet_Default", true);
}
