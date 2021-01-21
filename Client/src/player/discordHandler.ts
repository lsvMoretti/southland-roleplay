import * as alt from 'alt-client';
import * as native from 'natives';
import * as notyHandler from '../noty/notyHandler';

alt.onServer('DiscordLink:FetchUserId', LinkFetchUserId);

function LinkFetchUserId() {
    const userId = alt.Discord.currentUser.id;

    if (userId === null || userId === undefined) {
        notyHandler.SendNotification("You must run Discord & alt:V as an admin", 6000, "error", "topCenter");
        return;
    }

    alt.emitServer('LinKDiscord:SendUserId', userId);
}