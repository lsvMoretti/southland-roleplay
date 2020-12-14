import * as alt from 'alt-client';
alt.onServer('DiscordLink:FetchUserId', LinkFetchUserId);
function LinkFetchUserId() {
    var userId = alt.Discord.currentUser.id;
    alt.emitServer('LinKDiscord:SendUserId', userId);
}
