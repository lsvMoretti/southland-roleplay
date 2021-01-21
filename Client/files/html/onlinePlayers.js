function pageLoaded() {
    console.log("Online Players Page Loaded");
    alt.emit('requestOnlinePlayerList');
}

if ('alt' in window) {
    alt.on('SendOnlinePlayers', SendOnlinePlayers);
}

function SendOnlinePlayers(json) {
    var playerList = JSON.parse(json);

    var playerTable = document.getElementById('playerTable');

    var onlineCount = document.getElementById('playerCount');

    onlineCount.textContent = playerList.length + " Online Players";

    playerList.forEach(player => {
        var row = playerTable.insertRow(1);
        var nameCell = row.insertCell(0);
        var idCell = row.insertCell(1);
        var pingCell = row.insertCell(2);

        nameCell.classList.add('text-center');
        idCell.classList.add('text-center');
        pingCell.classList.add('text-center');

        nameCell.innerHTML = player.Name;
        idCell.innerHTML = player.Id;
        pingCell.innerHTML = player.Ping;
    });
}

function closePlayerList() {
    alt.emit("ClosePlayerList");
}