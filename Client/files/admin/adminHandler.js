import * as alt from 'alt-client';
import * as native from 'natives';
var vehicleListString = undefined;
var adminDealershipView = undefined;
var adminDealershipName = undefined;
var editVehicleIndex = undefined;
alt.onServer('teleportToWaypoint', () => {
    if (!native.isWaypointActive())
        return alt.log('Waypoint not defined');
    const z = 1000;
    const { scriptID: player } = alt.Player.local;
    const waypoint = native.getFirstBlipInfoId(8);
    const coords = native.getBlipInfoIdCoord(waypoint);
    native.freezeEntityPosition(player, true);
    native.startPlayerTeleport(player, coords.x, coords.y, z, 0, true, true, true);
    const interval = alt.setInterval(() => {
        if (native.hasPlayerTeleportFinished(player)) {
            const ground = native.getEntityHeightAboveGround(player);
            native.startPlayerTeleport(player, coords.x, coords.y, z - ground, 0, true, true, true);
            native.freezeEntityPosition(player, false);
            alt.clearInterval(interval);
        }
    }, 100);
});
alt.onServer('RockstarEditor:Toggle', (toggle) => {
    if (toggle) {
        native.activateRockstarEditor();
        native.setPlayerRockstarEditorDisabled(false);
        let interval = alt.setInterval(() => {
            if (native.isScreenFadedOut()) {
                native.doScreenFadeIn(1000);
                alt.clearInterval(interval);
            }
        }, 1000);
    }
    else {
        native.setPlayerRockstarEditorDisabled(true);
    }
});
alt.onServer('admin:showEditDealershipVehicles', showEditDealershipVehicles);
function showEditDealershipVehicles(dealerName, vehicleListJson) {
    vehicleListString = vehicleListJson;
    adminDealershipName = dealerName;
    if (adminDealershipView !== undefined) {
        closeDealershipVehiclePage();
    }
    alt.showCursor(true);
    adminDealershipView = new alt.WebView('http://resource/files/admin/dealershipVehicles.html', false);
    adminDealershipView.focus();
    adminDealershipView.on('admin:dealership:dealershipVehicleLoaded', dealershipVehiclePageLoaded);
    adminDealershipView.on('admin:dealership:editSelectedVehicle', editSelectedDealershipVehicle);
    adminDealershipView.on('admin:dealership:ClosePage', closeDealershipVehiclePage);
    adminDealershipView.on('admin:dealership:addNewDealershipVehicle', addNewDealershipVehicle);
    adminDealershipView.on('admin:dealership:editPageLoaded', editPageLoaded);
    adminDealershipView.on('admin:dealership:editDealershipVehicle', editDealershipVehicle);
    adminDealershipView.on('admin:dealership:removeDealershipVehicle', removeDealershipVehicle);
}
function editPageLoaded() {
    adminDealershipView.emit('admin:dealership:editPageInfo', editVehicleIndex, vehicleListString);
}
function addNewDealershipVehicle(vehicleName, vehicleModel, vehiclePrice) {
    alt.emitServer('admin:dealership:createNewDealershipVehicle', vehicleName, vehicleModel, vehiclePrice);
    closeDealershipVehiclePage();
}
function editSelectedDealershipVehicle(index) {
    editVehicleIndex = index;
}
function dealershipVehiclePageLoaded() {
    adminDealershipView.emit('admin:dealership:loadVehicleList', adminDealershipName, vehicleListString);
}
function closeDealershipVehiclePage() {
    alt.log('function Called');
    if (adminDealershipView !== undefined) {
        alt.log('not defined');
        alt.setTimeout(() => {
            adminDealershipView.destroy();
        }, 1100);
        native.freezeEntityPosition(alt.Player.local.scriptID, false);
        alt.showCursor(false);
        adminDealershipView = undefined;
        alt.emitServer('admin:dealership:callClosePage');
    }
}
function editDealershipVehicle(vehicleName, vehicleModel, vehiclePrice) {
    alt.emitServer('admin:dealership:editDealershipVehicle', editVehicleIndex, vehicleName, vehicleModel, vehiclePrice);
    closeDealershipVehiclePage();
}
function removeDealershipVehicle() {
    alt.emitServer('admin:dealership:removeDealershipVehicle', editVehicleIndex.toString());
    closeDealershipVehiclePage();
}
var factionView = undefined;
var factionJson = undefined;
var factionSelected = undefined;
var factionMembers = undefined;
var factionMemberEditIndex = undefined;
alt.onServer('loadFactionList', loadFactionList);
alt.onServer('closeFactionPage', closeFactionView);
alt.onServer('admin:faction:factionMembers', returnFactionMembers);
alt.onServer('admin:faction:showCreatePage', showFactionCreate);
function returnFactionMembers(json) {
    factionMembers = json;
    alt.setTimeout(() => {
        factionView.destroy();
    }, 1000);
    factionView = undefined;
    factionView = new alt.WebView('http://resource/files/admin/factionMembers.html', false);
    factionView.focus();
    factionView.on('faction:membersLoaded', () => {
        var factions = JSON.parse(factionJson);
        var selectedFaction = factions[factionSelected];
        factionView.emit('faction:membersInfo', factionMembers, selectedFaction.RanksJson);
    });
    factionView.on('closeFactionView', closeFactionView);
    factionView.on('admin:faction:removeMember', (index) => {
        alt.emitServer('admin:faction:removeMemberFromFaction', index.toString());
    });
    factionView.on('admin:faction:adjustMemberRank', (index) => {
        factionMemberEditIndex = index;
    });
    factionView.on('admin:faction:updateRankPageLoaded', () => {
        var factions = JSON.parse(factionJson);
        var selectedFaction = factions[factionSelected];
        factionView.emit('admin:faction:loadUpdateRankData', selectedFaction.RanksJson);
    });
    factionView.on('admin:faction:setMemberRank', (rankIndex) => {
        alt.emitServer('admin:faction:adjustMemberRank', factionMemberEditIndex.toString(), factionSelected.toString(), rankIndex.toString());
    });
}
function loadFactionList(json) {
    if (factionView !== undefined) {
        closeFactionView();
    }
    factionJson = json;
    alt.showCursor(true);
    factionView = new alt.WebView('http://resource/files/admin/factionList.html', false);
    factionView.focus();
    factionView.on('factionListLoaded', () => {
        factionView.emit('factionList', factionJson);
    });
    factionView.on('closeFactionView', closeFactionView);
    factionView.on('factionSelected', (index) => {
        factionSelected = index;
    });
    factionView.on('factionViewLoaded', factionViewLoaded);
    factionView.on('requestFactionRanks', requestFactionRanks);
    factionView.on('factionRanksLoaded', factionRanksLoaded);
    factionView.on('faction:adjustRankPerm', factionAdjustRankPerm);
    factionView.on('fetchFactionMembers', fetchFactionMembers);
    factionView.on('admin:faction:deleteFaction', factionRemove);
}
function factionRemove() {
    var factions = JSON.parse(factionJson);
    var factionId = factions[factionSelected].Id;
    alt.emitServer('admin:faction:removeFaction', factionId.toString());
}
function fetchFactionMembers() {
    var factions = JSON.parse(factionJson);
    var factionId = factions[factionSelected].Id;
    alt.emitServer('admin:faction:fetchMembers', factionId.toString());
}
function factionAdjustRankPerm(perm, index) {
    alt.emitServer('admin:faction:adjustRankPerm', perm.toString(), factionSelected.toString(), index.toString());
}
function factionRanksLoaded() {
    if (factionSelected === undefined)
        return;
    var factions = JSON.parse(factionJson);
    var selectedFaction = factions[factionSelected];
    factionView.emit('loadFactionRanks', selectedFaction.RanksJson);
}
function requestFactionRanks() {
    if (factionSelected === undefined)
        return;
}
function factionViewLoaded() {
    if (factionSelected === undefined)
        return;
    var factions = JSON.parse(factionJson);
    var selectedFaction = factions[factionSelected];
    factionView.emit('loadFactionData', JSON.stringify(selectedFaction));
}
function closeFactionView() {
    if (factionView === undefined)
        return;
    alt.setTimeout(() => {
        factionView.destroy();
    }, 1000);
    factionView = undefined;
    alt.showCursor(false);
    alt.emitServer('factionViewClosed');
}
function showFactionCreate() {
    if (factionView !== undefined) {
        factionView.destroy();
        factionView = undefined;
    }
    alt.showCursor(true);
    factionView = new alt.WebView('http://resource/files/admin/factionCreate.html', false);
    factionView.focus();
    factionView.on('closeFactionView', closeFactionView);
    factionView.on('admin:faction:create', (factionName, factionType, factionSubType) => {
        alt.emitServer('admin:faction:createFaction', factionName.toString(), factionType.toString(), factionSubType.toString());
    });
}
var AdminMode = false;
alt.onServer('EnabledAdminDuty', (adminMode) => {
    AdminMode = adminMode;
    var scriptID = alt.Player.local.scriptID;
    if (!adminMode) {
        native.setEntityCanBeDamaged(scriptID, true);
        native.setEntityInvincible(scriptID, false);
    }
});
alt.everyTick(() => {
    var scriptID = alt.Player.local.scriptID;
    if (AdminMode) {
        native.setEntityCanBeDamaged(scriptID, false);
        native.setEntityInvincible(scriptID, true);
        native.setEntityHealth(scriptID, 200, 0);
    }
});
