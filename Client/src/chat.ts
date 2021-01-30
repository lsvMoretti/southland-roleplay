import * as alt from 'alt-client';
import * as game from 'natives';
import * as nameTags from './nametags';
import * as hudHandler from "./hud/hudHandler";

let buffer: any = [];

let loaded = false;
let opened = false;
let hidden = false;

let input = true;

let localStorage = alt.LocalStorage.get();

let fontSize: number = localStorage.get("Chat:FontSize");
let timeStamp: boolean = localStorage.get("Chat:TimestampToggle");

if (fontSize == null) {
    fontSize = 0;
    localStorage.set("Chat:FontSize", fontSize);
    localStorage.save();
}

if (timeStamp == null) {
    timeStamp = false;
    localStorage.set("Chat:TimestampToggle", timeStamp);
    localStorage.save();
}

alt.onServer("Chat:ChangeFontSize", (newFontSize: number) => {
    fontSize = newFontSize;
    localStorage.set("Chat:FontSize", fontSize);
    localStorage.save();
    if (view != null) {
        view.emit("changeFontSize", fontSize);
    }
});

alt.onServer('chat:EnableTimestamp', (toggle: boolean) => {
    timeStamp = toggle;
    localStorage.set("Chat:TimestampToggle", toggle);
    localStorage.save();

    view.emit('TimeStampToggle', toggle);
});

let view = new alt.WebView("http://resource/files/chat/index.html", false);

function addMessage(name: string, text: string) {
    if (name) {
        view.emit('addMessage', name, text);
    } else {
        if (!text.includes('fish')) {
            let newDate: Date = new Date();
            alt.log(newDate.getUTCDate() + '/' + newDate.getUTCMonth() + '/' + newDate.getUTCFullYear() + ' ' + newDate.getUTCHours() + ':' + newDate.getUTCMinutes() + ':' + newDate.getUTCSeconds() + ' - ' + text);
        }

        view.emit('addString', text);
    }
}

view.on('chatloaded',
    () => {
        for (const msg of buffer) {
            addMessage(msg.name, msg.text);
        }

        loaded = true;
        view.emit("changeFontSize", fontSize);
        view.emit("Chat:TimestampToggle", timeStamp);
    });

view.on('chatmessage',
    (text: string) => {
        alt.emitServer('sendChatMessage', text);

        alt.emit("ChatStatus", false);
        opened = false;
        alt.emitServer('ChatStatusChange', opened);
        alt.toggleGameControls(true);
    });

export function pushMessage(name: string, text: string) {
    if (!loaded) {
        buffer.push({ name, text });
    } else {
        addMessage(name, text);
    }
}

export function pushLine(text: string) {
    pushMessage(null, text);
}

export function IsChatOpen() {
    return opened;
}

alt.onServer('chatmessage', pushMessage);

alt.onServer('toggleChat', (state: boolean) => {
    input = state;
});

alt.onServer('showChat', (state: boolean) => {
    view.emit('hideChat', state);
});

alt.on('keyup', (key) => {
    if (!loaded)
        return;

    if (!input)
        return;

    if (!opened && key === 0x54 && alt.gameControlsEnabled()) {
        alt.emit("ChatStatus", true);
        opened = true;
        view.emit('openChat', false);
        view.focus();
        alt.emitServer('ChatStatusChange', opened);
        alt.toggleGameControls(false);
    }
    else if (!opened && key === 0xBF && alt.gameControlsEnabled()) {
        alt.emit("ChatStatus", true);
        opened = true;
        view.focus();
        view.emit('openChat', true);
        alt.emitServer('ChatStatusChange', opened);
        alt.toggleGameControls(false);
    }
    else if (opened && key === 0x1B) {
        alt.emit("ChatStatus", false);
        opened = false;
        view.emit('closeChat');
        alt.emitServer('ChatStatusChange', opened);
        alt.toggleGameControls(true);
    }

    if (key === 0x76) {
        hidden = !hidden;
        game.displayHud(!hidden);
        game.displayRadar(!hidden);
        view.emit('hideChat', hidden);
        alt.emit('hideChat', hidden);
        nameTags.ToggleNameTags(!hidden);
        hudHandler.toggleHud(!hidden);
    }
})

export default { pushMessage, pushLine };