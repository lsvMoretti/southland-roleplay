function pageLoaded()
{
    console.log("Online Players Page Loaded");
    alt.emit('requestOnlinePlayerList');
}

if('alt' in window){

    alt.on('SendOnlinePlayers', SendOnlinePlayers);

}

function SendOnlinePlayers(json)
{
    var playerList = JSON.parse(json);

    var playerTable = document.getElementById('playerTable');

    var onlineCount = document.getElementById('playerCount');

    onlineCount.textContent = playerList.length + " Online Players";

    playerList.forEach(player => {
        var row = playerTable.insertRow(1);
        var nameCell = row.insertCell(0);
        var idCell = row.insertCell(1);
        var pmCell = row.insertCell(2);

        nameCell.classList.add('text-center');
        idCell.classList.add('text-center');
        pmCell.classList.add('text-center');

        nameCell.innerHTML = player.Name;
        idCell.innerHTML = player.Id;
    
        var button = document.createElement("BUTTON");
        var t = document.createTextNode("PM");
        button.appendChild(t);
        button.classList.add('btn', 'btn-primary');
        button.onclick = function(){
            sendPlayerPm(player.Id);
        }
        pmCell.appendChild(button);
    });
}

function closePlayerList()
{
    alt.emit("ClosePlayerList");
}
