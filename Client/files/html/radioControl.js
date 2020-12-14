function radioControlLoaded()
{
    alt.emit('radioControlPageLoaded');
}

if('alt' in window){
    alt.on('loadStationTable', loadStationTable);
}

var stationListJson = undefined;

function loadStationTable(json)
{
    stationListJson = json;
    console.log(json);
    var stationList = JSON.parse(json);

    var table = document.getElementById('stationTable');

    stationList.forEach(station => {
        var row = table.insertRow(1);
        var nameCell = row.insertCell(0);
        var playCell = row.insertCell(1);

        nameCell.classList.add('text-center');
        playCell.classList.add('text-center');

        nameCell.innerHTML = station.StationName;

        var button = document.createElement("BUTTON");
        var t = document.createTextNode("Play");
        button.appendChild(t);
        button.classList.add('btn', 'btn-warning');
        button.onclick = function(){
            playMusicStream(station.StationName);
        }
        playCell.appendChild(button);
    })
}

function playMusicStream(stationName)
{
    alt.emit('SelectedMusicStream', stationName);
}

function stopMusicStream()
{
    alt.emit('StopMusicStream');
}
