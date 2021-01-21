if ('alt' in window) {
    alt.on('factionList', loadFactionList);
    alt.on('loadFactionData', loadFactionData);
    alt.on('loadFactionRanks', loadFactionRanks);
    alt.on('faction:membersInfo', loadMemberInfo);
    alt.on('admin:faction:loadUpdateRankData', factionMemberLoadRankInfo);
}

function loadFactionList(json) {
    var factions = JSON.parse(json);

    var table = document.getElementById('FactionTable');

    factions.forEach(faction => {
        var row = table.insertRow(1);
        var cell0 = row.insertCell(0);
        var cell1 = row.insertCell(1);
        var cell2 = row.insertCell(2);

        cell0.innerHTML = faction.Id;
        cell1.innerHTML = faction.Name;

        var button = document.createElement("BUTTON");
        var t = document.createTextNode("View");
        button.appendChild(t);
        button.classList.add('btn', 'btn-success');
        var index = factions.indexOf(faction);
        button.onclick = function () {
            factionSelect(index);
        };
        cell2.appendChild(button);
    });
}

function factionSelect(index) {
    alt.emit('factionSelected', index);

    window.location.href = "http://resource/files/admin/factionView.html";
}

function factionViewLoaded() {
    alt.emit('factionViewLoaded');
}

function loadFactionData(json) {
    var selectedFaction = JSON.parse(json);

    var factionId = document.getElementById('factionId');
    var factionName = document.getElementById('factionName');

    factionId.innerText = 'Faction Id: ' + selectedFaction.Id;

    factionName.innerText = 'Faction Name: ' + selectedFaction.Name;
}

function viewFactionMembers() {
    alt.emit('fetchFactionMembers');
}

function viewFactionRanks() {
    window.location.href = "http://resource/files/admin/factionRanks.html";
}

function ClosePage() {
    alt.emit('closeFactionView');
}

function factionListLoaded() {
    alt.emit('factionListLoaded');
}

function factionRanksLoaded() {
    alt.emit('factionRanksLoaded');
}

function loadFactionRanks(rankJson) {
    var ranks = JSON.parse(rankJson);

    var table = document.getElementById('RankTable');

    ranks.forEach(rank => {
        var row = table.insertRow(1);
        var cell0 = row.insertCell(0);
        var cell1 = row.insertCell(1);
        var cell2 = row.insertCell(2);
        var cell3 = row.insertCell(3);
        var cell4 = row.insertCell(4);

        cell0.innerHTML = rank.Name;

        var index = ranks.indexOf(rank);

        var inviteButton = document.createElement("BUTTON");
        var inviteText = document.createTextNode("Invite");
        inviteButton.appendChild(inviteText);
        inviteButton.classList.add('btn', 'btn-success');
        inviteButton.onclick = function () {
            rankInvite(index);
        };
        cell1.appendChild(inviteButton);

        var promoteButton = document.createElement("BUTTON");
        var promoteText = document.createTextNode("Promote");
        promoteButton.appendChild(promoteText);
        promoteButton.classList.add('btn', 'btn-primary');
        promoteButton.onclick = function () {
            rankPromote(index);
        };
        cell2.appendChild(promoteButton);

        var addRankButton = document.createElement("BUTTON");
        var addRankText = document.createTextNode("Add Rank");
        addRankButton.appendChild(addRankText);
        addRankButton.classList.add('btn', 'btn-warning');
        addRankButton.onclick = function () {
            rankAddRank(index);
        };
        cell3.appendChild(addRankButton);

        var towButton = document.createElement("BUTTON");
        var towText = document.createTextNode("Tow");
        towButton.appendChild(towText);
        towButton.classList.add('btn', 'btn-danger');
        towButton.onclick = function () {
            rankTow(index);
        };
        cell4.appendChild(towButton);

        var deleteButton = document.createElement("BUTTON");
        var deleteText = document.createTextNode("Delete");
        deleteButton.appendChild(deleteText);
        deleteButton.classList.add('btn', 'btn-danger');
        deleteButton.onclick = function () {
            rankDelete(index);
        };
        cell5.appendChild(deleteButton);
    });
}

function rankInvite(index) {
    alt.emit('faction:adjustRankPerm', 'invite', index);
}

function rankPromote(index) {
    alt.emit('faction:adjustRankPerm', 'promote', index);
}

function rankAddRank(index) {
    alt.emit('faction:adjustRankPerm', 'addRank', index);
}

function rankTow(index) {
    alt.emit('faction:adjustRankPerm', 'towVehicle', index);
}

function rankDelete(index) {
    alt.emit('faction:adjustRankPerm', 'deleteRank', index);
}

function factionMembersLoaded() {
    alt.emit('faction:membersLoaded');
}

var memberJson = undefined;

function loadMemberInfo(json, rankJson) {
    memberJson = json;
    console.log(json);

    var memberList = JSON.parse(json);

    var ranks = JSON.parse(rankJson);

    var table = document.getElementById('MemberTable');

    memberList.forEach(member => {
        var row = table.insertRow(1);
        var cell0 = row.insertCell(0);
        var cell1 = row.insertCell(1);
        var cell2 = row.insertCell(2);
        var cell3 = row.insertCell(3);

        cell0.innerText = member.Name;

        var rank = ranks.find(obj => {
            return obj.Id === member.RankId;
        });

        if (rank.Name !== undefined || rank.Name !== null) {
            cell1.innerText = rank.Name;
        } else {
            cell1.innerText = "Not Found!";
        }

        var index = memberList.indexOf(member);
        var deleteButton = document.createElement("BUTTON");
        var deleteText = document.createTextNode("Remove");
        deleteButton.appendChild(deleteText);
        deleteButton.classList.add('btn', 'btn-danger');
        deleteButton.onclick = function () {
            memberDelete(index);
        };
        cell2.appendChild(deleteButton);

        var promoteButton = document.createElement("BUTTON");
        var rankText = document.createTextNode("Adjust Rank");
        promoteButton.appendChild(rankText);
        promoteButton.classList.add('btn', 'btn-success');
        promoteButton.onclick = function () {
            memberPromote(index);
        };
        cell3.appendChild(promoteButton);
    });
}

function memberDelete(index) {
    alt.emit('admin:faction:removeMember', index);
}

function memberPromote(index) {
    alt.emit('admin:faction:adjustMemberRank', index);
    window.location.href = "http://resource/files/admin/memberRank.html";
}

function factionMemberRankLoaded() {
    alt.emit('admin:faction:updateRankPageLoaded');
}

function factionMemberLoadRankInfo(rankJson) {
    var ranks = JSON.parse(rankJson);

    var table = document.getElementById('RankTable');

    ranks.forEach(rank => {
        var row = table.insertRow(1);
        var cell0 = row.insertCell(0);
        var cell1 = row.insertCell(1);

        cell0.innerHTML = rank.Name;

        var index = ranks.indexOf(rank);

        var inviteButton = document.createElement("BUTTON");
        var inviteText = document.createTextNode("Set Rank");
        inviteButton.appendChild(inviteText);
        inviteButton.classList.add('btn', 'btn-success');
        inviteButton.onclick = function () {
            setRank(index);
        };
        cell1.appendChild(inviteButton);
    });
}

function setRank(index) {
    alt.emit('admin:faction:setMemberRank', index);
}

function deleteFaction() {
    alt.emit('admin:faction:deleteFaction');
}

function factionTypeChange(elem) {
    document.getElementById('factionSubType').disabled = !elem.selectedIndex;
}

function factionCreateSubmit() {
    var factionName = document.getElementById("factionName").value;
    var factionType = document.getElementById("factionType").value;
    var factionSubType = document.getElementById("factionSubType").value;

    alt.emit('admin:faction:create', factionName, factionType, factionSubType);
}