import * as alt from 'alt-client';
import * as native from 'natives';
import * as extensions from './extensions';
import * as playerHandler from './player/PlayerHandler';

var roomLabels = new Array();

alt.onServer('Motel:ClearRoomLabels', () => {
    roomLabels = new Array();
});

alt.onServer('Motel:OnMotelAdded', OnMotelAdded);

function OnMotelAdded(motelJson:string){
    var motel = JSON.parse(motelJson);

    var motelRoomList = JSON.parse(motel.RoomList);
    
    motelRoomList.forEach((motelRoom:any) => {
        motelRoom.pos = new alt.Vector3(motelRoom.PosX, motelRoom.PosY, motelRoom.PosZ);

        roomLabels.push(motelRoom);
    });


}

alt.setInterval(() => {
    ProcessMotelRoomLabels();
}, 0)

function ProcessMotelRoomLabels(){
    if(roomLabels.length == 0) return;

    roomLabels.forEach(roomLabel => {
        DisplayRoomLabel(roomLabel);

    });
}

function DisplayRoomLabel(roomLabel:any){
    let playerPos = alt.Player.local.pos;
    var range:number = 5;

    if(extensions.Distance(playerPos, roomLabel.pos) > range) return;

    const [bol, _x, _y] = native.getScreenCoordFromWorldCoord(roomLabel.PosX, roomLabel.PosY, roomLabel.PosZ, null, null);
    const camCord = native.getFinalRenderedCamCoord();
    const dist = native.getDistanceBetweenCoords(camCord.x, camCord.y, camCord.z, roomLabel.PosX, roomLabel.PosY, roomLabel.PosZ, true);

    let scale = (4.00001 / dist) * 0.3;
    if (scale > 0.2)
    {
        scale = 0.2;
    }

    const fov = (1 / native.getGameplayCamFov()) * 100;
    
    scale = scale * fov;

    if(bol){
        native.setTextScale(scale, scale);
        native.setTextFont(4);
        native.setTextProportional(true);
        native.setTextColour(206, 111, 16, 200);
        native.setTextDropshadow(0, 0, 0, 0, 255);
        native.setTextEdge(2, 0, 0, 0, 150);
        native.setTextDropShadow();
        native.setTextOutline();
        native.setTextCentre(true);
        native.beginTextCommandDisplayText("STRING");
        if(roomLabel.OwnerId > 0)
        {
            native.addTextComponentSubstringPlayerName("Room " + roomLabel.Id);
        }
        else 
        {
            native.addTextComponentSubstringPlayerName("Room " + roomLabel.Id + "\n /rentroom \nRent Fee: $"+roomLabel.Value+".00");
        }
        native.endTextCommandDisplayText(_x, _y + 0.025, 0);
    }

}